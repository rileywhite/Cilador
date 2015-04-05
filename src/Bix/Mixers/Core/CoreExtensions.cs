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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.Core
{
    /// <summary>
    /// Extension methods used through Bix.Mixers
    /// </summary>
    public static class CoreExtensions
    {
        /// <summary>
        /// Checks whether an enumeration contains any null items.
        /// </summary>
        /// <param name="items">Items to check. Items may be traversed.</param>
        /// <returns><c>true</c> if any null items are found, else <c>false</c>.</returns>
        [Pure]
        public static bool AreAnyNull<T>(this IEnumerable<T> items)
            where T: class
        {
            return items.Any(item => item == null);
        }
    }
}
