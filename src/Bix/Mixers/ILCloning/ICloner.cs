﻿/***************************************************************************/
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

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Interface implemented by all item cloners.
    /// </summary>
    [ContractClass(typeof(IClonerContract))]
    public interface ICloner
    {
        /// <summary>
        /// Gets the context for IL cloning.
        /// </summary>
        IILCloningContext ILCloningContext { get; }

        /// <summary>
        /// Gets whether the item has been cloned.
        /// </summary>
        bool IsCloned { get; }

        /// <summary>
        /// Clones the item.
        /// </summary>
        void Clone();
    }
}
