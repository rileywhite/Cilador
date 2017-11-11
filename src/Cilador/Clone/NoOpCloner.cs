/***************************************************************************/
// Copyright 2013-2017 Riley White
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

namespace Cilador.Clone
{
    internal static class NoOpCloner
    {
        public static NoOpCloner<TSource, TTarget> Create<TSource, TTarget>(ICloningContext cloningContext, TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            Contract.Requires(cloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(Contract.Result<NoOpCloner<TSource, TTarget>>() != null);

            return new NoOpCloner<TSource, TTarget>(cloningContext, source, target);
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
        /// <param name="cloningContext">cloning context.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target that should already exist and that will remain unchanged.</param>
        public NoOpCloner(ICloningContext cloningContext, TSource source, TTarget target)
        {
            Contract.Requires(cloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.CloningContext != null);
            Contract.Ensures(this.Source != null);
            Contract.Ensures(this.Target != null);

            this.CloningContext = cloningContext;
            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Gets the context for cloning.
        /// </summary>
        public ICloningContext CloningContext { get; }

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
        public TSource Source { get; }

        /// <summary>
        /// Gets the cloning target.
        /// </summary>
        public TTarget Target { get; }

        /// <summary>
        /// Gets or sets the transform to apply to the target after creation
        /// </summary>
        public Action<object> TargetTransform { get; set; }
    }
}
