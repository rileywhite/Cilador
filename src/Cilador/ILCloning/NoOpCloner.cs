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
using Cilador.Core;
using System.Diagnostics.Contracts;

namespace Cilador.ILCloning
{
    internal static class NoOpCloner
    {
        public static NoOpCloner<TSource, TTarget> Create<TSource, TTarget>(IILCloningContext ilCloningContext, TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(Contract.Result<NoOpCloner<TSource, TTarget>>() != null);

            return new NoOpCloner<TSource, TTarget>(ilCloningContext, source, target);
        }
    }

    /// <summary>
    /// Cloner that does not contain any logic. The expectation is that the target
    /// will already exist. The intent is to have a valid cloner for the cloning
    /// graph without the side effect of any actual cloning logic.
    /// </summary>
    internal class NoOpCloner<TSource, TTarget> : ICloner<TSource, TTarget>
        where TSource : class
        where TTarget : class
    {
        /// <summary>
        /// Creates a new <see cref="NoOpCloner{TSource, TTarget}"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target that should already exist and that will remain unchanged.</param>
        public NoOpCloner(IILCloningContext ilCloningContext, TSource source, TTarget target)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Source != null);
            Contract.Ensures(this.Target != null);

            this.ILCloningContext = ilCloningContext;
            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Gets the context for IL cloning.
        /// </summary>
        public IILCloningContext ILCloningContext { get; private set; }

        /// <summary>
        /// Gets whether the item has been cloned.
        /// </summary>
        public bool IsCloned { get; private set; }

        /// <summary>
        /// No op. The target must already exist. It will not be modified.
        /// </summary>
        public void Clone()
        {
            this.IsCloned = true;
        }

        /// <summary>
        /// Gets the cloning source.
        /// </summary>
        public TSource Source { get; private set; }

        /// <summary>
        /// Gets the cloning target.
        /// </summary>
        public TTarget Target { get; private set; }
    }
}
