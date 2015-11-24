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

namespace Cilador.Graph
{
    /// <summary>
    /// Traverses a tree of CIL objects within a given root type
    /// collecting vertices and edges for a graph that represents CIL objects
    /// and object dependencies.
    /// </summary>
    public class CilGraphGetter
    {
        /// <summary>
        /// Creates a new <see cref="CilGraphGetter"/>.
        /// </summary>
        public CilGraphGetter()
        {
            Contract.Ensures(this.ChildrenGetter != null);
            Contract.Ensures(this.DependenciesGetter != null);

            this.ChildrenGetter = new CilOrderedChildrenGetDispatcher();
            this.DependenciesGetter = new CilDependencyGetDispatcher();
        }

        /// <summary>
        /// Traverses a root type to get a graph of CIL objects and their dependencies.
        /// </summary>
        /// <param name="type">Type ot get graph of items and dependencies.</param>
        /// <returns>
        /// <see cref="Cgraph"/> repesenting the items and dependencies of the given
        /// root type.
        /// </returns>
        public ICgraph Traverse(TypeDefinition type)
        {
            Contract.Requires(type != null);

            var vertices = new HashSet<object>();
            var parentChildEdges = new HashSet<ParentChildCilEdge>(new EdgeEqualityComparer());
            var siblingEdges = new HashSet<SiblingCilEdge>(new EdgeEqualityComparer());

            // in theory, this could be called for multiple roots before collecting edges
            this.CollectVerticesAndFamilyEdgesFrom(type, vertices, parentChildEdges, siblingEdges);

            return new Cgraph(vertices, parentChildEdges, siblingEdges, this.GetDependencyEdges(vertices));
        }

        /// <summary>
        /// Gets or sets the dispatcher for getting CIL item children.
        /// </summary>
        private CilOrderedChildrenGetDispatcher ChildrenGetter { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher for getting CIL item dependencies.
        /// </summary>
        private CilDependencyGetDispatcher DependenciesGetter { get; set; }

        /// <summary>
        /// Collects an item and its children as graph vertices.
        /// </summary>
        /// <param name="item">Item to collect vertices from.</param>
        /// <param name="vertices">Vertices to which items should be added.</param>
        /// <param name="parentChildEdges">Dependent/child relationship edges.</param>
        /// <param name="siblingEdges">Sibling relationship edges.</param>
        private void CollectVerticesAndFamilyEdgesFrom(
            object item,
            HashSet<object> vertices,
            HashSet<ParentChildCilEdge> parentChildEdges,
            HashSet<SiblingCilEdge> siblingEdges)
        {
            Contract.Requires(item != null);
            Contract.Requires(vertices != null);
            Contract.Requires(parentChildEdges != null);

            var items = new Stack<object>();
            items.Push(item);

            // use a depth-first-search to collect all items
            do
            {
                var currentItem = items.Pop();

                if (!vertices.Add(currentItem)) { /* skip item if already in hashset */ continue; }

                object previousChild = null;
                foreach (var child in this.ChildrenGetter.InvokeFor(currentItem))
                {
                    items.Push(child);
                    parentChildEdges.Add(new ParentChildCilEdge(currentItem, child));
                    if (previousChild != null && previousChild.GetType() == child.GetType())
                    {
                        siblingEdges.Add(new SiblingCilEdge(previousChild, child));
                    }
                    previousChild = child;
                }
            } while(items.Count > 0);
        }

        /// <summary>
        /// Gets edges by looking at dependencies for all vertices in a graph.
        /// </summary>
        /// <param name="vertices">Completed collection of vertices on the graph.</param>
        /// <returns></returns>
        /// <remarks>
        /// All vertices should be collected before this method is called.
        /// </remarks>
        private IEnumerable<DependencyCilEdge> GetDependencyEdges(HashSet<object> vertices)
        {
            Contract.Requires(vertices != null);

            var dependencyEdges = new HashSet<DependencyCilEdge>(new EdgeEqualityComparer());

            // loop through all vertices in the graph
            foreach(var vertex in vertices)
            {
                // get all dependencies for each vertex.
                foreach (var dependency in this.DependenciesGetter.InvokeFor(vertex).Where(vertices.Contains))
                {
                    // since edges is a hash set with a comparer that compares
                    // to and from vertices, the collected edges will be distinct
                    dependencyEdges.Add(new DependencyCilEdge(vertex, dependency));
                }
            }

            return dependencyEdges;
        }
    }
}
