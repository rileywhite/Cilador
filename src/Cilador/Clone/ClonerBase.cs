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
    /// <summary>
    /// Provides a way to clone items in such a way that the targets are created a late as possible.
    /// </summary>
    /// <remarks>
    /// This type is not thread-safe.
    /// </remarks>
    /// <typeparam name="TSource">Type of the cloning source item.</typeparam>
    /// <typeparam name="TTarget">Type of item to be cloned.</typeparam>
    [ContractClass(typeof(ClonerBaseContract<,>))]
    public abstract class ClonerBase<TSource, TTarget> : ICloner<TSource, TTarget>
        where TSource : class
        where TTarget : class
    {
        /// <summary>
        /// Creates a new <see cref="ClonerBase{TSource, TTarget}"/>.
        /// </summary>
        /// <param name="cloningContext">cloning context.</param>
        /// <param name="source">Cloning source.</param>
        protected ClonerBase(ICloningContext cloningContext, TSource source)
        {
            Contract.Requires(cloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.CloningContext != null);
            Contract.Ensures(this.Source != null);

            this.CloningContext = cloningContext;
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the context for cloning.
        /// </summary>
        public ICloningContext CloningContext { get; }

        /// <summary>
        /// Gets or sets the transform to apply to the target after creation
        /// </summary>
        public Action<object> TargetTransform { get; set; }

        /// <summary>
        /// Whether the target has be set from its accessor.
        /// </summary>
        public bool IsTargetSet { get; set; }

        /// <summary>
        /// When overridden in a subclass, this method should create or retrieve the cloning target.
        /// </summary>
        /// <returns>New target item.</returns>
        protected abstract TTarget GetTarget();

        /// <summary>
        /// Checks whether the cloning target is set, and, if it isn't, calls
        /// the method to retrieve and set the target.
        /// </summary>
        protected void EnsureTargetIsSet()
        {
            Contract.Ensures(this.IsTargetSet);
            if (!this.IsTargetSet) { this.RetrieveAndSetTarget(); }
        }

        /// <summary>
        /// Calls the abstract target retrieval method and sets the target.
        /// </summary>
        private void RetrieveAndSetTarget()
        {
            Contract.Requires(!this.IsTargetSet);
            Contract.Ensures(this.IsTargetSet);
            Contract.Ensures(this.Target != null);

            TTarget retrievedTarget;
            try
            {
                retrievedTarget = this.GetTarget();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve a cloning target for source of type [{typeof (TSource).FullName}].",
                    e);
            }

            this.Target = retrievedTarget ?? throw new InvalidOperationException("Retrieved target was null");
            this.TargetTransform?.Invoke(retrievedTarget);
            this.IsTargetSet = true;
        }

        private TTarget target;
        /// <summary>
        /// Gets the resolved cloning target.
        /// </summary>
        public TTarget Target
        {
            get
            {
                Contract.Ensures(Contract.Result<TTarget>() != null);
                this.EnsureTargetIsSet();
                return this.target;
            }
            private set
            {
                Contract.Requires(value != null);
                this.target = value;
            }
        }

        /// <summary>
        /// Gets the resolved cloning source.
        /// </summary>
        public TSource Source { get; set; }

        /// <summary>
        /// Gets or sets whether the item has been cloned.
        /// </summary>
        public bool IsCloned { get; private set; }

        public void Clone()
        {
            this.EnsureTargetIsSet();
            this.DoClone();
            this.IsCloned = true;
        }

        /// <summary>
        /// Clones the item from the source to the target.
        /// </summary>
        protected abstract void DoClone();
    }

    /// <summary>
    /// Simplified base type for symmetric cloners that have the same type of
    /// cloner source and target.
    /// </summary>
    /// <typeparam name="TCloned">Type of both the cloning source and target</typeparam>
    public abstract class ClonerBase<TCloned> : ClonerBase<TCloned, TCloned>, ICloner<TCloned>
        where TCloned : class
    {
        protected ClonerBase(ICloningContext cloningContext, TCloned source)
            : base(cloningContext, source)
        {
            Contract.Requires(cloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.CloningContext != null);
            Contract.Ensures(this.Source != null);
        }
    }
}
