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

using Cilador.Clone;
using Cilador.Graph.Factory;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Aop
{
    /// <summary>
    /// Represents an AOP join point (see https://en.wikipedia.org/wiki/Join_point) for a method.
    /// </summary>
    public class MethodJoinPoint
    {
        public MethodJoinPoint(IAssemblyResolver resolver, CilGraphGetter graphGetter, MethodDefinition target)
        {
            Contract.Requires(resolver != null);
            Contract.Requires(graphGetter != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.Resolver != null);
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.Target != null);

            this.Resolver = resolver;
            this.GraphGetter = graphGetter;
            this.Target = target;
        }

        internal IAssemblyResolver Resolver { get; }
        internal CilGraphGetter GraphGetter { get; }
        internal MethodDefinition Target { get; }

        public void ApplyAdvice<TArg>(ActionAdvice<TArg> advice)
        {
            var targetMethod = this.Target;
            var targetAssembly = targetMethod.Module.Assembly;
            var adviceAssembly = this.Resolver.Resolve(advice.Target.GetType().Assembly.FullName);
            var adviceType = adviceAssembly.MainModule.GetType(advice.Target.GetType().FullName.ToCecilTypeName());
            var adviceMethod = adviceType.Methods.Single(m => m.Name == advice.Method.Name);

            var originalTargetMethodName = targetMethod.Name;
            targetMethod.Name = $"cilador_{Guid.NewGuid().ToString("N")}";

            var adviceGraph = this.GraphGetter.Get(adviceMethod);
            var cloningContext = new CloningContext(adviceGraph, adviceMethod.DeclaringType, targetMethod.DeclaringType);

            MethodDefinition adviceMethodTarget = null;
            cloningContext.SourcePredicatesAndTargetTransforms.Add(Tuple.Create<Func<object, bool>, Action<object>>(
                s => s == adviceMethod,
                t =>
                {
                    adviceMethodTarget = (MethodDefinition)t;
                    adviceMethodTarget.Name = originalTargetMethodName;
                }));

            //cloningContext.SourcePredicatesAndTargetTransforms.Add(Tuple.Create<Func<object, bool>, Action<object>>(
            //    s =>
            //    {
            //        var i = s as Instruction;
            //        if (i == null) { return false; }
            //        return i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == targetMethod.Name;
            //    },
            //    t =>
            //    {
            //        ((Instruction)t).Operand = adviceMethodTarget;
            //    }));

            //cloningContext.SourcePredicatesAndTargetTransforms.Add(Tuple.Create<Func<object, bool>, Action<object>>(
            //    s =>
            //    {
            //        var i = s as Instruction;
            //        if (i == null) { return false; }
            //        return i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == "ForwardToOriginalAction";
            //    },
            //    t =>
            //    {
            //        var instruction = (Instruction)t;
            //        instruction.Operand = targetMethod;

            //        if (!targetMethod.IsStatic)
            //        {
            //            var firstArgInstruction = instruction.Previous;
            //            while (firstArgInstruction.Previous != null && firstArgInstruction.Previous.OpCode.Name.StartsWith("Ld"))
            //            {
            //                firstArgInstruction = firstArgInstruction.Previous;
            //            }
            //            var ilProcessor = method.Body.GetILProcessor();
            //            var newInstruction = ilProcessor.Create(OpCodes.Ldarg_0);
            //            ilProcessor.InsertBefore(firstArgInstruction, newInstruction);
            //        }
            //    }));

            cloningContext.Execute();

            targetMethod.DeclaringType.CustomAttributes.Clear();

            foreach (var method in targetAssembly.MainModule.Types.SelectMany(t => t.Methods).Where(m => m.HasBody).ToArray())
            {
                foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == targetMethod.Name).ToArray())
                {
                    instruction.Operand = adviceMethodTarget;
                }

                foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == "ForwardToOriginalAction").ToArray())
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
            }
        }
    }
}
