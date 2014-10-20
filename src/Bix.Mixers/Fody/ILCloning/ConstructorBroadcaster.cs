/***************************************************************************/
// Copyright 2013-2014 Riley White
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

using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class ConstructorBroadcaster
    {
        private ILCloningContext ILCloningContext { get; set; }
        private MethodDefinition SourceConstructor { get; set; }
        private TypeDefinition TargetType { get; set; }
        public List<VariableCloner> VariableCloners { get; private set; }
        public List<InstructionCloner> InstructionCloners { get; private set; }

        public ConstructorBroadcaster(
            ILCloningContext ilCloningContext,
            MethodDefinition sourceConstructor,
            TypeDefinition targetType)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(sourceConstructor != null);
            Contract.Requires(sourceConstructor.DeclaringType == ilCloningContext.RootSource);
            Contract.Requires(!sourceConstructor.Parameters.Any());
            Contract.Requires(sourceConstructor.HasBody);
            Contract.Requires(targetType != null);
            Contract.Requires(targetType.Methods != null);

            this.ILCloningContext = ilCloningContext;
            this.SourceConstructor = sourceConstructor;
            this.TargetType = targetType;
            this.VariableCloners = new List<VariableCloner>();
            this.InstructionCloners = new List<InstructionCloner>();

            Contract.Assert(this.SourceConstructor.Body != null);
            Contract.Assert(this.SourceConstructor.Body.Instructions != null);
            Contract.Assert(this.SourceConstructor.Body.Instructions.Any());
        }

        public void BroadcastConstructor()
        {
            List<VariableDefinition> sourceInitializationVariables;
            List<Instruction> sourceInitializationInstructions;
            List<VariableDefinition> sourceConstructionVariables;
            List<Instruction> sourceConstructionInstructions;
            if (!this.TryMultiplexInitializationAndConstructionItems(
                out sourceInitializationVariables,
                out sourceInitializationInstructions,
                out sourceConstructionVariables,
                out sourceConstructionInstructions)) { return; }

            // we're going to insert the initializing instruction clone targets into the initializing constructors after the first instruction
            //var instructionCloners = new List<InstructionCloner>(sourceInitializationInstructions.Count * initializingTargetConstructors.Count());
            foreach (var initializingTargetConstructor in this.GetInitializingTargetConstructors())
            {
                // clone all variables
                initializingTargetConstructor.Body.InitLocals = initializingTargetConstructor.Body.InitLocals || this.SourceConstructor.Body.InitLocals;
                var variableCloners = new List<VariableCloner>();
                var voidTypeReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
                foreach (var sourceVariable in this.SourceConstructor.Body.Variables)
                {
                    var targetVariable = new VariableDefinition(sourceVariable.Name, voidTypeReference);
                    initializingTargetConstructor.Body.Variables.Add(targetVariable);
                    variableCloners.Add(new VariableCloner(this.ILCloningContext, targetVariable, sourceVariable));
                }
                this.VariableCloners.AddRange(variableCloners);

                var ilProcessor = initializingTargetConstructor.Body.GetILProcessor();
                var firstInstructionInTargetConstructor = initializingTargetConstructor.Body.Instructions[0];

                // go backwards through the source initialization instructions
                // this makes it so that every new instruction is added just after the first instruction in the target
                var instructionCloners = new List<InstructionCloner>(sourceInitializationInstructions.Count);
                MethodContext methodContext = new MethodContext(
                    this.ILCloningContext,
                    Tuple.Create(this.SourceConstructor.Body.ThisParameter, initializingTargetConstructor.Body.ThisParameter),
                    new List<Tuple<ParameterDefinition, LazyAccessor<ParameterDefinition>>>(),
                    variableCloners,
                    instructionCloners);
                for (int i = sourceInitializationInstructions.Count - 1; i >= 0; i--)
                {
                    var sourceInstruction = sourceInitializationInstructions[i];
                    Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(this.ILCloningContext, ilProcessor, sourceInstruction);
                    ilProcessor.InsertAfter(firstInstructionInTargetConstructor, targetInstruction);
                    instructionCloners.Add(new InstructionCloner(methodContext, targetInstruction, sourceInstruction));
                }
                this.InstructionCloners.AddRange(instructionCloners);
            }
        }

        private IEnumerable<MethodDefinition> GetInitializingTargetConstructors()
        {
            // find target constructors that call into their base constructor
            var targetBaseTypeConstructorFullNames =
                from method in this.ILCloningContext.RootTarget.BaseType.Resolve().Methods
                where method.IsConstructor
                select method.FullName;

            IEnumerable<MethodDefinition> initializingTargetConstructors;
            initializingTargetConstructors =
                from method in this.TargetType.Methods
                where method.IsConstructor &&
                    method.HasBody &&
                    method.Body.Instructions.Any(instruction =>
                        instruction.OpCode == OpCodes.Call &&
                        instruction.Operand != null &&
                        instruction.Operand is MethodReference &&
                        targetBaseTypeConstructorFullNames.Contains(((MethodReference)instruction.Operand).FullName))
                select method;

            // a valid type should have at least one constructor that calls into a base constructor
            if (!initializingTargetConstructors.Any())
            {
                throw new InvalidOperationException("Could not find any target constructors that call into a base constructor.");
            }

            return initializingTargetConstructors;
        }

        private bool TryMultiplexInitializationAndConstructionItems(
            out List<VariableDefinition> sourceInitializationVariables,
            out List<Instruction> sourceInitializationInstructions,
            out List<VariableDefinition> sourceConstructionVariables,
            out List<Instruction> sourceConstructionInstructions)
        {
            sourceInitializationVariables = new List<VariableDefinition>();
            sourceInitializationInstructions = new List<Instruction>();
            sourceConstructionVariables = new List<VariableDefinition>(this.SourceConstructor.Body.Variables); // start with the assumption that all variables are construction variables
            sourceConstructionInstructions = new List<Instruction>();

            // we want the collection of instructions between the first ldarg and the call into the base constructor.
            var sourceInstruction = this.SourceConstructor.Body.Instructions[0];
            if (sourceInstruction.OpCode != OpCodes.Ldarg_0 || sourceInstruction.Operand != null)
            {
                throw new InvalidOperationException("The first instruction in a mixin implementation's default constructor wasn't the expected ldarg.0");
            }

            var objectConstructorFullName = this.ILCloningContext.RootSource.Module.Import(typeof(object).GetConstructor(new Type[0])).FullName;
            int? baseConstructorCallInstructionIndex = default(int?);
            for (int i = 1; i < this.SourceConstructor.Body.Instructions.Count && !baseConstructorCallInstructionIndex.HasValue; i++)
            {
                // we have a rule that an implementation must have only a default constructor and that it must inherit from object
                // all we have to do is find the first call to object's constructor
                sourceInstruction = this.SourceConstructor.Body.Instructions[i];
                if (sourceInstruction.OpCode == OpCodes.Call &&
                    sourceInstruction.Operand != null &&
                    sourceInstruction.Operand is MethodReference &&
                    ((MethodReference)sourceInstruction.Operand).FullName == objectConstructorFullName)
                {
                    baseConstructorCallInstructionIndex = i;
                }
                else
                {
                    // add the source instruction to initialization instructions
                    sourceInitializationInstructions.Add(sourceInstruction);

                    // if the instruction references a variable, then move that from construction to initialization
                    var variableOperand = sourceInstruction.Operand as VariableDefinition;
                    if (variableOperand != null)
                    {
                        sourceInitializationVariables.Remove(variableOperand);
                        sourceConstructionVariables.Add(variableOperand);
                    }
                }
            }

            if (!baseConstructorCallInstructionIndex.HasValue || baseConstructorCallInstructionIndex.Value <= 0)
            {
                // if more constructors were allowed, this might be possible in constructors that call through to other constructors
                // under current assumptions, this shouldn't happen
                // this is not the right place to handle the error, though, so simply return false from the method.
                sourceInitializationVariables = null;
                sourceInitializationInstructions = null;
                sourceConstructionVariables = null;
                sourceConstructionInstructions = null;
                return false;
            }

            // enforce the rule about no logic in the source's default constructor
            for (int i = baseConstructorCallInstructionIndex.Value + 1; i < this.SourceConstructor.Body.Instructions.Count; i++)
            {
                sourceInstruction = this.SourceConstructor.Body.Instructions[i];
                if (sourceInstruction.OpCode != OpCodes.Ret && sourceInstruction.OpCode != OpCodes.Nop)
                {
                    throw new NotImplementedException();
                }
            }

            // sanity check
            Contract.Assert(sourceInitializationInstructions.Count == baseConstructorCallInstructionIndex - 1);

            return true;
        }
    }
}
