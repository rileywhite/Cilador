/***************************************************************************/
// Copyright 2013-2017 Riley White
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

namespace Cilador.Graph.Core
{
    /// <summary>
    /// Used to ensure unique edges are collected in hash sets.
    /// </summary>
    internal class EdgeEqualityComparer : IEqualityComparer<ICilEdge>
    {
        /// <summary>
        /// Checks equality between two edges.
        /// </summary>
        /// <param name="x">One edge.</param>
        /// <param name="y">Another edge.</param>
        /// <returns><c>true</c> if the two edges connect the same vertices in the same direction, else <c>false</c>.</returns>
        public bool Equals(ICilEdge x, ICilEdge y)
        {
            if (x.GetType() != y.GetType()) { return false; }

            if (x.From == null || x.To == null || y.From == null || y.To == null)
            {
                throw new InvalidOperationException("Valid edges must connect non-null vertices.");
            }

            //return x.Dependent.Equals(y.Dependent) && x.DependsOn.Equals(y.DependsOn);
            return x.From == y.From && x.To == y.To;
        }

        /// <summary>
        /// Gets the hash code of an edge.
        /// </summary>
        /// <param name="obj">Edge to get hash code for.</param>
        /// <returns>Hash code for edge.</returns>
        public int GetHashCode(ICilEdge obj)
        {
            if (obj.From == null || obj.To == null)
            {
                throw new InvalidOperationException("Valid edges must connect non-null vertices.");
            }
            return obj.From.GetHashCode() ^ obj.To.GetHashCode();
        }
    }
}
