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
    /// Base type for all cloners that clone member items.
    /// </summary>
    /// <typeparam name="TClonedItem">Resolved type of the member.</typeparam>
    public abstract class OldClonerBase<TClonedItem> : LazyClonerBase<TClonedItem>
        where TClonedItem : class
    {
        /// <summary>
        /// Creates a new <see cref="ClonerBase{TCloned}"/>
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="target">Resolved cloning target.</param>
        /// <param name="source">Resolved cloning source.</param>
        protected OldClonerBase(IILCloningContext ilCloningContext, TClonedItem source, TClonedItem target)
            : base(ilCloningContext, source, () => target, item => { })
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Source != null);

            this.ActualTarget = target;
            this.CreateAndSetTarget();
        }

        /// <summary>
        /// Gets or sets the cloning target that was passed into the constructor.
        /// </summary>
        private TClonedItem ActualTarget { get; set; }

        /// <summary>
        /// Just returns the actual cloning target item.
        /// </summary>
        /// <returns>Cloning target.</returns>
        protected override TClonedItem CreateTarget()
        {
            return this.ActualTarget;
        }
    }
}
