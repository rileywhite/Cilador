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
    /// <summary>
    /// Clones a mixin implementation's default constructor into target constructors.
    /// </summary>
    /// <remarks>
    /// There are two classifications of constructors: those that call into a "base"
    /// constructor and those that call into a different "this" constructor. During
    /// object initialization, exactly one constructor that calls into a base constructor
    /// is executed, so these are the constructors that we focus on. Each of these
    /// "initializing" constructors will get code from the implementation's default
    /// constructor. They will get both initialization code, i.e. compler generated code
    /// that runs before the implementation's base constructor call such as field initialization,
    /// and constructor code, which is code compiled from programmer-written constructor
    /// code.
    /// </remarks>
    internal class ConstructorBroadcaster
    {
        /// <summary>
        /// Creates a new <see cref="ConstructorBroadcaster"/>.
        /// </summary>
        /// <param name="ilCloningContext"></param>
        /// <param name="sourceConstructor"></param>
        /// <param name="targetType"></param>
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

        /// <summary>
        /// Gets or sets the IL cloning context.
        /// </summary>
        private ILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Source default constructor that will be broadcast into target constructors.
        /// </summary>
        private MethodDefinition SourceConstructor { get; set; }

        /// <summary>
        /// Target type for the constructor broadcast.
        /// </summary>
        private TypeDefinition TargetType { get; set; }

        /// <summary>
        /// Variable cloners that will be added to the collection of all cloners.
        /// </summary>
        public List<VariableCloner> VariableCloners { get; private set; }

        /// <summary>
        /// Instruction cloners that will be added to the collection of all cloners.
        /// </summary>
        public List<InstructionCloner> InstructionCloners { get; private set; }

        /// <summary>
        /// Performs the actual work of cloning the source constructor to target constructors.
        /// </summary>
        public void BroadcastConstructor()
        {
            var multiplexedConstructor = ConstructorMultiplexer.Get(this.ILCloningContext, this.SourceConstructor);

            // we're going to insert the initializing instruction clone targets into the initializing constructors after the first instruction
            foreach (var initializingTargetConstructor in this.GetInitializingTargetConstructors())
            {
                // clone all variables
                initializingTargetConstructor.Body.InitLocals = initializingTargetConstructor.Body.InitLocals || this.SourceConstructor.Body.InitLocals;
                var preexistingVariableCount = initializingTargetConstructor.Body.Variables.Count;

                var variableCloners = new List<VariableCloner>();
                var voidTypeReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
                foreach (var sourceVariable in multiplexedConstructor.InitializationVariables)
                {
                    var targetVariable = new VariableDefinition(sourceVariable.Name, voidTypeReference);
                    initializingTargetConstructor.Body.Variables.Add(targetVariable);
                    variableCloners.Add(new VariableCloner(this.ILCloningContext, sourceVariable, targetVariable));
                }
                this.VariableCloners.AddRange(variableCloners);

                var ilProcessor = initializingTargetConstructor.Body.GetILProcessor();
                var firstInstructionInTargetConstructor = initializingTargetConstructor.Body.Instructions[0];

                // go backwards through the source initialization instructions
                // this makes it so that every new instruction is added just after the first instruction in the target
                var instructionCloners = new List<InstructionCloner>(multiplexedConstructor.InitializationInstructions.Count);
                MethodContext methodContext = new MethodContext(
                    this.ILCloningContext,
                    Tuple.Create(this.SourceConstructor.Body.ThisParameter, initializingTargetConstructor.Body.ThisParameter),
                    new List<Tuple<ParameterDefinition, LazyAccessor<ParameterDefinition>>>(),
                    variableCloners,
                    instructionCloners);
                for (int i = multiplexedConstructor.InitializationInstructions.Count - 1; i >= 0; i--)
                {
                    var sourceInstruction = multiplexedConstructor.InitializationInstructions[i].ApplyLocalVariableTranslation(preexistingVariableCount);
                    Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(methodContext, ilProcessor, sourceInstruction);
                    ilProcessor.InsertAfter(firstInstructionInTargetConstructor, targetInstruction);
                    instructionCloners.Add(new InstructionCloner(methodContext, sourceInstruction, targetInstruction));
                }
                this.InstructionCloners.AddRange(instructionCloners);
            }
        }

        /// <summary>
        /// Looks at the target types constructors, and determines which are initializing constructors,
        /// meaning that they call into the base type's constructor.
        /// </summary>
        /// <returns>Collection of initializing constructors for the target type.</returns>
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
    }
}
