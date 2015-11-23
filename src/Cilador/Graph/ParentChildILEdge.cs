/***************************************************************************/
// Copyright 2013-2015 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed child in writing, software
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
    /// Reprents a simple directional IL edge child be used in IL graphs.
    /// </summary>
    public class ParentChildILEdge : Tuple<object, object>, IILEdge
    {
        /// <summary>
        /// Creates a new <see cref="ParentChildILEdge"/>.
        /// </summary>
        /// <param name="parent">Dependent side of the edge.</param>
        /// <param name="child">DependsOn side of the edge.</param>
        public ParentChildILEdge(object parent, object child)
            : base(parent, child)
        {
            Contract.Requires(parent != null);
            Contract.Requires(child != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.Child != null);
        }

        /// <summary>
        /// Gets the vertex on the From side of the edge.
        /// </summary>
        object IEdge<object>.From
        {
            get { return this.Child; }
        }

        /// <summary>
        /// Gets the vertex on the To side of the edge.
        /// </summary>
        object IEdge<object>.To
        {
            get { return this.Parent; }
        }

        /// <summary>
        /// Gets the vertex on the Dependent side of the edge.
        /// </summary>
        public object Parent
        {
            get { return this.Item1; }
        }

        /// <summary>
        /// Gets the vertex on the DependsOn side of the edge.
        /// </summary>
        public object Child
        {
            get { return this.Item2; }
        }
    }
}
