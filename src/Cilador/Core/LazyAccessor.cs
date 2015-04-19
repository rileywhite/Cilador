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
    /// A type that allows an item to be retrieved and/or set in a late-bound fashion.
    /// </summary>
    /// <remarks>
    /// This allows code to provide Get/Set access to an item that it know how to get
    /// even if that item cannot yet be created in cases where finding the correct creation
    /// location might otherwise be difficul.
    /// </remarks>
    /// <typeparam name="T">Type of item to be accessed.</typeparam>
    public class LazyAccessor<T>
    {
        /// <summary>
        /// Creates a new <see cref="LazyAccessor{T}"/>.
        /// </summary>
        /// <param name="getter">Optional getter method if the new accessor will be a get accessor.</param>
        /// <param name="setter">Optional setter method if the new accessor will be a set accessor.</param>
        public LazyAccessor(Func<T> getter = null, Action<T> setter = null)
        {
            Contract.Requires(getter != null || setter != null);
            Contract.Ensures(this.IsGetAccessor || this.IsSetAccessor);

            this.Getter = getter;
            this.Setter = setter;
        }

        /// <summary>
        /// Gets whether this <see cref="LazyAccessor{T}"/> provides read access to an item.
        /// </summary>
        public bool IsGetAccessor
        {
            get { return this.getter != null; }
        }

        private Func<T> getter;
        /// <summary>
        /// Provides read access to an item if <see cref="IsGetAccessor"/> is <c>true</c>.
        /// </summary>
        public Func<T> Getter
        {
            get
            {
                Contract.Requires(this.IsGetAccessor);
                return this.getter;
            }
            private set { this.getter = value; }
        }

        /// <summary>
        /// Gets whether this <see cref="LazyAccessor{T}"/> provides write access to an item.
        /// </summary>
        public bool IsSetAccessor
        {
            get { return this.setter != null; }
        }

        private Action<T> setter;
        /// <summary>
        /// Provides write access to an item if <see cref="IsSetAccessor"/> is <c>true</c>.
        /// </summary>
        public Action<T> Setter
        {
            get
            {
                Contract.Requires(this.IsSetAccessor);
                return this.setter;
            }
            private set { this.setter = value; }
        }
    }
}
