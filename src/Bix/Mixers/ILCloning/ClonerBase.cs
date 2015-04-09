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
using Bix.Mixers.Core;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Provides a way to clone items in such a way that the targets are created a late as possible.
    /// </summary>
    /// <remarks>
    /// This type is not thread-safe.
    /// </remarks>
    /// <typeparam name="TCloned">Type of item to be cloned.</typeparam>
    [ContractClass(typeof(ClonerBaseContract<>))]
    public abstract class ClonerBase<TCloned> : ICloner
        where TCloned : class
    {
        /// <summary>
        /// Creates a new <see cref="ClonerBase{TCloned}"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        protected ClonerBase(
            IILCloningContext ilCloningContext,
            TCloned source)
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
        private bool IsTargetCreated { get; set; }

        /// <summary>
        /// When overridden in a subclass, this method should create the cloning target.
        /// </summary>
        /// <returns>New target item.</returns>
        protected abstract TCloned CreateTarget();

        /// <summary>
        /// Calls the abstract target creation method and sets the target.
        /// </summary>
        protected void CreateAndSetTarget()
        {
            Contract.Requires(!this.IsTargetCreated);
            Contract.Ensures(this.IsTargetCreated);
            Contract.Ensures(this.Target != null);

            TCloned target;
            try
            {
                target = this.CreateTarget();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format("Failed to create a cloning target for source of type [{0}].", typeof(TCloned).FullName),
                    e);
            }

            if (target == null) { throw new InvalidOperationException("CreateTarget returned a null value.");}

            this.Target = target;
            this.IsTargetCreated = true;
        }

        private TCloned target;
        /// <summary>
        /// Gets the resolved cloning target.
        /// </summary>
        public TCloned Target
        {
            get
            {
                if (!this.IsTargetCreated) { this.CreateAndSetTarget(); }
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
        public TCloned Source { get; set; }

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
