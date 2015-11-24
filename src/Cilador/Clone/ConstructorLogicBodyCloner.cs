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

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Clone
{
    /// <summary>
    /// Clones the logic part of a source constructor into a new method body.
    /// </summary>
    internal class ConstructorLogicBodyCloner :
        ClonerBase<MultiplexedConstructor, MethodBody>,
        ICloneToMethodBody<MultiplexedConstructor>
    {
        /// <summary>
        /// Creates a new <see cref="ConstructorLogicBodyCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner that creates the method signature for the logic part of the source constructor.</param>
        /// <param name="source"></param>
        public ConstructorLogicBodyCloner(ConstructorLogicSignatureCloner parent, MultiplexedConstructor source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
            Contract.Requires(parent.Source != null);
            Contract.Requires(parent.Source.Constructor != null);
            Contract.Requires(parent.Source.Constructor.Body != null);
            Contract.Requires(parent.Source.Constructor.Body.ThisParameter != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.SourceThisParameter != null);

            this.Parent = parent;
            this.SourceThisParameter = parent.Source.Constructor.Body.ThisParameter;
        }

        /// <summary>
        /// Gets or sets the cloner that creates the method signature for the logic part of the source constructor.
        /// </summary>
        private ConstructorLogicSignatureCloner Parent { get; set; }

        /// <summary>
        /// Gets or sets the This parameter of the source constructor.
        /// </summary>
        public ParameterDefinition SourceThisParameter { get; private set; }

        /// <summary>
        /// Gets the offset for  use in instruction cloning so that referenced variables can
        /// be translated. Normally zero, but in cases where a method is split up, such as for
        /// some constructors, variables may also be split up. This may be set to a non-zero
        /// value for cloners that are cloning only a subset of instructions and variables.
        /// </summary>
        public int GetVariableTranslation(Instruction sourceInstruction)
        {
            int? originalIndex;
            if (!sourceInstruction.TryGetVariableIndex(out originalIndex)) { return 0; }
            Contract.Assert(originalIndex.HasValue);

            int newIndex;
            if (!this.Source.TryGetConstructionVariableIndex(sourceInstruction, out newIndex)) { return 0; }
            return newIndex - originalIndex.Value;
        }

        /// <summary>
        /// Collection of source variables that may be referenced by source instructions
        /// that will be cloned to the target. This may or may not be all variables
        /// as method cloning may split methods into parts, as is the case for some
        /// constructors.
        /// </summary>
        public IEnumerable<VariableDefinition> PossiblyReferencedVariables
        {
            get { return this.Source.ConstructionVariables; }
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
            return instruction != null && this.Source.ConstructionInstructions.Contains(instruction);
        }

        private ILProcessor ilProcessor;
        /// <summary>
        /// Gets or sets the <see cref="ILProcessor"/> for accesing CIL instructions.
        /// </summary>
        internal ILProcessor TargetILProcessor
        {
            get
            {
                this.EnsureTargetIsSet();
                return this.ilProcessor;
            }
            private set { this.ilProcessor = value; }
        }

        /// <summary>
        /// Creates the target.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override MethodBody GetTarget()
        {
            Contract.Ensures(this.TargetILProcessor != null);

            var target = this.Parent.Target.Body;
            this.TargetILProcessor = target.GetILProcessor();
            return target;
        }

        /// <summary>
        /// Clones the constructor logic into a new method body.
        /// </summary>
        protected override void DoClone()
        {
            this.Target.InitLocals = this.Parent.Source.ConstructionVariables.Any();
        }
    }
}
