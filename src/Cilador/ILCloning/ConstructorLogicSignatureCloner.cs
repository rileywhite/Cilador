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

using Cilador.Core;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Clones the logic part of a source constructor by redirecting it into a new method.
    /// </summary>
    /// <remarks>
    /// Because the source constructor may have multiple exit points, it's not enough to simply copy instructions
    /// from the source into target constructors.
    /// The safe way to handle this is to enclose construction items into their own method and invoke that from
    /// every initializing target constructor.
    /// </remarks>
    internal class ConstructorLogicSignatureCloner : ClonerBase<MultiplexedConstructor, MethodDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="ConstructorLogicSignatureCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the root type of the cloning operation, which is the type that the cloned constructor belongs to.</param>
        /// <param name="source">Source constructor, multiplexed into the initialization and constructor logic parts.</param>
        public ConstructorLogicSignatureCloner(RootTypeCloner parent, MultiplexedConstructor source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or set the cloner for the root type of the cloning operation, which is the type that the cloned constructor belongs to.
        /// </summary>
        private RootTypeCloner Parent { get; set; }

        /// <summary>
        /// Creates the target method signature.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override MethodDefinition GetTarget()
        {
            var target = new MethodDefinition(
                string.Format("ctor_{0:N}", Guid.NewGuid()),
                MethodAttributes.Private | MethodAttributes.HideBySig,
                this.ILCloningContext.RootTarget.Module.Import(typeof(void)));
            this.Parent.Target.Methods.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the constructor logic signature.
        /// </summary>
        protected override void DoClone()
        {
            // nothing to do here
        }
    }
}
