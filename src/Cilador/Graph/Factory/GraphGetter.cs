/***************************************************************************/
// Copyright 2013-2018 Riley White
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
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Graph.Factory
{
    /// <summary>
    /// Traverses a tree of CIL objects within a given root item
    /// collecting vertices and edges for a graph that represents CIL objects
    /// and object dependencies.
    /// </summary>
    public class CilGraphGetter
    {
        /// <summary>
        /// Traverses the assemblies at the given paths to get a graph of CIL
        /// objects contained in the assemblies
        /// </summary>
        /// <param name="assemblyPaths">Paths to assembly DLL files.</param>
        /// <returns>
        /// <see cref="CilGraph"/> repesenting the items and dependencies of the assemblies
        /// at the given locations.
        /// </returns>
        public ICilGraph Get(params string[] assemblyPaths)
        {
            return this.Get(assemblyPaths.Select(ModuleDefinition.ReadModule).Cast<object>().ToArray());
        }

        /// <summary>
        /// Traverses a set of root items to get a graph of CIL objects and their dependencies.
        /// </summary>
        /// <param name="items">CIL items to build CIL graph from.</param>
        /// <returns>
        /// <see cref="CilGraph"/> repesenting the items and dependencies of the given
        /// item.
        /// </returns>
        public ICilGraph Get(params object[] items)
        {
            var vertices = new HashSet<object>();
            var parentChildEdges = new HashSet<ParentChildCilEdge>(new EdgeEqualityComparer());
            var siblingEdges = new HashSet<SiblingCilEdge>(new EdgeEqualityComparer());

            foreach (var item in items ?? new object[0])
            {
                this.CollectVerticesAndFamilyEdgesFrom(item, vertices, parentChildEdges, siblingEdges);
            }

            return new CilGraph(vertices, parentChildEdges, siblingEdges, this.GetDependencyEdges(vertices));
        }

        /// <summary>
        /// Gets or sets the dispatcher for getting CIL item children.
        /// </summary>
        private CilOrderedChildrenGetDispatcher ChildrenGetter { get; } = new CilOrderedChildrenGetDispatcher();

        /// <summary>
        /// Gets or sets the dispatcher for getting CIL item parents.
        /// </summary>
        private CilParentGetDispatcher ParentGetter { get; } = new CilParentGetDispatcher();

        /// <summary>
        /// Gets or sets the dispatcher for getting CIL item dependencies.
        /// </summary>
        private CilDependencyGetDispatcher DependenciesGetter { get; } = new CilDependencyGetDispatcher();

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

            // use a depth-first-search to collect all items in the child tree
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

            // use a similar, but simplified strategy to collect the parent lineage
            items.Push(item);
            do
            {
                var currentItem = items.Pop();

                // always continue even if the item is already added as this is a relatively straight
                // shot up the parent line which should self-terminate after adding the assembly
                vertices.Add(currentItem);

                foreach (var parent in this.ParentGetter.InvokeFor(currentItem))
                {
                    items.Push(parent);
                    parentChildEdges.Add(new ParentChildCilEdge(parent, currentItem));
                }
            } while (items.Count > 0);
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
