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
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Base type for all cloners that clone member items.
    /// </summary>
    /// <typeparam name="TClonedItem">Resolved type of the member.</typeparam>
    internal abstract class ClonerBase<TClonedItem> : Tuple<TClonedItem, TClonedItem>, ICloner
    {
        /// <summary>
        /// Creates a new <see cref="MemberClonerBase<,>"/>
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="target">Resolved cloning target.</param>
        /// <param name="source">Resolved cloning source.</param>
        public ClonerBase(ILCloningContext ilCloningContext, TClonedItem target, TClonedItem source)
            : base(source, target)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Source != null);

            this.ILCloningContext = ilCloningContext;
            this.Target = target;
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        public ILCloningContext ILCloningContext { get; private set; }

        /// <summary>
        /// Gets or sets the resolved cloning target.
        /// </summary>
        public TClonedItem Target { get; private set; }

        /// <summary>
        /// Gets or sets the resolved cloning source.
        /// </summary>
        public TClonedItem Source { get; private set; }

        /// <summary>
        /// Gets or sets whether the item has been cloned.
        /// </summary>
        public bool IsCloned { get; protected set; }

        /// <summary>
        /// Clones the item from the source to the target.
        /// </summary>
        public abstract void Clone();
    }
}
