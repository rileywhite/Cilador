using Cilador.Clone;
using Cilador.Graph.Factory;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cilador.Core
{
    public class Loom
    {
        public List<Tuple<Func<MethodDefinition, bool>, ActionAdvice<string[]>>> Aspects { get; }
            = new List<Tuple<Func<MethodDefinition, bool>, ActionAdvice<string[]>>>();

        public void Weave(AssemblyDefinition targetAssembly)
        {
            var resolver = new DefaultAssemblyResolver();
            var graphGetter = new CilGraphGetter();

            foreach(var aspect in this.Aspects)
            {
                var allMethods = targetAssembly.Modules.SelectMany(mod => mod.Types).SelectMany(t => t.Methods).ToArray();
                foreach (var targetMethod in targetAssembly.Modules.SelectMany(mod => mod.Types).SelectMany(t => t.Methods).Where(m => aspect.Item1(m)).ToArray())
                {
                    var adviceAssembly = resolver.Resolve("Cilador.Tests");
                    var adviceParentType = adviceAssembly.MainModule.GetType("Cilador.Core.AddAdviceToMethod");
                    var adviceMethod = adviceParentType.NestedTypes.SelectMany(t => t.Methods).Single(m => m.Name == aspect.Item2.Method.Name);
                    var adviceType = adviceMethod.DeclaringType;

                    targetMethod.Name = $"cilador_{Guid.NewGuid().ToString("N")}";
                    var adviceGraph = graphGetter.Get(adviceMethod);
                    var cloningContext = new CloningContext(adviceGraph, adviceMethod.DeclaringType, targetMethod.DeclaringType);

                    MethodDefinition adviceMethodTarget = null;
                    cloningContext.SourcePredicatesAndTargetTransforms.Add(Tuple.Create<Func<object, bool>, Action<object>>(
                        s => s == adviceMethod,
                        t =>
                        {
                            adviceMethodTarget = (MethodDefinition)t;
                            adviceMethodTarget.Name = "Run";
                        }));

                    cloningContext.Execute();

                    targetMethod.DeclaringType.CustomAttributes.Clear();

                    var methodCallInstructions =
                        targetAssembly
                            .MainModule
                            .Types
                            .SelectMany(t => t.Methods)
                            .Where(m => m.HasBody)
                            .SelectMany(m => m.Body.Instructions)
                            .Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference).ToArray();

                    foreach (var method in targetAssembly.MainModule.Types.SelectMany(t => t.Methods).Where(m => m.HasBody).ToArray())
                    {
                        foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == targetMethod.Name).ToArray())
                        {
                            instruction.Operand = adviceMethodTarget;
                        }

                        foreach (var instruction in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference && ((MethodReference)i.Operand).Name == "ForwardToOriginalAction").ToArray())
                        {
                            instruction.Operand = targetMethod;

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
}
