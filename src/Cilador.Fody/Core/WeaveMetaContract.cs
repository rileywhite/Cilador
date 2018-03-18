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

using System;
using System.Diagnostics.Contracts;

namespace Cilador.Fody.Core
{
    /// <summary>
    /// Contracts for <see cref="IWeaveMeta"/>.
    /// </summary>
    [ContractClassFor(typeof(IWeaveMeta))]
    internal abstract class WeaveMetaContract : IWeaveMeta
    {
        /// <summary>
        /// Contracts for <see cref="IWeaveMeta.AttributeType"/>.
        /// </summary>
        public Type AttributeType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Contracts for <see cref="IWeaveMeta.ConfigType"/>.
        /// </summary>
        public Type ConfigType
        {
            get { throw new NotSupportedException(); }
        }
    }
}
