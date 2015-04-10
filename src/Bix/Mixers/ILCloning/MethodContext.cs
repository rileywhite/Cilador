/***************************************************************************/
// Copyright 2013-2015 Riley White
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Bix.Mixers.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// This type provides source and target context for root import of method items.
    /// </summary>
    internal class MethodContext
    {
        /// <summary>
        /// Creates a new <see cref="MethodContext"/> for a method that will be fully cloned.
        /// </summary>
        /// <param name="methodBodyCloner">Cloner for the method.</param>
        public MethodContext(MethodBodyCloner methodBodyCloner) : this(
            methodBodyCloner.ILCloningContext,
            Tuple.Create(methodBodyCloner.Source.ThisParameter, methodBodyCloner.Target.ThisParameter),
            methodBodyCloner.Parent.ParameterCloners,
            methodBodyCloner.VariableCloners,
            methodBodyCloner.InstructionCloners)
        {
            Contract.Requires(methodBodyCloner != null);
            Contract.Requires(methodBodyCloner.ILCloningContext != null);
            Contract.Ensures(this.ILCloningContext != null);
        }

        /// <summary>
        /// Creates a new <see cref="MethodContext"/> for a method that will not be
        /// fully cloned.
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="thisParameterSourceAndTarget">This parameter definition for the source and target methods.</param>
        /// <param name="parameterSourceAndTargets">Parameter definitions for the source and target methods.</param>
        /// <param name="variableSourceAndTargets">Variable definitions for the source and target methods.</param>
        /// <param name="instructionSourceAndTargets">Instructiosn for the source and target methods.</param>
        public MethodContext(
            IILCloningContext ilCloningContext,
            Tuple<ParameterDefinition, ParameterDefinition> thisParameterSourceAndTarget,
            IEnumerable<Tuple<ParameterDefinition, LazyAccessor<ParameterDefinition>>> parameterSourceAndTargets,
            IEnumerable<Tuple<VariableDefinition, LazyAccessor<VariableDefinition>>> variableSourceAndTargets,
            IEnumerable<Tuple<Instruction, LazyAccessor<Instruction>>> instructionSourceAndTargets)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(thisParameterSourceAndTarget != null);
            Contract.Requires(parameterSourceAndTargets != null);
            Contract.Requires(variableSourceAndTargets != null);
            Contract.Requires(instructionSourceAndTargets != null);
            Contract.Ensures(this.ILCloningContext != null);

            this.ILCloningContext = ilCloningContext;
            this.ThisParameterSourceAndTarget = thisParameterSourceAndTarget;
            this.ParameterSourceAndTargets = parameterSourceAndTargets;
            this.VariableSourceAndTargets = variableSourceAndTargets;
            this.InstructionSourceAndTargets = instructionSourceAndTargets;
        }

        /// <summary>
        /// Gets or sets the This parameter definition for the source and target.
        /// </summary>
        private Tuple<ParameterDefinition, ParameterDefinition> ThisParameterSourceAndTarget { get; set; }

        /// <summary>
        /// Gets or sets the parameter definitions for the source and target.
        /// </summary>
        private IEnumerable<Tuple<ParameterDefinition, LazyAccessor<ParameterDefinition>>> ParameterSourceAndTargets { get; set; }

        /// <summary>
        /// Gets or sets the variable definitions for the source and target.
        /// </summary>
        private IEnumerable<Tuple<VariableDefinition, LazyAccessor<VariableDefinition>>> VariableSourceAndTargets { get; set; }

        /// <summary>
        /// Gets or sets the instructions for the source and target.
        /// </summary>
        private IEnumerable<Tuple<Instruction, LazyAccessor<Instruction>>> InstructionSourceAndTargets { get; set; }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        public IILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Root import an instruction.
        /// </summary>
        /// <param name="source">Source instruction.</param>
        /// <returns>Target instruction corresponding to the source.</returns>
        public Instruction RootImport(Instruction source)
        {
            if (source == null) { return null; }

            var instructionCloner = this.InstructionSourceAndTargets.FirstOrDefault(cloner => cloner.Item1 == source);
            if (instructionCloner == null)
            {
                throw new InvalidOperationException("Could not root import an instruction");
            }
            return instructionCloner.Item2.Getter();
        }

        /// <summary>
        /// Root import a variable definition.
        /// </summary>
        /// <param name="source">Source variable definition.</param>
        /// <returns>Target variable definition corresponding to the source.</returns>
        public VariableDefinition RootImport(VariableDefinition source)
        {
            if (source == null) { return null; }

            var variableCloner = this.VariableSourceAndTargets.FirstOrDefault(cloner => cloner.Item1 == source);
            if (variableCloner == null)
            {
                throw new InvalidOperationException("Could not root import a variable");
            }
            return variableCloner.Item2.Getter();
        }

        /// <summary>
        /// Root import a parameter definition.
        /// </summary>
        /// <param name="source">Source parameter definition.</param>
        /// <returns>Target parameter definition corresponding to the source.</returns>
        public ParameterDefinition RootImport(ParameterDefinition source)
        {
            if (source == null) { return null; }

            if (source == this.ThisParameterSourceAndTarget.Item1)
            {
                return this.ThisParameterSourceAndTarget.Item2;
            }

            var parameterCloner =
                this.ParameterSourceAndTargets.FirstOrDefault(cloner => cloner.Item1 == source);
            if (parameterCloner == null)
            {
                throw new InvalidOperationException("Could not root import a variable a parameter definition.");
            }

            return parameterCloner.Item2.Getter();
        }
    }
}
