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

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Base type for types that implement the visitor pattern to perform
    /// operations on a hierarchy of IL items that are being cloned.
    /// </summary>
    internal interface IILCloningVisitor
    {
        /// <summary>
        /// Visits the given item and all contained visitable items.
        /// </summary>
        /// <typeparam name="T">Type of item to visit</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <exception cref="ArgumentException">Raised when the type <typeparamref name="T"/> cannot be cloned.</exception>
        void Visit<T>(T visitedItem) where T : class;
    }
}
