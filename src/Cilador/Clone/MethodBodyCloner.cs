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
    /// Clones a method body from a source to a target.
    /// </summary>
    internal class MethodBodyCloner : ClonerBase<MethodBody>, ICloneToMethodBody<MethodBody>
    {
        /// <summary>
        /// Creates a new <see cref="MethodBodyCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the method signature to which this method body is attached.</param>
        /// <param name="source">Resolved cloning source.</param>
        public MethodBodyCloner(MethodSignatureCloner parent, MethodBody source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the method signature cloner with which this method body cloner is associated
        /// </summary>
        public MethodSignatureCloner Parent { get; private set; }

        /// <summary>
        /// Gets the This parameter of the source.
        /// </summary>
        public ParameterDefinition SourceThisParameter
        {
            get { return this.Source.ThisParameter; }
        }

        /// <summary>
        /// Gets the offset for  use in instruction cloning so that referenced variables can
        /// be translated. Normally zero, but in cases where a method is split up, such as for
        /// some constructors, variables may also be split up. This may be set to a non-zero
        /// value for cloners that are cloning only a subset of instructions and variables.
        /// </summary>
        public int GetVariableTranslation(Instruction sourceInstruction)
        {
            return 0;
        }

        /// <summary>
        /// Collection of source variables that may be referenced by source instructions
        /// that will be cloned to the target. This may or may not be all variables
        /// as method cloning may split methods into parts, as is the case for some
        /// constructors.
        /// </summary>
        public IEnumerable<VariableDefinition> PossiblyReferencedVariables
        {
            get { return this.Source.Variables; }
        }

        /// <summary>
        /// Gets the action that should be used for inserting instructions for cloning instructions contained in the method.
        /// </summary>
        public Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction> InstructionInsertAction
        {
            get { return InstructionCloner.DefaultInstructionInsertAction; }
        }

        /// <summary>
        /// Determines whether the given instruction is a valid source instruction for the cloner
        /// that should be cloned to a target instruction.
        /// </summary>
        /// <param name="instruction">Instruction to examine.</param>
        /// <returns><c>true</c> if <paramref name="instruction"/> is a valid source instruction that should be cloned, else <c>false</c>.</returns>
        public bool IsValidSourceInstruction(Instruction instruction)
        {
            return this.Source.Instructions.Contains(instruction);
        }

        /// <summary>
        /// The method body already exists, attached to the parent cloner.
        /// </summary>
        /// <returns>Dependent cloner's target body.</returns>
        protected override MethodBody GetTarget()
        {
            Contract.Ensures(this.TargetILProcessor != null);

            var target = this.Parent.Target.Body;
            this.TargetILProcessor = target.GetILProcessor();
            return target;
        }

        /// <summary>
        /// Gets or sets the <see cref="ILProcessor"/> for accesing CIL instructions.
        /// </summary>
        internal ILProcessor TargetILProcessor { get; private set; }

        /// <summary>
        /// Clones the method body from the source to the target.
        /// </summary>
        protected override void DoClone()
        {
            this.Target.InitLocals = this.Source.InitLocals;

            this.Target.MaxStackSize = this.Source.MaxStackSize;

            if (this.Source.Scope != null)
            {
                // TODO method body scope may be tough to get right
                // for now raise an exception
                throw new NotSupportedException("An unsupported configuration was detected. Please consider filing a bug report on the project's github page.");
            }
        }
    }
}
