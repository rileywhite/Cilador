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

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Multiplexes a single constructor into component variables and instructions.
    /// This separates the compiler generated initialization variables and instructions used
    /// for, as an example, initializing fields from those that run the actual constructor logic.
    /// </summary>
    internal class ConstructorMultiplexer
    {
        /// <summary>
        /// Gets a multiplexed constructor.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="constructor">Constructor to multiplex.</param>
        /// <returns>Multiplexed constructor.</returns>
        public static ConstructorMultiplexer Get(ILCloningContext ilCloningContext, MethodDefinition constructor)
        {
            var multiplexer = new ConstructorMultiplexer(ilCloningContext, constructor);
            multiplexer.Multiplex();
            return multiplexer;
        }

        /// <summary>
        /// Creates a new <see cref="ConstructorMultiplexer"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="constructor">Constructor to multiplex.</param>
        private ConstructorMultiplexer(ILCloningContext ilCloningContext, MethodDefinition constructor)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(constructor != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Constructor != null);

            this.ILCloningContext = ilCloningContext;
            this.Constructor = constructor;
        }

        /// <summary>
        /// Does the work of multiplexing the constructor.
        /// </summary>
        private void Multiplex()
        {
            Contract.Ensures(this.InitializationVariables != null);
            Contract.Ensures(this.InitializationInstructions != null);
            Contract.Ensures(this.ConstructionVariables != null);
            Contract.Ensures(this.ConstructionInstructions != null);

            this.InnerInitializationVariables = new List<VariableDefinition>();
            this.InnerInitializationInstructions = new List<Instruction>();
            this.InnerConstructionVariables = new List<VariableDefinition>(this.Constructor.Body.Variables); // start with the assumption that all variables are construction variables
            this.InnerConstructionInstructions = new List<Instruction>();

            this.PopulateInitializationAndBoundary();
            this.PopulateConstruction();
        }

        /// <summary>
        /// Populated the initialization variables and instructions. Also populates the boundary instruction index.
        /// </summary>
        private void PopulateInitializationAndBoundary()
        {
            // we want the collection of instructions between the first ldarg and the boundary instruction (base constructor call or forwarding this constructor call).
            var instruction = this.Constructor.Body.Instructions[0];
            if (instruction.OpCode != OpCodes.Ldarg_0 || instruction.Operand != null)
            {
                throw new InvalidOperationException("The first instruction in a mixin implementation's default constructor wasn't the expected ldarg.0");
            }

            for (int i = 1; i < this.Constructor.Body.Instructions.Count && !this.BoundaryInstructionIndex.HasValue; i++)
            {
                instruction = this.Constructor.Body.Instructions[i];
                var operandAsMethodReference = instruction.Operand as MethodReference;
                if (instruction.OpCode == OpCodes.Call &&
                    operandAsMethodReference != null &&
                    operandAsMethodReference.Name == ".ctor" &&
                    (operandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.FullName ||
                    operandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.BaseType.FullName))
                {
                    this.BoundaryInstructionIndex = i;
                    this.IsInitializingConstructor =
                        operandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.BaseType.FullName;
                }
                else
                {
                    // add the instruction to initialization instructions
                    this.InnerInitializationInstructions.Add(instruction);

                    // if the instruction references a variable, then move that from construction to initialization
                    VariableDefinition variable;
                    if (this.TryGetReferencedVariable(instruction, out variable))
                    {
                        if (this.InnerConstructionVariables.Contains(variable))
                        {
                            this.InnerConstructionVariables.Remove(variable);
                            this.InnerInitializationVariables.Add(variable);
                        }
                        else if (!this.InnerInitializationVariables.Contains(variable))
                        {
                            // variable wasn't found at all
                            throw new InvalidOperationException(
                                "An instruction in the initialization part of a multiplexed constructor references a variable that either cannot be found.");
                        }
                    }
                }
            }

            if (!this.BoundaryInstructionIndex.HasValue || this.BoundaryInstructionIndex.Value <= 0)
            {
                throw new InvalidOperationException("Cannot find base or forwarding constructor call in mixin implementation's parameterless constructor.");
            }

            // sanity check
            Contract.Assert(this.InnerInitializationInstructions.Count == this.BoundaryInstructionIndex - 1);
        }

        /// <summary>
        /// Populates construction variables and instructions.
        /// </summary>
        private void PopulateConstruction()
        {
            for (int i = this.BoundaryInstructionIndex.Value + 1; i < this.Constructor.Body.Instructions.Count; i++)
            {
                var instruction = this.Constructor.Body.Instructions[i];
                this.InnerConstructionInstructions.Add(instruction);

                // if the instruction references a variable, then ensure that the variable exists in the collection of construction variables
                VariableDefinition variable;
                if (this.TryGetReferencedVariable(instruction, out variable) &&
                    !this.InnerConstructionVariables.Contains(variable))
                {
                    // variable wasn't in the expected place
                    if (this.InnerInitializationVariables.Contains(variable))
                    {
                        // looks like a variable was shared
                        // this indicates that the code was written with an incorrect assumption that variables are not shared
                        throw new InvalidOperationException(
                            "An instruction in the construction part of a multiplexed constructor references a variable that is also referenced by initialization instructions.");
                    }
                    else
                    {
                        // a variable wasn't found at all
                        throw new InvalidOperationException(
                            "An instruction in the construction part of a multiplexed constructor references a variable that either cannot be found.");
                    }
                }
            }
        }

        /// <summary>
        /// Finds a variable, if any, that is referenced by an instruction.
        /// </summary>
        /// <param name="instruction">Instruction that may reference a variable.</param>
        /// <param name="variable">When populated, this is the referenced variable.</param>
        /// <returns></returns>
        private bool TryGetReferencedVariable(Instruction instruction, out VariableDefinition variable)
        {
            variable = instruction.Operand as VariableDefinition;
            if (variable == null)
            {
                int? variableIndex;
                if (instruction.TryGetVariableIndex(out variableIndex))
                {
                    variable = this.InnerConstructionVariables.FirstOrDefault(
                        possibleInitializationVariable => possibleInitializationVariable.Index == variableIndex.Value);
                }
            }
            return variable != null;
        }

        /// <summary>
        /// Gets or sets the IL cloning context.
        /// </summary>
        private ILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Gets whether the constructor is initializing, i.e. whether it runs compiler generated code
        /// such as initializing instance fields. The indicator in the IL is that it call the base constructor.
        /// </summary>
        public bool IsInitializingConstructor { get; private set; }

        /// <summary>
        /// Gets or sets the index of the constructor instruction that separates compiler-generated initialization
        /// code, such as field initialization, from developer-written constructor code. This will be the index
        /// of the instruction that calls the base constructor or that forwards to a different "this" constructor.
        /// </summary>
        private int? BoundaryInstructionIndex { get; set; }

        /// <summary>
        /// Gets the constructor that will be multiplexed.
        /// </summary>
        public MethodDefinition Constructor { get; private set; }

        /// <summary>
        /// Gets or sets variables used in compiler-generated initialization code.
        /// </summary>
        private List<VariableDefinition> InnerInitializationVariables { get; set; }

        /// <summary>
        /// Gets variables used in compiler-generated initialization code.
        /// </summary>
        public IReadOnlyList<VariableDefinition> InitializationVariables
        {
            get { return this.InnerInitializationVariables; }
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
            get { return this.InnerInitializationInstructions; }
        }

        /// <summary>
        /// Gets or sets variables used in developer-written constructor code.
        /// </summary>
        private List<VariableDefinition> InnerConstructionVariables { get; set; }

        /// <summary>
        /// Gets variables used in developer-written constructor code.
        /// </summary>
        public IReadOnlyList<VariableDefinition> ConstructionVariables
        {
            get { return this.InnerConstructionVariables; }
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
            get { return this.InnerConstructionInstructions; }
        }
    }
}
