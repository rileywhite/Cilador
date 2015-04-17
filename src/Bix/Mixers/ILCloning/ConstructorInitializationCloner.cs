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

using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones a root source type's constructor into an existing root target type's constuctor.
    /// </summary>
    internal class ConstructorInitializationCloner : ClonerBase<MultiplexedConstructor, MethodBody>
    {
        /// <summary>
        /// Creates a new <see cref="ConstructorInitializationCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the parent root type for the constructor being cloned.</param>
        /// <param name="logicSignatureCloner">Cloner for the signature of the logic portion of the constructor, if any.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public ConstructorInitializationCloner(
            RootTypeCloner parent,
            ConstructorLogicSignatureCloner logicSignatureCloner,
            MultiplexedConstructor source,
            MethodBody target)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.ExistingTarget != null);

            this.Parent = parent;
            this.LogicSignatureCloner = logicSignatureCloner;
            this.ExistingTarget = target;
        }

        /// <summary>
        /// Gets or sets the cloner for the parent root type for the constructor being cloned.
        /// </summary>
        private RootTypeCloner Parent { get; set; }

        /// <summary>
        /// Gets or sets the cloner for the signature of the logic portion of the constructor, if any.
        /// </summary>
        public ConstructorLogicSignatureCloner LogicSignatureCloner { get; private set; }

        /// <summary>
        /// Gets or sets the pre-existing target method body.
        /// </summary>
        private MethodBody ExistingTarget { get; set; }

        /// <summary>
        /// Retrieves the existing constructor into which the source's data will be added.
        /// </summary>
        /// <returns>Target constructor method body.</returns>
        protected override MethodBody GetTarget()
        {
            return this.ExistingTarget;
        }

        /// <summary>
        /// Clones the source constructor instructions into the target constructor's method body.
        /// </summary>
        protected override void DoClone()
        {
            Contract.Assert(this.Source.HasInitializationItems);

            var source = this.Source;
            var target = this.Target;

            target.InitLocals = target.InitLocals || source.InitializationVariables.Any();

            var voidTypeReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
            var targetVariableIndexBySourceVariableIndex = new Dictionary<int, int>();

            if (this.LogicSignatureCloner != null)
            {
                // we can't re-use multiplexed target constructors from initialization because they may have changed
                var targetMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, this.Target.Method);

                var boundaryInstruction =
                    this.Target.Instructions[targetMultiplexedConstructor.BoundaryLastInstructionIndex];
                var targetILProcessor = this.Target.GetILProcessor();

                // insert in reverse order
                targetILProcessor.InsertAfter(
                    boundaryInstruction,
                    targetILProcessor.Create(OpCodes.Call, this.LogicSignatureCloner.Target));
                targetILProcessor.InsertAfter(boundaryInstruction, targetILProcessor.Create(OpCodes.Ldarg_0));
            }
        }
    }
}
