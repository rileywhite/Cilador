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

using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Contracts for <see cref="IMemberCloner"/>.
    /// </summary>
    [ContractClassFor(typeof(IMemberCloner))]
    internal abstract class IMemberClonerContract : IMemberCloner
    {
        /// <summary>
        /// Contracts for <see cref="IMemberCloner.IsStructureCloned"/>.
        /// </summary>
        public bool IsStructureCloned { get; private set; }

        /// <summary>
        /// Contracts for <see cref="IMemberCloner.CloneStructure()"/>.
        /// </summary>
        public void CloneStructure()
        {
            Contract.Requires(!this.IsStructureCloned);
            Contract.Ensures(this.IsStructureCloned);
        }
    }
}
