/***************************************************************************/
// Copyright 2013-2018 Riley White
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

using Cilador.Fody.Core;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Fody.InterfaceMixins
{

    /// <summary>
    /// Annotates types to which the <see cref="InterfaceMixinWeave"/> should be applied,
    /// and supplies arguments for the invocation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InterfaceMixinAttribute : Attribute, IWeaveAttribute
    {
        /// <summary>
        /// Creates a new <see cref="InterfaceMixinAttribute"/>.
        /// </summary>
        /// <param name="interface">Mixin definition interface that will be applied to the target type.</param>
        public InterfaceMixinAttribute(Type @interface)
        {
            Contract.Requires(@interface != null);
            this.Interface = @interface;
        }

        private Type @interface;
        /// <summary>
        /// Gets or sets the mixin definition interface that will be applied to the target type.
        /// </summary>
        public Type Interface
        {
            get { return this.@interface; }
            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value.IsInterface);
                this.@interface = value;
            }
        }
    }
}
