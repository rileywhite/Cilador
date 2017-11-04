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

using Cilador.Graph.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Graph.Factory
{
    /// <summary>
    /// Represents CIL as a directed acyclic graph.
    /// </summary>
    internal class CilGraph : ICilGraph
    {
        /// <summary>
        /// Creates a new <see cref="CilGraph"/> with the given vertices and edges.
        /// </summary>
        /// <param name="vertices">Vertices of the graph.</param>
        /// <param name="parentChildEdges">Edges representing parent/child dependencies of the graph.</param>
        /// <param name="siblingEdges">Edges representing ordered sibling dependencies of the graph.</param>
        /// <param name="dependencyEdges">Edges repesenting dependencies of the graph.</param>
        public CilGraph(
            IEnumerable<object> vertices,
            IEnumerable<ParentChildCilEdge> parentChildEdges,
            IEnumerable<SiblingCilEdge> siblingEdges,
            IEnumerable<DependencyCilEdge> dependencyEdges)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(parentChildEdges != null);
            Contract.Requires(siblingEdges != null);
            Contract.Requires(dependencyEdges != null);
            Contract.Ensures(this.Vertices != null);
            Contract.Ensures(this.ParentChildEdges != null);
            Contract.Ensures(this.DepthByVertex != null);
            Contract.Ensures(this.SiblingEdges != null);
            Contract.Ensures(this.DependencyEdges != null);
            Contract.Ensures(this.Roots != null);

            var verticesHashSet = new HashSet<object>(vertices);
            this.Vertices = verticesHashSet;
            this.VertexCount = verticesHashSet.Count;
            this.ParentChildEdges = new HashSet<ParentChildCilEdge>(parentChildEdges, new EdgeEqualityComparer());
            this.DepthByVertex = new Dictionary<object, int>();
            this.SiblingEdges = new HashSet<SiblingCilEdge>(siblingEdges, new EdgeEqualityComparer());
            this.DependencyEdges = new HashSet<DependencyCilEdge>(dependencyEdges, new EdgeEqualityComparer());

            this.ProcessParentEdges();
            this.ProcessSiblingEdges();
            this.ProcessDependencyEdges();
        }

        /// <summary>
        /// Process parent/child edges for validation and indexing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any edge has a <c>null</c> vertex.
        /// </exception>
        private void ProcessParentEdges()
        {
            Contract.Requires(this.Vertices != null);
            Contract.Requires(this.ParentChildEdges != null);
            Contract.Ensures(this.ParentByChild != null);
            Contract.Ensures(this.Roots != null);

            var roots = new HashSet<object>(this.Vertices);
            var parentByChild = new Dictionary<object, object>(this.Vertices.Count);
            foreach (var edge in this.ParentChildEdges)
            {
                if (edge.Parent == null || edge.Child == null)
                {
                    throw new InvalidOperationException("All parent edges must connect non-null vertices.");
                }
                parentByChild.Add(edge.Child, edge.Parent);
                roots.Remove(edge.Child);
            }

            this.ParentByChild = parentByChild;
            this.Roots = roots;
        }

        /// <summary>
        /// Process sibling edges for validation and indexing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any edge has a <c>null</c> vertex. Also thrown if any two connected siblings are of different types.
        /// </exception>
        private void ProcessSiblingEdges()
        {
            Contract.Requires(this.Vertices != null);
            Contract.Requires(this.SiblingEdges != null);
            Contract.Ensures(this.PreviousSiblingBySibling != null);

            var previousSiblingBySibling = new Dictionary<object, object>();
            foreach (var edge in this.SiblingEdges)
            {
                if (edge.First == null || edge.Second == null)
                {
                    throw new InvalidOperationException("All sibling edges must connect non-null vertices.");
                }
                if (edge.First.GetType() != edge.Second.GetType())
                {
                    throw new InvalidOperationException(
                        $"Sibling edges must connect vertices of the same type, but a sibling edge was found connecting {edge.First.GetType()} and {edge.Second.GetType()}.");
                }
                previousSiblingBySibling.Add(edge.Second, edge.First);
            }

            this.PreviousSiblingBySibling = previousSiblingBySibling;
        }

        /// <summary>
        /// Process dependency edges for validation and indexing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any edge has a <c>null</c> vertex.
        /// </exception>
        private void ProcessDependencyEdges()
        {
            Contract.Requires(this.Vertices != null);
            Contract.Requires(this.DependencyEdges != null);

            if (this.DependencyEdges.Any(edge => edge.Dependent == null || edge.DependsOn == null)) {
                throw new InvalidOperationException("All dependency edges must connect non-null vertices.");
            }
        }

        /// <summary>
        /// Represents the vertices of the graph.
        /// </summary>
        IEnumerable<object> ICilGraph.Vertices => this.Vertices;

        /// <summary>
        /// Represents the vertices of the graph.
        /// </summary>
        private HashSet<object> Vertices { get; }

        /// <summary>
        /// Gets or sets the number of vertices in the graph.
        /// </summary>
        public int VertexCount { get; }

        /// <summary>
        /// Determines whether a given vertex is contained within the graph.
        /// </summary>
        /// <param name="vertex">Vertex to check for.</param>
        /// <returns><c>true</c> if the graph contains the given vertex, else <c>false</c>.</returns>
        public bool ContainsVertex(object vertex)
        {
            return this.Vertices.Contains(vertex);
        }

        /// <summary>
        /// Represents the vertices of the graph that are root nodes. I.e., those with no parent.
        /// </summary>
        /// <remarks>
        /// Even though roots have no parents, it's important to note that they may have siblings,
        /// and they may have dependents. The idea of a root node only applies to the parent/child
        /// relationship.
        /// </remarks>
        public IEnumerable<object> Roots { get; private set; }

        /// <summary>
        /// Represents the edges of the graph for the parent/child relationship.
        /// E.g. a field F is a child of a type T, so there will be an edge
        /// from T to F to represent that relationship.
        /// </summary>
        public IEnumerable<ParentChildCilEdge> ParentChildEdges { get; }

        /// <summary>
        /// Gets or sets the lookup for finding parent items of child items.
        /// </summary>
        private IReadOnlyDictionary<object, object> ParentByChild { get; set; }

        /// <summary>
        /// Get the parent of a given child item.
        /// </summary>
        /// <typeparam name="TParent">Type of the parent node.</typeparam>
        /// <param name="child">DependsOn to find parent of.</param>
        /// <returns>The parent.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="child"/> has no parent, meaning it is a root node.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown when the parent cannot be held in a reference of type <typeparamref name="TParent"/>.
        /// </exception>
        public TParent GetParentOf<TParent>(object child) where TParent : class
        {
            TParent parent;
            if (!this.TryGetParentOf(child, out parent))
            {
                throw new ArgumentException(
                    $"Given child {child} is a root node with no parent.",
                    nameof(child));
            }

            Contract.Assert(parent != null);
            return parent;
        }

        /// <summary>
        /// Try to get the parent of a given child item.
        /// </summary>
        /// <param name="child">DependsOn to find parent for.</param>
        /// <param name="parent">DependsOn be populated with the parent, if found.</param>
        /// <returns><c>true</c> if the parent was found, else <c>false</c>.</returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when the parent cannot be held in a reference of type <typeparamref name="TParent"/>.
        /// </exception>
        public bool TryGetParentOf<TParent>(object child, out TParent parent)
            where TParent : class
        {
            object parentObject;
            if(!this.ParentByChild.TryGetValue(child, out parentObject))
            {
                parent = null;
                return false;
            }

            Contract.Assert(parentObject != null);
            parent = (TParent)parentObject;
            return true;
        }

        /// <summary>
        /// Gets or sets the dictinary storing depth on the parent/child aspect of the graph
        /// (which is actually a tree) keyed by the vertex.
        /// </summary>
        private IDictionary<object, int> DepthByVertex { get; }

        /// <summary>
        /// Gets the depth of an item with respect to the parent/child aspect of the graph
        /// (which is actually a tree).
        /// </summary>
        /// <param name="item">Item to get depth of.</param>
        /// <returns>Depth of the item. E.g. root items are depth 0, their children are 1, etc.</returns>
        public int GetDepth(object item)
        {
            // we'll calculate and populate depths on-demand
            int depth;
            if (this.DepthByVertex.TryGetValue(item, out depth)) { return depth; }

            var items = new Stack<object>();
            var currentItem = item;
            while (!this.DepthByVertex.ContainsKey(currentItem) && !this.Roots.Contains(currentItem))
            {
                items.Push(currentItem);
                if (items.Count > this.VertexCount)
                {
                    throw new InvalidOperationException(
                        "Found an unexpected cycle in the parent/child tree when looking for vertex depth.");
                }
                currentItem = this.GetParentOf<object>(currentItem);
            }

            // at this point, the current item is either cached or a root item, and the stack
            // contains items with increasing depths
            int currentDepth;
            if (!this.DepthByVertex.TryGetValue(currentItem, out currentDepth))
            {
                currentDepth = 0;
                this.DepthByVertex[currentItem] = 0;
            }

            while (items.Count > 0)
            {
                currentItem = items.Pop();
                ++currentDepth;
                this.DepthByVertex[currentItem] = currentDepth;
            }

            return currentDepth;
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
        public IEnumerable<SiblingCilEdge> SiblingEdges { get; }

        /// <summary>
        /// Gets or sets the lookup for finding previous sibling of sibling items.
        /// </summary>
        private IReadOnlyDictionary<object, object> PreviousSiblingBySibling { get; set; }

        /// <summary>
        /// Get the previous sibling of a given sibling item.
        /// </summary>
        /// <typeparam name="TSibling">Type of the sibling and previous sibling.</typeparam>
        /// <param name="sibling">Sibling to find previous sibling of.</param>
        /// <returns>The previous sibling.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sibling"/> has no previous sibling.
        /// </exception>
        public TSibling GetPreviousSiblingOf<TSibling>(TSibling sibling) where TSibling : class
        {
            TSibling previousSibling;
            if (!this.TryGetPreviousSiblingOf(sibling, out previousSibling))
            {
                throw new ArgumentException(
                    $"Given sibling {sibling} has no previous sibling.",
                    nameof(sibling));
            }

            Contract.Assert(previousSibling != null);
            return previousSibling;
        }

        /// <summary>
        /// Try to get the previous sibling of a given sibling item.
        /// </summary>
        /// <typeparam name="TSibling">Type of the sibling and previous sibling.</typeparam>
        /// <param name="sibling">Sibling to find previous sibling of.</param>
        /// <param name="previousSibling">DependsOn be populated with the previous sibling, if found.</param>
        /// <returns><c>true</c> if the previous sibling was found, else <c>false</c>.</returns>
        public bool TryGetPreviousSiblingOf<TSibling>(TSibling sibling, out TSibling previousSibling) where TSibling : class
        {
            object previousSiblingObject;
            if (!this.PreviousSiblingBySibling.TryGetValue(sibling, out previousSiblingObject))
            {
                previousSibling = null;
                return false;
            }

            Contract.Assert(previousSiblingObject != null);
            previousSibling = (TSibling)previousSiblingObject;
            return true;
        }

        /// <summary>
        /// Represents the edges of the graph for dependencies between items.
        /// E.g. a generic type T depends on a generic parameter G, so there will
        /// be an edge from T to G.
        /// </summary>
        public IEnumerable<DependencyCilEdge> DependencyEdges { get; }
    }
}
