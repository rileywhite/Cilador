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
    internal class GenericParameterCloner : ClonerBase<GenericParameter>
    {
        public GenericParameterCloner(ILCloningContext ilCloningContext, GenericParameter target, GenericParameter source)
            : base(ilCloningContext, target, source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        /// <summary>
        /// Clones the <see cref="GenericParameter"/>.
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Source.Name == this.Target.Name);

            this.Target.Attributes = this.Source.Attributes;

            // TODO research correct usage of metadata token
            // TODO research correct usage of scope

            foreach(var sourceConstraint in this.Source.Constraints)
            {
                this.Target.Constraints.Add(this.ILCloningContext.RootImport(sourceConstraint));
            }

            this.Target.CloneAllCustomAttributes(this.Source, this.ILCloningContext);

            this.IsCloned = true;
        }
    }
}
