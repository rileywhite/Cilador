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

namespace Cilador.Clone
{
    /// <summary>
    /// Defines an interface for cloners with a method body context
    /// that provide access to the <see cref="MethodBody.ThisParameter"/>
    /// of the target method.
    /// </summary>
    [ContractClass(typeof(CloneToMethodBodyContract<>))]
    internal interface ICloneToMethodBody<out TSource> : ICloner<TSource, MethodBody>
        where TSource : class
    {
        /// <summary>
        /// Gets the <see cref="ParameterDefinition"/>, if any, that
        /// instructions should map to the <see cref="MethodBody.ThisParameter"/>
        /// of the target method body. Generally this will be the ThisParameter
        /// of a source method body.
        /// </summary>
        ParameterDefinition SourceThisParameter { get; }

        /// <summary>
        /// Gets the offset for  use in instruction cloning so that referenced variables can
        /// be translated. Normally zero, but in cases where a method is split up, such as for
        /// some constructors, variables may also be split up. This may be set to a non-zero
        /// value for cloners that are cloning only a subset of instructions and variables.
        /// </summary>
        int GetVariableTranslation(Instruction sourceInstruction);

        /// <summary>
        /// Gets the action that should be used for inserting instructions for cloning instructions contained in the method.
        /// </summary>
        Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction> InstructionInsertAction { get; }

        /// <summary>
        /// Collection of source variables that may be referenced by source instructions
        /// that will be cloned to the target. This may or may not be all variables
        /// as method cloning may split methods into parts, as is the case for some
        /// constructors.
        /// </summary>
        IEnumerable<VariableDefinition> PossiblyReferencedVariables { get; }

        /// <summary>
        /// Determines whether the given instruction is a valid source instruction for the cloner
        /// that should be cloned to a target instruction.
        /// </summary>
        /// <param name="instruction">Instruction to examine.</param>
        /// <returns><c>true</c> if <paramref name="instruction"/> is a valid source instruction that should be cloned, else <c>false</c>.</returns>
        bool IsValidSourceInstruction(Instruction instruction);
    }
}
