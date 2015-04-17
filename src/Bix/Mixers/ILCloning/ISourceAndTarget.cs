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

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Defines a contract for holding a source and a target item.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <typeparam name="TTarget">Type of target.</typeparam>
    internal interface ISourceAndTarget<out TSource, out TTarget>
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        TSource Source { get; }

        /// <summary>
        /// Gets the target.
        /// </summary>
        TTarget Target { get; }
    }

    /// <summary>
    /// Defines a contract for holding a source and a target item.
    /// </summary>
    /// <typeparam name="T">Type of source and target.</typeparam>
    internal interface ISourceAndTarget<out T> : ISourceAndTarget<T, T> { }
}
