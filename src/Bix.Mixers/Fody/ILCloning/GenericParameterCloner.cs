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
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
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
        /// <param name="targetGetter">Getter method for the target.</param>
        /// <param name="targetSetter">Setter method for the target.</param>
        /// <param name="sourceGetter">Getter method for the source.</param>
        public GenericParameterCloner(
            ILCloningContext ilCloningContext,
            Func<GenericParameter> targetGetter,
            Action<GenericParameter> targetSetter,
            Func<GenericParameter> sourceGetter)
            : base(
            ilCloningContext,
            new LazyAccessor<GenericParameter>(getter: targetGetter, setter: targetSetter),
            new LazyAccessor<GenericParameter>(getter: sourceGetter))
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(targetGetter != null);
            Contract.Requires(targetSetter != null);
            Contract.Requires(sourceGetter != null);
        }

        /// <summary>
        /// Clones the <see cref="GenericParameter"/>.
        /// </summary>
        public override void Clone()
        {
            var sourceGenericParameter = this.Source.Getter();
            if (sourceGenericParameter == null) { throw new InvalidOperationException("Unable to retrieve a generic parameter using a source getter method."); }

            var targetGenericParameter = new GenericParameter(
                sourceGenericParameter.Name,
                this.ILCloningContext.DynamicRootImport(sourceGenericParameter.Owner));
            this.Target.Setter(targetGenericParameter);

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
