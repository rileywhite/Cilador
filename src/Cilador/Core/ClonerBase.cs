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

namespace Cilador.Core
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
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        protected ClonerBase(IILCloningContext ilCloningContext, TSource source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Source != null);

            this.ILCloningContext = ilCloningContext;
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        public IILCloningContext ILCloningContext { get; private set; }

        /// <summary>
        /// Whether the target has be set from its accessor.
        /// </summary>
        public bool IsTargetSet { get; private set; }

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
                    string.Format("Failed to retrieve a cloning target for source of type [{0}].", typeof(TSource).FullName),
                    e);
            }

            if (retrievedTarget == null) { throw new InvalidOperationException("GetTarget returned a null value.");}

            this.Target = retrievedTarget;
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
        protected ClonerBase(IILCloningContext ilCloningContext, TCloned source)
            : base(ilCloningContext, source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Source != null);
        }
    }
}
