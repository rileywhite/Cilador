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
    internal class GenericParameterCloner : ClonerBase<GenericParameterAccessor>
    {
        public GenericParameterCloner(
            ILCloningContext ilCloningContext,
            Func<GenericParameter> targetGetter,
            Action<GenericParameter> targetSetter,
            Func<GenericParameter> sourceGetter)
            : this(
            ilCloningContext,
            new GenericParameterAccessor(getter: targetGetter, setter: targetSetter),
            new GenericParameterAccessor(getter: sourceGetter))
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(targetGetter != null);
            Contract.Requires(targetSetter != null);
            Contract.Requires(sourceGetter != null);
        }

        public GenericParameterCloner(ILCloningContext ilCloningContext, GenericParameterAccessor target, GenericParameterAccessor source)
            : base(ilCloningContext, target, source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(target.IsGetAccessor && target.IsSetAccessor);
            Contract.Requires(source != null);
            Contract.Requires(source.IsGetAccessor);
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
