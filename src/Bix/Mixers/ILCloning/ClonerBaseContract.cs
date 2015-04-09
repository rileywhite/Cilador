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

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Contracts for <see cref="ClonerBase{TItem}"/>.
    /// </summary>
    [ContractClassFor(typeof(ClonerBase<>))]
    internal abstract class ClonerBaseContract<TClonedItem> : ClonerBase<TClonedItem>
        where TClonedItem : class
    {
        /// <summary>
        /// Unused constructor for <see cref="LazyClonerBaseContract{TClonedItem}"/>.
        /// </summary>
        private ClonerBaseContract() : base(null, null) { }

        /// <summary>
        /// Contracts for <see cref="CreateTarget()"/>
        /// </summary>
        /// <returns></returns>
        protected override TClonedItem CreateTarget()
        {
            Contract.Ensures(Contract.Result<TClonedItem>() != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for <see cref="Clone()"/>.
        /// </summary>
        public override void Clone()
        {
            Contract.Requires(!this.IsCloned);
            Contract.Ensures(this.IsCloned);
            throw new NotSupportedException();
        }
    }
}
