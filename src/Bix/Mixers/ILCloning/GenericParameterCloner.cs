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
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones <see cref="GenericParameter"/> contents from a source to a target.
    /// </summary>
    internal class GenericParameterCloner : ClonerBase<GenericParameter>
    {
        /// <summary>
        /// Creates a new <see cref="GenericParameterCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for member that contains the generic parameter to be cloned.</param>
        /// <param name="previous">Cloner for the generic parameter, if any, that comes before the generic parameter being cloned.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="targetGetter">Getter method for the target.</param>
        /// <param name="targetSetter">Setter method for the target.</param>
        public GenericParameterCloner(
            ICloner<IGenericParameterProvider> parent,
            GenericParameterCloner previous,
            GenericParameter source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
            this.Previous = previous;
        }

        /// <summary>
        /// Gets or sets the cloner for member that contains the generic parameter to be cloned.
        /// </summary>
        private ICloner<IGenericParameterProvider> Parent { get; set; }

        /// <summary>
        /// Gets or sets the cloner for the generic parameter, if any, that comes before the generic parameter being cloned.
        /// </summary>
        private GenericParameterCloner Previous { get; set; }

        /// <summary>
        /// Creates the cloning target.
        /// </summary>
        /// <returns>New cloning target.</returns>
        protected override GenericParameter GetTarget()
        {
            // order matters on generic parameters, so make sure they are created in order
            if (this.Previous != null) { this.Previous.EnsureTargetIsSet(); }

            // now create the generic parameter and add it to the parent collection
            var genericParameter = new GenericParameter(this.Source.Name, this.Parent.Target);
            this.Parent.Target.GenericParameters.Add(genericParameter);
            return genericParameter;
        }

        /// <summary>
        /// Clones the <see cref="GenericParameter"/>.
        /// </summary>
        protected override void DoClone()
        {
            var sourceGenericParameter = this.Source;
            var targetGenericParameter = this.Target;

            targetGenericParameter.Attributes = sourceGenericParameter.Attributes;

            foreach (var sourceConstraint in sourceGenericParameter.Constraints)
            {
                targetGenericParameter.Constraints.Add(this.ILCloningContext.RootImport(sourceConstraint));
            }
        }
    }
}
