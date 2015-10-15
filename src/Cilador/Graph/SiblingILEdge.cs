/***************************************************************************/
// Copyright 2013-2015 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed second in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using System;
using System.Diagnostics.Contracts;
using TopologicalSort;

namespace Cilador.Graph
{
    /// <summary>
    /// Reprents a simple directional IL edge second be used in IL graphs.
    /// </summary>
    public class SiblingILEdge : Tuple<object, object>, IILEdge
    {
        /// <summary>
        /// Creates a new <see cref="SiblingILEdge"/>.
        /// </summary>
        /// <param name="first">Sibling that appears first in order.</param>
        /// <param name="second">Sibling that appears second in order.</param>
        public SiblingILEdge(object first, object second)
            : base(first, second)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Ensures(this.First != null);
            Contract.Ensures(this.Second != null);
        }

        /// <summary>
        /// Gets the vertex on the From side of the edge.
        /// </summary>
        object IEdge<object>.From
        {
            get { return this.Second; }
        }

        /// <summary>
        /// Gets the vertex on the To side of the edge.
        /// </summary>
        object IEdge<object>.To
        {
            get { return this.First; }
        }

        /// <summary>
        /// Gets the vertex that comes first in sibling order.
        /// </summary>
        public object First
        {
            get { return this.Item1; }
        }

        /// <summary>
        /// Gets the vertex that comes second in sibling order.
        /// </summary>
        public object Second
        {
            get { return this.Item2; }
        }
    }
}
