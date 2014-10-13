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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// A type that allows a generic parameter to be retrieved and/or set.
    /// </summary>
    /// <remarks>
    /// Because the <see cref="GenericParameter.Owner"/> cannot be set outside
    /// of the constructor of a <see cref="GenericParameter"/>, this type is
    /// used to delay construction of a final generic parameter until the cloning
    /// step.
    /// </remarks>
    public class GenericParameterAccessor
    {
        /// <summary>
        /// Creates a new <see cref="GenericParameterAccessor"/>.
        /// </summary>
        /// <param name="getter">Optional getter method if the new accessor will be a get accessor.</param>
        /// <param name="setter">Optional setter method if the new accessor will be a set accessor.</param>
        public GenericParameterAccessor(Func<GenericParameter> getter = null, Action<GenericParameter> setter = null)
        {
            Contract.Requires(getter != null || setter != null);
            Contract.Ensures(this.IsGetAccessor || this.IsSetAccessor);

            this.Getter = getter;
            this.Setter = setter;
        }

        /// <summary>
        /// Gets whether this <see cref="GenericParameterAccessor"/> provides
        /// read access to a generic parameter.
        /// </summary>
        public bool IsGetAccessor
        {
            get { return this.getter != null; }
        }

        private Func<GenericParameter> getter;
        /// <summary>
        /// Provides read access to a generic parameter if <see cref="IsGetAccessor"/>
        /// is <c>true</c>.
        /// </summary>
        public Func<GenericParameter> Getter
        {
            get
            {
                Contract.Requires(this.IsGetAccessor);
                return this.getter;
            }
            private set { this.getter = value; }
        }

        /// <summary>
        /// Gets whether this <see cref="GenericParameterAccessor"/> provides
        /// write access to a generic parameter.
        /// </summary>
        public bool IsSetAccessor
        {
            get { return this.setter != null; }
        }

        private Action<GenericParameter> setter;
        /// <summary>
        /// Provides write access to a generic parameter if <see cref="IsSetAccessor"/>
        /// is <c>true</c>.
        /// </summary>
        public Action<GenericParameter> Setter
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
