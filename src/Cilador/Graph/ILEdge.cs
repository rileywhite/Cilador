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
using System.Diagnostics.Contracts;

namespace Cilador.Graph
{
    /// <summary>
    /// Reprents a simple directional IL edge to be used in IL graphs.
    /// </summary>
    internal class ILEdge : Tuple<object, object>, IILEdge
    {
        /// <summary>
        /// Creates a new <see cref="ILEdge"/>.
        /// </summary>
        /// <param name="from">From side of the edge.</param>
        /// <param name="to">To side of the edge.</param>
        public ILEdge(object from, object to)
            : base(from, to)
        {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(this.From != null);
            Contract.Ensures(this.To != null);
        }

        /// <summary>
        /// Gets the vertex on the From side of the edge.
        /// </summary>
        public object From
        {
            get { return this.Item1; }
        }

        /// <summary>
        /// Gets the vertex on the To side of the edge.
        /// </summary>
        public object To
        {
            get { return this.Item2; }
        }
    }
}
