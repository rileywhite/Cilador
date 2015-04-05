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
using Bix.Mixers.Core;
using Mono.Cecil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones <see cref="GenericParameter"/> contents from a source to a target.
    /// </summary>
    internal class GenericParameterCloner : LazyClonerBase<GenericParameter>
    {
        /// <summary>
        /// Creates a new <see cref="GenericParameterCloner"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="targetGetter">Getter method for the target.</param>
        /// <param name="targetSetter">Setter method for the target.</param>
        public GenericParameterCloner(
            ILCloningContext ilCloningContext,
            GenericParameter source,
            Func<GenericParameter> targetGetter,
            Action<GenericParameter> targetSetter)
            : base(
            ilCloningContext,
            source,
            new LazyAccessor<GenericParameter>(getter: targetGetter, setter: targetSetter))
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(targetGetter != null);
            Contract.Requires(targetSetter != null);
        }

        /// <summary>
        /// Creates the cloning target.
        /// </summary>
        /// <returns>New cloning target.</returns>
        protected override GenericParameter CreateTarget()
        {
            return new GenericParameter(
                this.Source.Name,
                this.ILCloningContext.DynamicRootImport(this.Source.Owner));
        }

        /// <summary>
        /// Clones the <see cref="GenericParameter"/>.
        /// </summary>
        public override void Clone()
        {
            var sourceGenericParameter = this.Source;
            var targetGenericParameter = this.Target;

            targetGenericParameter.Attributes = sourceGenericParameter.Attributes;

            foreach (var sourceConstraint in sourceGenericParameter.Constraints)
            {
                targetGenericParameter.Constraints.Add(this.ILCloningContext.RootImport(sourceConstraint));
            }

            targetGenericParameter.CloneAllCustomAttributes(sourceGenericParameter, this.ILCloningContext);

            this.IsCloned = true;
        }
    }
}
