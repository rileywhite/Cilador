/***************************************************************************/
// Copyright 2013-2016 Riley White
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

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Clone
{
    /// <summary>
    /// Multiplexes a single constructor into component variables and instructions.
    /// This separates the compiler generated initialization variables and instructions used
    /// for, as an example, initializing fields from those that run the actual constructor logic.
    /// </summary>
    internal class MultiplexedConstructor
    {
        /// <summary>
        /// Gets a multiplexed constructor.
        /// </summary>
        /// <param name="cloningContext">cloning context.</param>
        /// <param name="constructor">Constructor to multiplex.</param>
        /// <returns>Multiplexed constructor.</returns>
        public static MultiplexedConstructor Get(ICloningContext cloningContext, MethodDefinition constructor)
        {
            var multiplexer = new MultiplexedConstructor(cloningContext, constructor);
            multiplexer.Multiplex();
            return multiplexer;
        }

        /// <summary>
        /// Creates a new <see cref="MultiplexedConstructor"/>.
        /// </summary>
        /// <param name="cloningContext">cloning context.</param>
        /// <param name="constructor">Constructor to multiplex.</param>
        private MultiplexedConstructor(ICloningContext cloningContext, MethodDefinition constructor)
        {
            Contract.Requires(cloningContext != null);
            Contract.Requires(constructor != null);
            Contract.Ensures(this.CloningContext != null);
            Contract.Ensures(this.Constructor != null);
            Contract.Ensures(this.Variables != null);

            this.CloningContext = cloningContext;

            this.Constructor = constructor;
            this.ThisParameter = constructor.Body.ThisParameter;
            this.Variables = new List<VariableDefinition>(constructor.Body.Variables);
        }

        /// <summary>
        /// Gets or sets the cloning context.
        /// </summary>
        private ICloningContext CloningContext { get; }

        /// <summary>
        /// Gets whether the constructor is initializing, i.e. whether it runs compiler generated code
        /// such as initializing instance fields. The indicator in the CIL is that it call the base constructor.
        /// </summary>
        public bool IsInitializingConstructor { get; private set; }

        /// <summary>
        /// Gets or sets the first index of the set of instructions for the base or chained constructor call
        /// that separates compiler-generated initialization
        /// code, such as field initialization, from developer-written constructor code.
        /// </summary>
        private int? InnerBoundaryFirstInstructionIndex { get; set; }

        /// <summary>
        /// Gets or sets the "this" parameter within the constructor.
        /// </summary>
        public ParameterDefinition ThisParameter { get; private set; }

        /// <summary>
        /// Gets the first index of the set of instructions for the base or chained constructor call
        /// that separates compiler-generated initialization
        /// code, such as field initialization, from developer-written constructor code.
        /// </summary>
        public int BoundaryFirstInstructionIndex
        {
            get
            {
                Contract.Requires(this.InnerBoundaryFirstInstructionIndex.HasValue);
                // ReSharper disable once PossibleInvalidOperationException
                return this.InnerBoundaryFirstInstructionIndex.Value;
            }
        }

        /// <summary>
        /// Gets or sets the last index of the set of instructions for the base or chained constructor call
        /// that separates compiler-generated initialization
        /// code, such as field initialization, from developer-written constructor code.
        /// </summary>
        private int? InnerBoundaryLastInstructionIndex { get; set; }

        /// <summary>
        /// Gets the last index of the set of instructions for the base or chained constructor call
        /// that separates compiler-generated initialization
        /// code, such as field initialization, from developer-written constructor code.
        /// </summary>
        public int BoundaryLastInstructionIndex
        {
            get
            {
                Contract.Requires(this.InnerBoundaryLastInstructionIndex.HasValue);
                // ReSharper disable once PossibleInvalidOperationException
                return this.InnerBoundaryLastInstructionIndex.Value;
            }
        }

        /// <summary>
        /// Gets the constructor that will be multiplexed.
        /// </summary>
        public MethodDefinition Constructor { get; }

        /// <summary>
        /// Gets or sets the collection of all variables.
        /// </summary>
        public IReadOnlyList<VariableDefinition> Variables { get; }

        /// <summary>
        /// Gets or sets variables used in compiler-generated initialization code.
        /// </summary>
        private List<VariableDefinition> InnerInitializationVariables { get; set; }

        /// <summary>
        /// Gets variables used in compiler-generated initialization code.
        /// </summary>
        public IEnumerable<VariableDefinition> InitializationVariables
        {
            get
            {
                Contract.Requires(this.InnerInitializationVariables != null);
                return this.InnerInitializationVariables.Union(this.InnerSharedVariables);
            }
        }

        /// <summary>
        /// Tries to get the index of a variable within the group of initialization variables that
        /// have been pulled from the full collection of constructor variables.
        /// </summary>
        /// <param name="sourceInstruction">Source instruction possibly referencing a variable to look up.</param>
        /// <param name="index">Index of the variable in the subset of variables.</param>
        public bool TryGetInitializationVariableIndex(Instruction sourceInstruction, out int index)
        {
            Contract.Requires(sourceInstruction != null);

            VariableDefinition sourceVariable;

            if (this.TryGetReferencedVariable(sourceInstruction, out sourceVariable))
            {
                return this.TryGetInitializationVariableIndex(sourceVariable, out index);
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// Tries to get the index of a variable within the group of initialization variables that
        /// have been pulled from the full collection of constructor variables.
        /// </summary>
        /// <param name="sourceVariable">Source variable to look up.</param>
        /// <param name="index">Index of the variable in the subset of variables.</param>
        public bool TryGetInitializationVariableIndex(VariableDefinition sourceVariable, out int index)
        {
            Contract.Requires(sourceVariable != null);

            index = this.InnerInitializationVariables.IndexOf(sourceVariable);
            return index >= 0;
        }

        /// <summary>
        /// Gets or sets instructions used in compiler-generated initialization code.
        /// </summary>
        private List<Instruction> InnerInitializationInstructions { get; set; }

        /// <summary>
        /// Gets instructions used in compiler-generated initialization code.
        /// </summary>
        public IReadOnlyList<Instruction> InitializationInstructions
        {
            get
            {
                Contract.Requires(this.InnerInitializationInstructions != null);
                return this.InnerInitializationInstructions;
            }
        }

        /// <summary>
        /// Gets or sets variables used in developer-written constructor code.
        /// </summary>
        private List<VariableDefinition> InnerSharedVariables { get; set; }

        /// <summary>
        /// Gets or sets variables used in developer-written constructor code.
        /// </summary>
        private List<VariableDefinition> InnerConstructionVariables { get; set; }

        /// <summary>
        /// Gets variables used in developer-written constructor code.
        /// </summary>
        public IEnumerable<VariableDefinition> ConstructionVariables
        {
            get
            {
                Contract.Requires(this.InnerConstructionVariables != null);
                return this.InnerConstructionVariables.Union(this.InnerSharedVariables);
            }
        }

        /// <summary>
        /// Tries to get the index of a variable within the group of construction variables that
        /// have been pulled from the full collection of constructor variables.
        /// </summary>
        /// <param name="sourceInstruction">Source instruction possibly referencing a variable to look up.</param>
        /// <param name="index">Index of the variable in the subset of variables.</param>
        public bool TryGetConstructionVariableIndex(Instruction sourceInstruction, out int index)
        {
            Contract.Requires(sourceInstruction != null);

            VariableDefinition sourceVariable;

            if (this.TryGetReferencedVariable(sourceInstruction, out sourceVariable))
            {
                return this.TryGetConstructionVariableIndex(sourceVariable, out index);
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// Tries to get the index of a variable within the group of construction variables that
        /// have been pulled from the full collection of constructor variables.
        /// </summary>
        /// <param name="sourceVariable">Source variable to look up.</param>
        /// <param name="index">Index of the variable in the subset of variables.</param>
        public bool TryGetConstructionVariableIndex(VariableDefinition sourceVariable, out int index)
        {
            Contract.Requires(sourceVariable != null);

            index = this.InnerConstructionVariables.IndexOf(sourceVariable);
            return index >= 0;
        }

        /// <summary>
        /// Gets or sets instructions used in developer-written constructor code.
        /// </summary>
        private List<Instruction> InnerConstructionInstructions { get; set; }

        /// <summary>
        /// Gets instructions used in developer-written constructor code.
        /// </summary>
        public IReadOnlyList<Instruction> ConstructionInstructions
        {
            get
            {
                Contract.Requires(this.InnerConstructionInstructions != null);
                return this.InnerConstructionInstructions;
            }
        }

        /// <summary>
        /// Gets whether there are any initialization items in the multiplexed constructor.
        /// </summary>
        public bool HasInitializationItems
        {
            get
            {
                Contract.Requires(this.InnerInitializationVariables != null);
                Contract.Requires(this.InnerInitializationInstructions != null);
                return this.InnerInitializationInstructions.Any() || this.InnerInitializationVariables.Any();
            }
        }

        /// <summary>
        /// Gets whether there are any construction items in the multiplexed constructor.
        /// </summary>
        public bool HasConstructionItems
        {
            get
            {
                Contract.Requires(this.InnerConstructionVariables != null);
                Contract.Requires(this.InnerConstructionInstructions != null);
                return
                    this.InnerConstructionVariables.Any() ||
                    this.InnerConstructionInstructions.Any(
                    instruction =>
                        instruction.OpCode.Code != Code.Nop &&
                        instruction.OpCode.Code != Code.Ret);
            }
        }

        /// <summary>
        /// Does the work of multiplexing the constructor.
        /// </summary>
        private void Multiplex()
        {
            Contract.Ensures(this.InnerBoundaryLastInstructionIndex.HasValue);
            Contract.Ensures(this.InitializationVariables != null);
            Contract.Ensures(this.InitializationInstructions != null);
            Contract.Ensures(this.ConstructionVariables != null);
            Contract.Ensures(this.ConstructionInstructions != null);

            this.InnerSharedVariables = new List<VariableDefinition>();
            this.InnerInitializationVariables = new List<VariableDefinition>();
            this.InnerInitializationInstructions = new List<Instruction>();
            this.InnerConstructionVariables = new List<VariableDefinition>(this.Constructor.Body.Variables); // start with the assumption that all variables are construction variables
            this.InnerConstructionInstructions = new List<Instruction>();

            this.PopulateInitializationItemsAndFindBoundary();
            this.PopulateConstructionItems();
        }

        /// <summary>
        /// This type is used to gather groups of instructions that are potential calls to base or chained constructors.
        /// </summary>
        private class InstructionGroup
        {
            /// <summary>
            /// Grabs the next group of instructions that could potentially be a base or chained construtor call.
            /// </summary>
            /// <param name="sourceInstructions">Source instructions to look at.</param>
            /// <param name="firstIndex">Index of the first instruction to look at.</param>
            /// <param name="instructionGroup">Output parameter to contain the results.</param>
            /// <returns><c>true</c> if a group was successfully created, else <c>false</c>.</returns>
            public static bool TryGetNext(IList<Instruction> sourceInstructions, int firstIndex, out InstructionGroup instructionGroup)
            {
                Contract.Requires(sourceInstructions != null);

                if (firstIndex < 0 || firstIndex >= sourceInstructions.Count)
                {
                    instructionGroup = null;
                    return false;
                }

                var instructions = new List<Instruction>();
                var instruction = sourceInstructions[firstIndex];
                instructions.Add(instruction);


                int lastIndex;
                if (instruction.OpCode.Code != Code.Ldarg_0) { lastIndex = firstIndex; }
                else
                {
                    int i;
                    // calls into base and chained constructors start like this
                    // so we'll look for the next call instruction or any instruction where the next instruction is another ldarg.0
                    // meaning that the stack was cleared at some point
                    // there is no assumption that this grouping is generally useful, but the hope is that it will catch constructor calls in this specific case
                    var isLastInstructionFound = false;
                    for (i = firstIndex + 1; !isLastInstructionFound && i < sourceInstructions.Count; i++)
                    {
                        instruction = sourceInstructions[i];
                        instructions.Add(instruction);
                        if (instruction.OpCode.Code == Code.Call ||
                            instruction.OpCode.Code == Code.Callvirt ||
                            instruction.OpCode.Code == Code.Calli ||
                            (instruction.Next != null && instruction.Next.OpCode.Code == Code.Ldarg_0))
                        {
                            isLastInstructionFound = true;
                        }
                    }

                    lastIndex = i - 1;
                }

                instructionGroup = new InstructionGroup(firstIndex, lastIndex, instructions);
                return true;
            }

            /// <summary>
            /// Creates a new <see cref="InstructionGroup"/>.
            /// </summary>
            /// <param name="firstIndex">Index in the source instructions of the first instruction in this group.</param>
            /// <param name="lastIndex">Index in the source instructions of the last instruction in this group.</param>
            /// <param name="instructions">Instructions that have been grouped together.</param>
            private InstructionGroup(int firstIndex, int lastIndex, List<Instruction> instructions)
            {
                Contract.Requires(firstIndex <= lastIndex);
                Contract.Requires(instructions != null);
                Contract.Requires(instructions.Any());
                Contract.Ensures(this.FirstIndex <= this.LastIndex);
                Contract.Ensures(this.Instructions != null);
                Contract.Ensures(this.Instructions.Any());

                this.FirstIndex = firstIndex;
                this.LastIndex = lastIndex;
                this.Instructions = instructions;
            }

            /// <summary>
            /// Gets the collection of instructions that have been grouped together.
            /// </summary>
            public List<Instruction> Instructions { get; }

            /// <summary>
            /// Gets the index in the source instructions of the first instruction in this group.
            /// </summary>
            public int FirstIndex { get; }

            /// <summary>
            /// Gets the index in the source instructions of the last instruction in this group.
            /// </summary>
            public int LastIndex { get; }

            /// <summary>
            /// Gets the last instruction in the group.
            /// </summary>
            public Instruction LastInstruction => this.Instructions[this.Instructions.Count - 1];

            /// <summary>
            /// Gets whether this method group ends in a Call instruction.
            /// </summary>
            public bool IsCall => this.LastInstruction.OpCode.Code == Code.Call;
        }

        /// <summary>
        /// Populated the initialization variables and instructions. Also populates the boundary instruction index.
        /// </summary>
        private void PopulateInitializationItemsAndFindBoundary()
        {
            // we want the collection of instructions before the boundary instruction (base constructor or chained constructor call)
            InstructionGroup instructionGroup = null;
            var isConstructorCallFound = false;
            while (!isConstructorCallFound && InstructionGroup.TryGetNext(
                this.Constructor.Body.Instructions,
                instructionGroup?.LastIndex + 1 ?? 0,
                out instructionGroup))
            {
                var instructionOperandAsMethodReference = instructionGroup.LastInstruction.Operand as MethodReference;
                if (instructionGroup.IsCall &&
                    instructionOperandAsMethodReference != null &&
                    instructionOperandAsMethodReference.Name == ".ctor" &&
                    (instructionOperandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.FullName ||
                    instructionOperandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.BaseType.FullName))
                {
                    this.InnerBoundaryFirstInstructionIndex = instructionGroup.FirstIndex;
                    this.InnerBoundaryLastInstructionIndex = instructionGroup.LastIndex;
                    this.IsInitializingConstructor =
                        instructionOperandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.BaseType.FullName;
                    isConstructorCallFound = true;
                }
                else
                {
                    // add the instruction to initialization instructions
                    this.InnerInitializationInstructions.AddRange(instructionGroup.Instructions);
                }

                // any variables referenced by instructions should be removed from construction variables
                // and if we have not yet found the constructor call, then the variable should be put into initialization
                foreach (var instruction in instructionGroup.Instructions)
                {
                    VariableDefinition variable;
                    if (!this.TryGetReferencedVariable(instruction, out variable) ||
                        !this.InnerConstructionVariables.Contains(variable))
                    {
                        continue;
                    }

                    this.InnerConstructionVariables.Remove(variable);
                    if (!isConstructorCallFound)
                    {
                        // if an instance arises where a variable is only used in a constructor call
                        // then it would be dropped
                        this.InnerInitializationVariables.Add(variable);
                    }
                }
            }

            if (!isConstructorCallFound)
            {
                throw new InvalidOperationException(
                    $"Cannot find base or chained constructor call while multiplexing a constructor: {this.Constructor.FullName}");
            }
        }

        /// <summary>
        /// Populates construction variables and instructions.
        /// </summary>
        private void PopulateConstructionItems()
        {
            Contract.Assert(this.InnerBoundaryLastInstructionIndex.HasValue);

            for (var i = this.InnerBoundaryLastInstructionIndex.Value + 1; i < this.Constructor.Body.Instructions.Count; i++)
            {
                var instruction = this.Constructor.Body.Instructions[i];
                this.InnerConstructionInstructions.Add(instruction);

                // if the instruction references a variable, then ensure that the variable exists in the collection of construction variables
                VariableDefinition variable;
                if (!this.TryGetReferencedVariable(instruction, out variable) ||
                    this.InnerConstructionVariables.Contains(variable) ||
                    this.InnerSharedVariables.Contains(variable))
                {
                    continue;
                }

                // variable wasn't in the expected place
                if (this.InnerInitializationVariables.Contains(variable))
                {
                    // looks like a variable was shared
                    this.InnerInitializationVariables.Remove(variable);
                    this.InnerSharedVariables.Add(variable);
                }
                else
                {
                    // a variable wasn't found at all
                    throw new InvalidOperationException(
                        "An instruction in the construction part of a multiplexed constructor references a variable that either cannot be found.");
                }
            }
        }

        /// <summary>
        /// Finds a variable, if any, that is referenced by an instruction.
        /// </summary>
        /// <param name="instruction">Instruction that may reference a variable.</param>
        /// <param name="variable">When populated, this is the referenced variable.</param>
        /// <returns><c>true</c> if a variable referenced by the instruction was found, else <c>false</c>.</returns>
        private bool TryGetReferencedVariable(Instruction instruction, out VariableDefinition variable)
        {
            return
                MultiplexedConstructor.TryGetVariableDefinitionOperand(instruction, out variable) ||
                MultiplexedConstructor.TryGetIndexedVariableOperand(instruction, this.Constructor.Body.Variables, ref variable);
        }

        /// <summary>
        /// If the <paramref name="instruction"/> has a variable operand, retrieves it.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to look at</param>
        /// <param name="variable"><see cref="VariableDefinition"/> to populate with the operand, if possible.</param>
        /// <returns><c>true</c> if the operand is a <see cref="VariableDefinition"/>, else <c>false</c>.</returns>
        private static bool TryGetVariableDefinitionOperand(Instruction instruction, out VariableDefinition variable)
        {
            variable = instruction.Operand as VariableDefinition;
            return variable != null;
        }

        /// <summary>
        /// If the <paramref name="instruction"/> has a variable operand, retrieves it.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to look at</param>
        /// <param name="indexedVariables">Collection of variables that are potentially referenced by the operand</param>
        /// <param name="variable"><see cref="VariableDefinition"/> to populate with the referenced indexed variable, if possible.</param>
        /// <returns><c>true</c> if the operand is an index for a variable in the <paramref name="indexedVariables"/>, else <c>false</c>.</returns>
        private static bool TryGetIndexedVariableOperand(
            Instruction instruction,
            IEnumerable<VariableDefinition> indexedVariables,
            ref VariableDefinition variable)
        {
            int? variableIndex;
            if (!instruction.TryGetVariableIndex(out variableIndex)) { return false; }

            Contract.Assert(variableIndex.HasValue);
            variable = indexedVariables.FirstOrDefault(
                possibleInitializationVariable => possibleInitializationVariable.Index == variableIndex.Value);

            return variable != null;
        }
    }
}
