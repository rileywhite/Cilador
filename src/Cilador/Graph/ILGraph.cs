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

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using TopologicalSort;

namespace Cilador.Graph
{
    /// <summary>
    /// Represents IL as a directed acyclic graph.
    /// </summary>
    internal class ILGraph
    {
        /// <summary>
        /// Creates a new <see cref="ILGraph"/> with the given vertices and edges.
        /// </summary>
        /// <param name="vertices">Vertices of the graph.</param>
        /// <param name="parentChildEdges">Edges representing parent/child dependencies of the graph.</param>
        /// <param name="siblingEdges">Edges representing ordered sibling dependencies of the graph.</param>
        /// <param name="dependencyEdges">Edges repesenting dependencies of the graph.</param>
        public ILGraph(
            IEnumerable<object> vertices,
            IEnumerable<Edge<object>> parentChildEdges,
            IEnumerable<Edge<object>> siblingEdges,
            IEnumerable<Edge<object>> dependencyEdges)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(parentChildEdges != null);
            Contract.Requires(siblingEdges != null);
            Contract.Requires(dependencyEdges != null);
            Contract.Ensures(this.Vertices != null);
            Contract.Ensures(this.ParentChildEdges != null);
            Contract.Ensures(this.SiblingEdges != null);
            Contract.Ensures(this.DependencyEdges != null);

            this.Vertices = vertices;
            this.ParentChildEdges = parentChildEdges;
            this.SiblingEdges = siblingEdges;
            this.DependencyEdges = dependencyEdges;

            this.ProcessParentEdges();
            this.ProcessSiblingEdges();
        }

        /// <summary>
        /// Process parent edges to pull additional index-type information for easier lookup.
        /// </summary>
        private void ProcessParentEdges()
        {
            Contract.Requires(this.Vertices != null);
            Contract.Ensures(this.ParentByChild != null);

            var parentByChild = new Dictionary<object, object>(this.Vertices.Count());
            foreach (var edge in this.ParentChildEdges)
            {
                parentByChild.Add(edge.To, edge.From);
            }

            this.ParentByChild = parentByChild;
        }

        /// <summary>
        /// Process sibling edges to pull additional index-type information for easier lookup.
        /// </summary>
        private void ProcessSiblingEdges()
        {
            Contract.Requires(this.Vertices != null);
            Contract.Ensures(this.PreviousSiblingBySibling != null);

            var previousSiblingBySibling = new Dictionary<object, object>();
            foreach (var edge in this.SiblingEdges)
            {
                previousSiblingBySibling.Add(edge.To, edge.From);
            }

            this.PreviousSiblingBySibling = previousSiblingBySibling;
        }

        /// <summary>
        /// Represents the vertices of the graph.
        /// </summary>
        public IEnumerable<object> Vertices { get; private set; }

        /// <summary>
        /// Represents the edges of the graph for the parent/child relationship.
        /// E.g. a field F is a child of a type T, so there will be an edge
        /// from T to F to represent that relationship.
        /// </summary>
        public IEnumerable<Edge<object>> ParentChildEdges { get; private set; }

        /// <summary>
        /// Gets or sets the lookup for finding parent items of child items.
        /// </summary>
        private IReadOnlyDictionary<object, object> ParentByChild { get; set; }

        /// <summary>
        /// Try to get the parent of a given child item.
        /// </summary>
        /// <param name="child">Child to find parent for.</param>
        /// <param name="parent">To be populated with the parent, if found.</param>
        /// <returns><c>true</c> if the parent was found, else <c>false</c>.</returns>
        public bool TryGetParentFor(object child, out object parent)
        {
            return this.ParentByChild.TryGetValue(child, out object parent);
        }

        /// <summary>
        /// Represents the edges of the graph for the sibling ordering relationships.
        /// E.g. a parameter A comes before parameter B, so there will be an edge
        /// from A to B.
        /// </summary>
        /// <remarks>
        /// There will also be non-meaningful edges here. So, for example, order
        /// doesn't matter between fields, but the implicit order in which they are
        /// generated will be enshrined in an edge, anyway.
        /// </remarks>
        public IEnumerable<Edge<object>> SiblingEdges { get; private set; }

        /// <summary>
        /// Gets or sets the lookup for finding previous sibling of sibling items.
        /// </summary>
        private IReadOnlyDictionary<object, object> PreviousSiblingBySibling { get; set; }

        /// <summary>
        /// Try to get the previous sibling of a given sibling item.
        /// </summary>
        /// <param name="sibling">Sibling to find previous sibling of.</param>
        /// <param name="previousSibling">To be populated with the previous sibling, if found.</param>
        /// <returns><c>true</c> if the previous sibling was found, else <c>false</c>.</returns>
        public bool TryGetParentFor(object sibling, out object previousSibling)
        {
            return this.ParentByChild.TryGetValue(child, out object parent);
        }

        /// <summary>
        /// Represents the edges of the graph for dependencies between items.
        /// E.g. a generic type T depends on a generic parameter G, so there will
        /// be an edge from T to G.
        /// </summary>
        public IEnumerable<Edge<object>> DependencyEdges { get; private set; }
    }
}
