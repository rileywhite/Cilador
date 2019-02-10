/***************************************************************************/
// Copyright 2013-2019 Riley White
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
    /// Contracts for <see cref="ICloneToMethodBody{TSource}"/>.
    /// </summary>
    [ContractClassFor(typeof(ICloneToMethodBody<>))]
    internal abstract class CloneToMethodBodyContract<TSource> : ICloneToMethodBody<TSource>
        where TSource : class
    {
        /// <summary>
        /// Contracts for <see cref="ICloneToMethodBody{TSource}.SourceThisParameter"/>.
        /// </summary>
        public ParameterDefinition SourceThisParameter
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Contracts for <see cref="ICloneToMethodBody{TSource}.GetVariableTranslation"/>.
        /// </summary>
        public int GetVariableTranslation(Instruction sourceInstruction)
        {
            Contract.Requires(sourceInstruction != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for <see cref="ICloneToMethodBody{TSource}.IsValidSourceInstruction"/>.
        /// </summary>
        public bool IsValidSourceInstruction(Instruction instruction)
        {
            Contract.Requires(instruction != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for <see cref="ICloneToMethodBody{TSource}.PossiblyReferencedVariables"/>
        /// </summary>
        public IEnumerable<VariableDefinition> PossiblyReferencedVariables
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<VariableDefinition>>() != null);
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Contracts for <see cref="ICloneToMethodBody{TSource}.InstructionInsertAction"/>.
        /// </summary>
        public Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction> InstructionInsertAction
        {
            get
            {
                Contract.Assert(Contract.Result<Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction>>() != null);
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// No contracts for inherited interface items.
        /// </summary>
        public abstract ICloningContext CloningContext { get; }

        /// <summary>
        /// No contracts for inherited interface items.
        /// </summary>
        public abstract bool IsCloned { get; }

        /// <summary>
        /// No contracts for inherited interface items.
        /// </summary>
        public abstract void Clone();

        /// <summary>
        /// No contracts for inherited interface items.
        /// </summary>
        public abstract TSource Source { get; }

        /// <summary>
        /// No contracts for inherited interface items.
        /// </summary>
        public abstract MethodBody Target { get; }

        /// <summary>
        /// No contracts for target transform.
        /// </summary>
        public Action<object> TargetTransform
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
