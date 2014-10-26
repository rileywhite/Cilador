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

            var initializationVariables = new List<VariableDefinition>();
            var initializationInstructions = new List<Instruction>();
            var constructionVariables = new List<VariableDefinition>(this.Constructor.Body.Variables); // start with the assumption that all variables are construction variables
            var constructionInstructions = new List<Instruction>();

            // we want the collection of instructions between the first ldarg and the boundary instruction (base constructor call or forwarding this constructor call).
            var sourceInstruction = this.Constructor.Body.Instructions[0];
            if (sourceInstruction.OpCode != OpCodes.Ldarg_0 || sourceInstruction.Operand != null)
            {
                throw new InvalidOperationException("The first instruction in a mixin implementation's default constructor wasn't the expected ldarg.0");
            }

            int? boundaryInstructionIndex = default(int?);
            for (int i = 1; i < this.Constructor.Body.Instructions.Count && !boundaryInstructionIndex.HasValue; i++)
            {
                sourceInstruction = this.Constructor.Body.Instructions[i];
                var sourceOperandAsMethodReference = sourceInstruction.Operand as MethodReference;
                if (sourceInstruction.OpCode == OpCodes.Call &&
                    sourceOperandAsMethodReference != null &&
                    sourceOperandAsMethodReference.Name == ".ctor" &&
                    (sourceOperandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.FullName ||
                    sourceOperandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.BaseType.FullName))
                {
                    boundaryInstructionIndex = i;
                    this.IsInitializingConstructor =
                        sourceOperandAsMethodReference.DeclaringType.FullName == this.Constructor.DeclaringType.BaseType.FullName;
                }
                else
                {
                    // add the source instruction to initialization instructions
                    initializationInstructions.Add(sourceInstruction);

                    // if the instruction references a variable, then move that from construction to initialization
                    var sourceVariable = sourceInstruction.Operand as VariableDefinition;
                    if (sourceVariable == null)
                    {
                        int? variableIndex;
                        if (sourceInstruction.TryGetVariableIndex(out variableIndex))
                        {
                            sourceVariable = constructionVariables.FirstOrDefault(variable => variable.Index == variableIndex.Value);
                        }
                    }

                    if (sourceVariable != null && constructionVariables.Contains(sourceVariable))
                    {
                        constructionVariables.Remove(sourceVariable);
                        initializationVariables.Add(sourceVariable);
                    }
                }
            }

            if (!boundaryInstructionIndex.HasValue || boundaryInstructionIndex.Value <= 0)
            {
                throw new InvalidOperationException("Cannot find base or forwarding constructor call in mixin implementation's parameterless constructor.");
            }

            // sanity check
            Contract.Assert(initializationInstructions.Count == boundaryInstructionIndex - 1);

            // enforce the rule about no logic in the source's default constructor
            for (int i = boundaryInstructionIndex.Value + 1; i < this.Constructor.Body.Instructions.Count; i++)
            {
                sourceInstruction = this.Constructor.Body.Instructions[i];
                if (sourceInstruction.OpCode != OpCodes.Ret && sourceInstruction.OpCode != OpCodes.Nop)
                {
                    throw new NotImplementedException();
                }
            }

            this.InitializationVariables = initializationVariables;
            this.InitializationInstructions = initializationInstructions;
            this.ConstructionVariables = constructionVariables;
            this.ConstructionInstructions = constructionInstructions;
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
        /// Gets the constructor that will be multiplexed.
        /// </summary>
        public MethodDefinition Constructor { get; private set; }

        /// <summary>
        /// Gets variables used in compiler-generated initialization code.
        /// </summary>
        public IReadOnlyList<VariableDefinition> InitializationVariables { get; private set; }

        /// <summary>
        /// Gets instructions used in compiler-generated initialization code.
        /// </summary>
        public IReadOnlyList<Instruction> InitializationInstructions { get; private set; }

        /// <summary>
        /// Gets variables used in developer-written constructor code.
        /// </summary>
        public IReadOnlyList<VariableDefinition> ConstructionVariables { get; private set; }

        /// <summary>
        /// Gets instructions used in developer-written constructor code.
        /// </summary>
        public IReadOnlyList<Instruction> ConstructionInstructions { get; private set; }
    }
}
