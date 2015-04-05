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

using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Config
{
    /// <summary>
    /// Configuration data that maps mixin definition interfaces to mixin implementations
    /// that will be used to satisfy the definition during mixing.
    /// </summary>
    public partial class InterfaceMixinMapType
    {
        /// <summary>
        /// Gets the mixin definition interface.
        /// </summary>
        /// <param name="weavingContext">Context to use for resolving the type.</param>
        /// <returns>Resolved mixin definition interface type.</returns>
        public TypeDefinition GetInterfaceType(IWeavingContext weavingContext)
        {
            Contract.Requires(weavingContext != null);
            return weavingContext.GetTypeDefinition(this.Interface);
        }

        /// <summary>
        /// Gets the mixin implementation type.
        /// </summary>
        /// <param name="weavingContext">Context to use for resolving the type.</param>
        /// <returns>Resolved mixin definition type.</returns>
        public TypeDefinition GetMixinType(IWeavingContext weavingContext)
        {
            Contract.Requires(weavingContext != null);
            return weavingContext.GetTypeDefinition(this.Mixin);
        }
    }
}
