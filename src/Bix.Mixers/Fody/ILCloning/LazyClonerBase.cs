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
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Provides a way to clone items in such a way that the targets are created a late as possible.
    /// </summary>
    /// <typeparam name="TClonedItem">Type of item to be cloned.</typeparam>
    internal abstract class LazyClonerBase<TClonedItem> : ClonerBase<LazyAccessor<TClonedItem>>
    {
        /// <summary>
        /// Creates a new <see cref="LazyClonerBase"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="targetGetter">Getter method for the target.</param>
        /// <param name="targetSetter">Setter method for the target.</param>
        /// <param name="sourceGetter">Getter method for the source.</param>
        public LazyClonerBase(
            ILCloningContext ilCloningContext,
            Func<TClonedItem> targetGetter,
            Action<TClonedItem> targetSetter,
            Func<TClonedItem> sourceGetter)
            : this(
            ilCloningContext,
            new LazyAccessor<TClonedItem>(getter: targetGetter, setter: targetSetter),
            new LazyAccessor<TClonedItem>(getter: sourceGetter))
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(targetGetter != null);
            Contract.Requires(targetSetter != null);
            Contract.Requires(sourceGetter != null);
        }

        /// <summary>
        /// Creates a new <see cref="LazyClonerBase"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="target">Target's lazy accesser. Must provide a getter and a setter.</param>
        /// <param name="source">Source's lazy accesser. Must provide a getter.</param>
        public LazyClonerBase(ILCloningContext ilCloningContext, LazyAccessor<TClonedItem> target, LazyAccessor<TClonedItem> source)
            : base(ilCloningContext, target, source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(target.IsGetAccessor && target.IsSetAccessor);
            Contract.Requires(source != null);
            Contract.Requires(source.IsGetAccessor);
        }
    }
}
