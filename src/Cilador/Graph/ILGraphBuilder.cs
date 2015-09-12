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

using Cilador.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using TopologicalSort;

namespace Cilador.Graph
{
    /// <summary>
    /// Traverses a tree of IL objects within a given root type
    /// collecting vertices and edges for a graph that represents IL objects
    /// and object dependencies.
    /// </summary>
    internal class ILGraphBuilder
    {
        /// <summary>
        /// Creates a new <see cref="ILGraphBuilder"/>.
        /// </summary>
        public ILGraphBuilder()
        {
            Contract.Ensures(this.ChildrenGetter != null);
            Contract.Ensures(this.DependenciesGetter != null);

            this.ChildrenGetter = new ILOrderedChildrenGetDispatcher();
            this.DependenciesGetter = new ILDependencyGetDispatcher();
        }

        /// <summary>
        /// Traverses a root type to get a graph of IL objects and their dependencies.
        /// </summary>
        /// <param name="type">Type ot get graph of items and dependencies.</param>
        /// <returns>
        /// <see cref="ILGraph"/> repesenting the items and dependencies of the given
        /// root type.
        /// </returns>
        public ILGraph Traverse(TypeDefinition type)
        {
            Contract.Requires(type != null);

            var vertices = new HashSet<object>();
            var parentChildEdges = new HashSet<Edge<object>>(new EdgeEqualityComparer());
            var siblingEdges = new HashSet<Edge<object>>(new EdgeEqualityComparer());

            // in theory, this could be called for multiple roots before collecting edges
            this.CollectVerticesAndFamilyEdgesFrom(type, vertices, parentChildEdges, siblingEdges);

            return new ILGraph(vertices, parentChildEdges, siblingEdges, this.GetDependencyEdges(vertices));
        }

        /// <summary>
        /// Gets or sets the dispatcher for getting IL item children.
        /// </summary>
        private ILOrderedChildrenGetDispatcher ChildrenGetter { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher for getting IL item dependencies.
        /// </summary>
        private ILDependencyGetDispatcher DependenciesGetter { get; set; }

        /// <summary>
        /// Gets a graph edge with the correct from/to directionality based on a parent/child relationship.
        /// </summary>
        /// <param name="parent">IL item that is the parent of the <paramref name="child"/>.</param>
        /// <param name="child">IL item that is the child of the <paramref name="parent"/>.</param>
        /// <returns>New edge.</returns>
        private static Edge<object> GetParentChildEdge(object parent, object child)
        {
            return Edge.Create(parent, child);
        }

        /// <summary>
        /// Gets a graph edge with the correct from/to directionality based on a sibling relationship.
        /// </summary>
        /// <param name="firstSibling">IL item that is the first sibling.</param>
        /// <param name="secondSibling">IL item that is the second sibling.</param>
        /// <returns>New edge.</returns>
        private static Edge<object> GetSiblingEdge(object firstSibling, object secondSibling)
        {
            return Edge.Create(firstSibling, secondSibling);
        }

        /// <summary>
        /// Gets a graph edge with the correct from/to directionality based on a directional dependency.
        /// </summary>
        /// <param name="dependent">IL item that depends on another item.</param>
        /// <param name="dependsOn">IL item that is depended upon by another item.</param>
        /// <returns>New edge.</returns>
        private static Edge<object> GetDependencyEdge(object dependent, object dependsOn)
        {
            return Edge.Create(dependent, dependsOn);
        }

        /// <summary>
        /// Collects an item and its children as graph vertices.
        /// </summary>
        /// <param name="item">Item to collect vertices from.</param>
        /// <param name="vertices">Vertices to which items should be added.</param>
        /// <param name="parentChildEdges">Parent/child relationship edges.</param>
        /// <param name="siblingEdges">Sibling relationship edges.</param>
        private void CollectVerticesAndFamilyEdgesFrom(
            object item,
            HashSet<object> vertices,
            HashSet<Edge<object>> parentChildEdges,
            HashSet<Edge<object>> siblingEdges)
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
                    parentChildEdges.Add(GetParentChildEdge(currentItem, child));
                    if (previousChild != null && previousChild.GetType().Equals(child.GetType()))
                    {
                        siblingEdges.Add(GetSiblingEdge(previousChild, child));
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
        private IEnumerable<Edge<object>> GetDependencyEdges(IEnumerable<object> vertices)
        {
            Contract.Requires(vertices != null);

            var dependencyEdges = new HashSet<Edge<object>>(new EdgeEqualityComparer());

            // loop through all vertices in the graph
            foreach(var vertex in vertices)
            {
                // get all dependencies for each vertex.
                foreach (var dependency in this.DependenciesGetter.InvokeFor(vertex))
                {
                    // add edges for dependencies that lie on the graph
                    if (vertices.Contains(dependency))
                    {
                        // since edges is a hash set with a comparer that compares
                        // to and from vertices, the collected edges will be distinct
                        dependencyEdges.Add(GetDependencyEdge(vertex, dependency));
                    }
                }
            }

            return dependencyEdges;
        }
    }
}
