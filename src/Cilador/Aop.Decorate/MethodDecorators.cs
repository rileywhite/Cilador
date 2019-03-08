/***************************************************************************/
// Copyright 2013-2019 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Cilador.Aop.Transform;
using Cilador.Aop.Core;
using Cilador.Clone;
using Cilador.Graph.Factory;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Aop.Decorate
{
    public abstract class MethodDecoratorBase : IConceptWeaver<MethodDefinition>
    {
        public MethodDecoratorBase(
            IAssemblyResolver resolver, CilGraphGetter graphGetter, Delegate sourceDecorationDelegate, Func<string, string> decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(sourceDecorationDelegate != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.SourceDecorationAssembly != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);

            // TODO could probably allow instance decorations for instance targets without any real trouble since required state would be cloned over
            if (!sourceDecorationDelegate.Method.IsStatic)
            {
                throw new ArgumentException("Decoration methods must be static.", nameof(sourceDecorationDelegate));
            }

            this.Resolver = resolver;
            this.SourceDecorationAssembly = resolver.Resolve(AssemblyNameReference.Parse(sourceDecorationDelegate.Method.Module.Assembly.FullName));
            this.SourceDecoration = this.SourceDecorationAssembly.MainModule.ImportReference(sourceDecorationDelegate.Method).Resolve();

            this.GraphGetter = graphGetter;
            this.TargetDecorationNameGenerator = decorationNameGenerator ?? (name => name);
        }

        ~MethodDecoratorBase()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.SourceDecorationAssembly?.Dispose();
                this.SourceDecorationAssembly = null;

                this.Resolver?.Dispose();
                this.Resolver = null;
            }
        }

        public IAssemblyResolver Resolver { get; private set; }
        public AssemblyDefinition SourceDecorationAssembly { get; private set; }

        public CilGraphGetter GraphGetter { get; }
        public MethodDefinition SourceDecoration { get; }
        public Func<string, string> TargetDecorationNameGenerator { get; }

        public void Weave(MethodDefinition decoratedMethod)
        {
            var targetDecorationName = this.TargetDecorationNameGenerator(decoratedMethod.Name);
            var isTargetReplacedByDecorator = targetDecorationName == decoratedMethod.Name;

            var (targetDecoration, areArgsAutoForwarded, isStaticToInstanceClone) = CloneDecorationIntoTargetLocation(
                decoratedMethod,
                targetDecorationName,
                isTargetReplacedByDecorator);

            // if the names are the same, then we're replacing the target, so move it to a randomly named location
            if (isTargetReplacedByDecorator)
            {
                decoratedMethod.Name = $"cilador_{decoratedMethod.Name}_{Guid.NewGuid().ToString("N")}";
            }

            RedirectForwardingCall(decoratedMethod, targetDecoration, areArgsAutoForwarded, isStaticToInstanceClone);

            var redirectMethodCallsLoom = new Loom();

            using (var transform = new TransformAdvisor<MethodDefinition>(
                        method =>
                        {
                            if (isTargetReplacedByDecorator)
                            {
                                RedirectDecoratedMethodCallsToTargetDecoration(method, decoratedMethod, targetDecoration);
                            }
                        }))
            {
                redirectMethodCallsLoom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(new PointCut<MethodDefinition>(m => m.HasBody), transform));

                redirectMethodCallsLoom.Weave(decoratedMethod.Module.Assembly, decoratedMethod.Module.AssemblyResolver, this.GraphGetter);
            }
        }

        private (MethodDefinition TargetDecoration, bool AreArgsAutoForwarded, bool IsStaticToInstanceClone) CloneDecorationIntoTargetLocation(
            MethodDefinition decoratedMethod,
            string targetDecorationName,
            bool isTargetReplacedByDecorator)
        {
            var sourceDecorationGraph = this.GraphGetter.Get(this.SourceDecoration);
            var cloningContext = new CloningContext(sourceDecorationGraph, this.SourceDecoration.DeclaringType, decoratedMethod.DeclaringType);
            var areArgsAutoForwarded = isTargetReplacedByDecorator && this.CanArgsBeAutoForwarded(decoratedMethod);

            if (isTargetReplacedByDecorator && !areArgsAutoForwarded)
            {
                this.EnsureParametersMatch(decoratedMethod, cloningContext);
            }

            var targetDecoration = default(MethodDefinition);
            bool isStaticToInstanceClone = false;

            void updateAndCaptureDecorationTargetTransform(ICloner<object, object> cloner)
            {
                targetDecoration = (MethodDefinition)cloner.Target;
                targetDecoration.Name = targetDecorationName;
                targetDecoration.Attributes = decoratedMethod.Attributes;

                isStaticToInstanceClone = !decoratedMethod.IsStatic;
                if (isStaticToInstanceClone) { ((MethodSignatureCloner)cloner).IsStaticToInstanceClone = true; }

                if (areArgsAutoForwarded)
                {
                    // modify target to accept args
                    var voidReference = cloningContext.TargetModule.ImportReference(typeof(void));
                    foreach (var parameter in decoratedMethod.Parameters)
                    {
                        var decorationParameter =
                            new ParameterDefinition(
                                parameter.Name,
                                parameter.Attributes,
                                voidReference);

                        cloningContext.CopyDetails(decorationParameter, parameter);

                        targetDecoration.Parameters.Add(decorationParameter);
                    }
                }
            }

            cloningContext.InlineTransforms.Add((s => s == this.SourceDecoration, updateAndCaptureDecorationTargetTransform));
            cloningContext.Execute();


            Contract.Assert(targetDecoration != null);
            return (targetDecoration, areArgsAutoForwarded, isStaticToInstanceClone);
        }

        private void EnsureParametersMatch(MethodDefinition decoratedMethod, CloningContext cloningContext)
        {
            // capture original values so that can be replaced temporarily
            var sourceDecorationName = this.SourceDecoration.Name;
            var sourceDecorationAttributes = this.SourceDecoration.Attributes;

            try
            {
                // update decoration values to allow the target signature check to work
                this.SourceDecoration.Name = decoratedMethod.Name;
                this.SourceDecoration.Attributes = decoratedMethod.Attributes;
                if (!decoratedMethod.SignatureEquals(this.SourceDecoration, cloningContext))
                {
                    throw new InvalidOperationException(
                        "Decoration signature has parameters that do not match target signature, so it cannot decorate a target with the same name. You may change the decoration signature to fully match the target signature, change the decoration name to be distinct from the target name, or make the decoration parameterless to take advantage of argument forwarding.");
                }
            }
            finally
            {

                // return original values to avoid side-effects
                this.SourceDecoration.Name = sourceDecorationName;
                this.SourceDecoration.Attributes = sourceDecorationAttributes;
            }
        }

        private bool CanArgsBeAutoForwarded(MethodDefinition decoratedMethod) => decoratedMethod.HasParameters && !this.SourceDecoration.HasParameters;

        private static void RedirectDecoratedMethodCallsToTargetDecoration(
            MethodDefinition inMethod,
            MethodDefinition decoratedMethod,
            MethodDefinition targetDecoration)
        {
            // if replacing rather than adding a separately named decorator, then redirect all calls to the decorator
            foreach (var instruction in inMethod.Body.Instructions.Where(i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.Operand is MethodReference && ((MethodReference)i.Operand).FullName == decoratedMethod.FullName).ToArray())
            {
                instruction.Operand = targetDecoration;
            }
        }

        private static void RedirectForwardingCall(MethodDefinition decoratedMethod, MethodDefinition targetDecoration, bool areArgsAutoForwarded, bool isStaticToInstanceClone)
        {
            foreach (var instruction in
                targetDecoration.Body.Instructions.Where(
                    i => i.OpCode == OpCodes.Call && i.Operand is MethodReference &&
                    ((MethodReference)i.Operand).Name == nameof(Forwarders.ForwardToOriginalAction)).ToArray())
            {
                var ilProcessor = targetDecoration.Body.GetILProcessor();
                var decoratedMethodReference = decoratedMethod.Module.ImportReference(decoratedMethod);
                var replacementInstruction = ilProcessor.Create(instruction.OpCode, decoratedMethodReference);
                ilProcessor.Replace(instruction, replacementInstruction);

                //instruction.Operand = decoratedMethod.Module.ImportReference(decoratedMethod);

                if (areArgsAutoForwarded && decoratedMethod.HasParameters)
                {
                    throw new NotImplementedException("In progress...");
                    // implies no arguments are on the forwarded version of the method call
                    // this means we simply add ldarg instructions

                    // instance <--> static argument translation happens during cloning
                    // this pointer handling (i.e. adding a new ldarg.0) is done in later by logic in this class

                    var newInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                    ilProcessor.InsertBefore(instruction, newInstruction);

                    var parameterCount = (ushort)decoratedMethod.Parameters.Count;

                    if (parameterCount >= 1) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_0)); }
                    if (parameterCount >= 2) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_1)); }
                    if (parameterCount >= 3) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_2)); }
                    if (parameterCount >= 4) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_3)); }

                    for (ushort i = 4; i < parameterCount && i <= byte.MaxValue; ++i)
                    {
                        ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_S, i));
                    }

                    for (int i = byte.MaxValue + 1; i < parameterCount; ++i)
                    {
                        ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg, i));
                    }
                }

                if (isStaticToInstanceClone)
                {
                    AddThisPointerToTargetDecorationCall(targetDecoration, replacementInstruction);
                }
            }
        }

        private static void AddThisPointerToTargetDecorationCall(MethodDefinition potentialCallingMethod, Instruction potentialCallingInstruction)
        {
            if (!potentialCallingInstruction.TryGetFirstInstructionOfMethodCall(out var firstArgInstruction)) { /* maybe needs error handling? */ return; }
            if (firstArgInstruction.OpCode == OpCodes.Ldarg_0) { /* this pointer already being sent to the method */ return; }

            var ilProcessor = potentialCallingMethod.Body.GetILProcessor();
            var newInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
            ilProcessor.InsertBefore(firstArgInstruction, newInstruction);
        }
    }

    #region Concrete Method Decorator Types

    public class ActionDecorator : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class ActionDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : MethodDecoratorBase
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    public class FuncDecorator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : MethodDecoratorBase
    {
        public FuncDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> decoration,
            Func<string, string> decorationNameGenerator = null)
            : base(resolver, graphGetter, decoration, decorationNameGenerator)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceDecoration != null);
            Contract.Ensures(this.TargetDecorationNameGenerator == null);
        }
    }

    #endregion
}
