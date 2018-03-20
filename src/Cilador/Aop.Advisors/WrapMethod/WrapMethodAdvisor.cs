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

using Cilador.Aop.Advisors.Transform;
using Cilador.Aop.Core;
using Cilador.Clone;
using Cilador.Graph.Factory;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Aop.Advisors.WrapMethod
{
    public class WrapMethodAdvisor<TArg> : IAdvisor<MethodDefinition>
    {
        public WrapMethodAdvisor(IAssemblyResolver resolver, CilGraphGetter graphGetter, ActionAdvice<TArg> advice)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(advice != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.Advice != null);

            this.Resolver = resolver;
            this.GraphGetter = graphGetter;
            this.Advice = advice;
        }

        public IAssemblyResolver Resolver { get; }
        public CilGraphGetter GraphGetter { get; }
        public ActionAdvice<TArg> Advice { get; }

        public void Advise(MethodDefinition target)
        {
            var targetMethod = target;
            var targetAssembly = targetMethod.Module.Assembly;
            var adviceAssembly = this.Resolver.Resolve(AssemblyNameReference.Parse(this.Advice.Target.GetType().Assembly.FullName));
            var adviceType = adviceAssembly.MainModule.GetType(this.Advice.Target.GetType().FullName.ToCecilTypeName());
            var adviceMethod = adviceType.Methods.Single(m => m.Name == this.Advice.Method.Name);

            var originalTargetMethodName = targetMethod.Name;
            targetMethod.Name = $"cilador_{Guid.NewGuid().ToString("N")}";

            var adviceGraph = this.GraphGetter.Get(adviceMethod);
            var cloningContext = new CloningContext(adviceGraph, adviceMethod.DeclaringType, targetMethod.DeclaringType);

            MethodDefinition adviceMethodTarget = null;
            cloningContext.InlineAspects.Add(new Aspect<object>(
                new PointCut<object>(s => s == adviceMethod),
                new TransformAdvisor<object>(
                t =>
                {
                    adviceMethodTarget = (MethodDefinition)t;
                    adviceMethodTarget.Name = originalTargetMethodName;
                })));

            cloningContext.Execute();

            targetMethod.DeclaringType.CustomAttributes.Clear();

            var redirectMethodCallsLoom = new Loom();
            redirectMethodCallsLoom.Aspects.Add(new Aspect<MethodDefinition>(
                new PointCut<MethodDefinition>(m => m.HasBody),
                new TransformAdvisor<MethodDefinition>(
                    method =>
                    {
                        foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == targetMethod.Name).ToArray())
                        {
                            instruction.Operand = adviceMethodTarget;
                        }

                        foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == nameof(AdviceForwarder.ForwardToOriginalAction)).ToArray())
                        {
                            instruction.Operand = targetMethod;

                            if (!targetMethod.IsStatic)
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
                    })));

            redirectMethodCallsLoom.Weave(targetAssembly, this.Resolver, this.GraphGetter);
        }
    }
}
