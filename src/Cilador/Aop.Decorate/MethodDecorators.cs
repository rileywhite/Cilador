﻿/***************************************************************************/
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
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            Delegate decoration,
            Func<string, string> decorationNameGenerator = null)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(decoration != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);

            this.Resolver = resolver;
            this.GraphGetter = graphGetter;
            this.Decoration = decoration;
            this.DecorationNameGenerator = decorationNameGenerator ?? (sourceName => sourceName);
        }

        public IAssemblyResolver Resolver { get; }
        public CilGraphGetter GraphGetter { get; }
        public Delegate Decoration { get; }
        public Func<string, string> DecorationNameGenerator { get; }

        public void Weave(MethodDefinition target)
        {
            var targetAssembly = target.Module.Assembly;
            var decorationName = this.DecorationNameGenerator(target.Name);
            var isTargetReplacedByDecorator = decorationName == target.Name;

            var decorationTarget = CloneDecorationIntoTargetLocation(
                target,
                decorationName,
                isTargetReplacedByDecorator,
                out var areArgsAutoForwarded);

            var redirectMethodCallsLoom = new Loom();
            redirectMethodCallsLoom.WeavableConcepts.Add(
                new WeavableConcept<MethodDefinition>(
                    new PointCut<MethodDefinition>(m => m.HasBody),
                    new TransformAdvisor<MethodDefinition>(
                        method =>
                        {
                            if (isTargetReplacedByDecorator)
                            {
                                RedirectTargetCallsToDecorator(method, target, decorationTarget);
                            }

                            RedirectForwardingCall(target, method, areArgsAutoForwarded);
                        })));

            redirectMethodCallsLoom.Weave(targetAssembly, this.Resolver, this.GraphGetter);
        }

        private MethodDefinition CloneDecorationIntoTargetLocation(
            MethodDefinition target,
            string decorationName,
            bool isTargetReplacedByDecorator,
            out bool areArgsAutoForwarded)
        {
            using (var decorationAssembly = this.Resolver.Resolve(AssemblyNameReference.Parse(this.Decoration.Target.GetType().Assembly.FullName)))
            {
                var decorationSource = decorationAssembly.MainModule.ImportReference(this.Decoration.Method).Resolve();

                var decorationGraph = this.GraphGetter.Get(decorationSource);
                var cloningContext = new CloningContext(decorationGraph, decorationSource.DeclaringType, target.DeclaringType);
                areArgsAutoForwarded = isTargetReplacedByDecorator && CanArgsBeAutoForwarded(target, decorationSource);
                var localAreArgsAutoForwarded = areArgsAutoForwarded;

                if (isTargetReplacedByDecorator && !areArgsAutoForwarded)
                {
                    EnsureParametersMatch(target, decorationSource, cloningContext);
                }

                // if the names are the same, then we're replacing the target, so move it to a randomly named location
                if (isTargetReplacedByDecorator)
                {
                    target.Name = $"cilador_{target.Name}_{Guid.NewGuid().ToString("N")}";
                }

                MethodDefinition decorationTarget = null;
                cloningContext.InlineWeaves.Add(new WeavableConcept<object>(
                    new PointCut<object>(s => s == decorationSource),
                    new TransformAdvisor<object>(
                    t =>
                    {
                        decorationTarget = (MethodDefinition)t;
                        decorationTarget.Name = decorationName;
                        decorationTarget.Attributes = target.Attributes;

                        if (localAreArgsAutoForwarded)
                        {
                            // modify target to accept args
                            var voidReference = cloningContext.TargetModule.ImportReference(typeof(void));
                            foreach (var parameter in target.Parameters)
                            {
                                var decorationParameter =
                                    new ParameterDefinition(
                                        parameter.Name,
                                        parameter.Attributes,
                                        voidReference);

                                cloningContext.CopyDetails(decorationParameter, parameter);

                                decorationTarget.Parameters.Add(decorationParameter);
                            }
                        }
                    })));

                cloningContext.Execute();

                Contract.Assert(decorationTarget != null);
                return decorationTarget;
            }
        }

        private static void EnsureParametersMatch(MethodDefinition target, MethodDefinition decorationSource, CloningContext cloningContext)
        {
            // capture original values so that can be replaced temporarily
            var decorationSourceName = decorationSource.Name;
            var decorationSourceAttributes = decorationSource.Attributes;

            // update decoration values to allow the target signature check to work
            decorationSource.Name = target.Name;
            decorationSource.Attributes = target.Attributes;
            if (!target.SignatureEquals(decorationSource, cloningContext))
            {
                throw new InvalidOperationException(
                    "Decoration signature has parameters that do not match target signature, so it cannot decorate a target with the same name. You may change the decoration signature to fully match the target signature, change the decoration name to be distinct from the target name, or make the decoration parameterless to take advantage of argument forwarding.");
            }

            // return original values to avoid side-effects
            decorationSource.Name = decorationSourceName;
            decorationSource.Attributes = decorationSourceAttributes;
        }

        private static bool CanArgsBeAutoForwarded(MethodDefinition target, MethodDefinition decorationSource)
        {
            return target.HasParameters && !decorationSource.HasParameters;
        }

        private static void RedirectTargetCallsToDecorator(
            MethodDefinition inMethod,
            MethodDefinition originalTargetMethod,
            MethodDefinition decorationMethod)
        {
            // if replacing rather than adding a separately named decorator, then redirect all calls to the decorator
            foreach (var instruction in inMethod.Body.Instructions.Where(i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.Operand is MethodReference && ((MethodReference)i.Operand).FullName == originalTargetMethod.FullName).ToArray())
            {
                instruction.Operand = decorationMethod;
            }
        }

        private static void RedirectForwardingCall(
            MethodDefinition originalTargetMethod,
            MethodDefinition decorationMethod,
            bool areArgsAutoForwarded)
        {
            // TODO needs rethought to consider whether each involved method is instance or static
            // may have some details wrong, so need some tests around this concern
            foreach (var instruction in
                decorationMethod.Body.Instructions.Where(
                    i => i.OpCode == OpCodes.Call && i.Operand is MethodReference &&
                    ((MethodReference)i.Operand).Name == nameof(Forwarders.ForwardToOriginalAction)).ToArray())
            {
                instruction.Operand = originalTargetMethod;

                if (areArgsAutoForwarded && originalTargetMethod.HasParameters)
                {
                    // implies no arguments are on the forwarded version of the method call
                    // this means we simply add instructions to ldargs.1 - the count of parameters
                    // this pointer handling (i.e. ldarg.0) is done afterward

                    var ilProcessor = decorationMethod.Body.GetILProcessor();

                    var newInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
                    ilProcessor.InsertBefore(instruction, newInstruction);

                    // TODO handling of this needs more consideration since this assumes that we're decorating an instance method
                    var parameterCount = (ushort)originalTargetMethod.Parameters.Count;

                    if (parameterCount >= 1) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_1)); }
                    if (parameterCount >= 2) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_2)); }
                    if (parameterCount >= 3) { ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg_3)); }

                    for(ushort i = 4; i <= parameterCount; ++i)
                    {
                        ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Ldarg, i));
                    }
                }

                if (!originalTargetMethod.IsStatic)
                {
                    AddThisPointerToMethodCall(decorationMethod, instruction);
                }
            }
        }

        private static void AddThisPointerToMethodCall(MethodDefinition method, Instruction instruction)
        {
            // TODO handling of this pointer needs more consideration since this assumes that we're decorating an instance method
            var firstArgInstruction = instruction.Previous;
            //while (firstArgInstruction.Previous != null && firstArgInstruction.Previous.OpCode.Name.StartsWith("Ld"))
            while (
                firstArgInstruction.Previous != null &&
                firstArgInstruction.Previous.OpCode.Code != OpCodes.Call.Code &&
                firstArgInstruction.Previous.OpCode.Code != OpCodes.Callvirt.Code &&
                firstArgInstruction.Previous.OpCode.Code != OpCodes.Nop.Code)
            {
                if (firstArgInstruction.OpCode == OpCodes.Ldarg_0) { /* this pointer already being sent to the method */ return; }
                firstArgInstruction = firstArgInstruction.Previous;
            }
            var ilProcessor = method.Body.GetILProcessor();
            var newInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
            ilProcessor.InsertBefore(firstArgInstruction, newInstruction);
        }
    }

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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
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
            Contract.Ensures(this.Decoration != null);
            Contract.Ensures(this.DecorationNameGenerator == null);
        }
    }
}
