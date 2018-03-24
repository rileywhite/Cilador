/***************************************************************************/
// Copyright 2013-2018 Riley White
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
    public class ActionDecorator<TArg> : IConceptWeaver<MethodDefinition>
    {
        public ActionDecorator(
            IAssemblyResolver resolver,
            CilGraphGetter graphGetter,
            ActionDecoration<TArg> decoration,
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
        public ActionDecoration<TArg> Decoration { get; }
        public Func<string, string> DecorationNameGenerator { get; }

        public void Weave(MethodDefinition target)
        {
            var targetAssembly = target.Module.Assembly;
            MethodDefinition decorationTarget = null;
            var decorationName = this.DecorationNameGenerator(target.Name);
            var isTargetReplacedByDecorator = decorationName == target.Name;

            decorationTarget = CloneDecorationIntoTargetLocation(target, decorationTarget, decorationName, isTargetReplacedByDecorator);

            //target.DeclaringType.CustomAttributes.Clear();

            var redirectMethodCallsLoom = new Loom();
            redirectMethodCallsLoom.Aspects.Add(new WeavableConcept<MethodDefinition>(
                new PointCut<MethodDefinition>(m => m.HasBody),
                new TransformAdvisor<MethodDefinition>(
                    method =>
                    {

                        if (isTargetReplacedByDecorator)
                        {
                            RedirectTargetCallsToDecorator(target, method, decorationTarget);
                        }

                        RedirectForwardingCall(target, method);
                    })));

            redirectMethodCallsLoom.Weave(targetAssembly, this.Resolver, this.GraphGetter);
        }

        private MethodDefinition CloneDecorationIntoTargetLocation(MethodDefinition target, MethodDefinition decorationTarget, string decorationName, bool isTargetReplacedByDecorator)
        {
            using (var adviceAssembly = this.Resolver.Resolve(AssemblyNameReference.Parse(this.Decoration.Target.GetType().Assembly.FullName)))
            {
                var adviceType = adviceAssembly.MainModule.GetType(this.Decoration.Target.GetType().FullName.ToCecilTypeName());
                var decorationSource = adviceType.Methods.Single(m => m.Name == this.Decoration.Method.Name);

                var adviceGraph = this.GraphGetter.Get(decorationSource);
                var cloningContext = new CloningContext(adviceGraph, decorationSource.DeclaringType, target.DeclaringType);

                // if the names are the same, then we're replacing the target, so move it to a randomly named location
                if (isTargetReplacedByDecorator)
                {
                    target.Name = $"cilador_{Guid.NewGuid().ToString("N")}";
                }
                cloningContext.InlineWeaves.Add(new WeavableConcept<object>(
                    new PointCut<object>(s => s == decorationSource),
                    new TransformAdvisor<object>(
                    t =>
                    {
                        decorationTarget = (MethodDefinition)t;
                        decorationTarget.Name = decorationName;
                        decorationTarget.Attributes = target.Attributes;
                    })));

                cloningContext.Execute();
            }

            return decorationTarget;
        }

        private static void RedirectTargetCallsToDecorator(MethodDefinition target, MethodDefinition method, MethodDefinition decorationTarget)
        {
            // if replacing rather than adding a separately named decorator, then redirect all calls to the decorator
            // TODO name check is not enough since overloads will cause a problem
            foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == target.Name).ToArray())
            {
                instruction.Operand = decorationTarget;
            }
        }

        private static void RedirectForwardingCall(MethodDefinition target, MethodDefinition method)
        {
            foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == nameof(Forwarders.ForwardToOriginalAction)).ToArray())
            {
                instruction.Operand = target;

                if (!target.IsStatic)
                {
                    AddThisPointerToMethodCall(method, instruction);
                }
            }
        }

        private static void AddThisPointerToMethodCall(MethodDefinition method, Instruction instruction)
        {
            var firstArgInstruction = instruction.Previous;
            while (firstArgInstruction.Previous != null && firstArgInstruction.Previous.OpCode.Name.StartsWith("Ld"))
            {
                firstArgInstruction = firstArgInstruction.Previous;
            }
            var ilProcessor = method.Body.GetILProcessor();
            var newInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
            ilProcessor.InsertBefore(firstArgInstruction, newInstruction);
        }
    }
}
