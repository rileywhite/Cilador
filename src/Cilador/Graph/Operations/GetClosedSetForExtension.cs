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
using Cilador.Graph.Factory;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Graph.Operations
{
    /// <summary>
    /// Given a set of vertices and an <see cref="ICilGraph"/>, produces a new
    /// <see cref="ICilGraph"/> composed solely of vertices and edges traversable from
    /// the given vertices.
    /// </summary>
    public static class GetClosedSetForExtension
    {
        public static ICilGraph GetClosedSetForRoots(this ICilGraph sourceGraph)
        {
            return sourceGraph.GetClosedSetFor(sourceGraph.Roots);
        }

        /// <summary>
        /// Given a source graph and a set of start items, returns a minimal subset of the source
        /// graph that contains all start items
        /// </summary>
        /// <param name="sourceGraph">Source graph for the crop operation</param>
        /// <param name="startItems">Vertices of <paramref name="sourceGraph"/> which form the basis of the crop operation</param>
        /// <returns></returns>
        public static ICilGraph GetClosedSetFor(this ICilGraph sourceGraph, IEnumerable<object> startItems)
        {
            Contract.Requires(sourceGraph != null);
            Contract.Ensures(Contract.Result<ICilGraph>() != null);

            if (startItems == null || !startItems.Any())
            {
                return new CilGraph(new object[0], new ParentChildCilEdge[0], new SiblingCilEdge[0], new DependencyCilEdge[0]);
            }

            if (startItems.Any(item => !sourceGraph.ContainsVertex(item)))
            {
                throw new ArgumentException("All start items must be vertices in the source graph.", nameof(startItems));
            }

            // perform a search for all vertices that will be included in the 
            var vertices = sourceGraph.FindMinimalVerticesSubset(startItems);

            // dependencies are maintained, and all member dependencies are included
            var depedencyEdges =
                from edge in sourceGraph.DependencyEdges
                where vertices.Contains(edge.Dependent)
                select new DependencyCilEdge(edge.Dependent, edge.DependsOn);

            // parent/child relationships are maintained, and all member children are included
            var parentChildEdges =
                from edge in sourceGraph.ParentChildEdges
                where vertices.Contains(edge.Parent)
                select new ParentChildCilEdge(edge.Parent, edge.Child);

            // sibling edges maintain order
            // sibling relationships only make sense when a parent is included, and all siblings within an included parent are included
            var siblingEdges =
                from edge in sourceGraph.SiblingEdges
                where vertices.Contains(edge.First) && vertices.Contains(edge.Second)
                select new SiblingCilEdge(edge.First, edge.Second);

            return new CilGraph(vertices, parentChildEdges, siblingEdges, depedencyEdges);
        }

        /// <summary>
        /// Retrieves the minimal subset of vertices from the <paramref name="sourceGraph"/>
        /// that are required to fully support the <paramref name="startItems"/>.
        /// </summary>
        /// <param name="sourceGraph">Graph from which the vertices will be pulled</param>
        /// <param name="startItems">Items which form the starting set of the subset</param>
        /// <returns></returns>
        private static HashSet<object> FindMinimalVerticesSubset(
            this ICilGraph sourceGraph,
            IEnumerable<object> startItems)
        {
            Contract.Requires(sourceGraph != null);
            Contract.Requires(startItems != null);
            Contract.Ensures(Contract.Result<HashSet<object>>() != null);

            var vertices = new HashSet<object>();

            var stack = new Stack<object>(startItems);
            while (stack.Count > 0)
            {
                var currentItem = stack.Pop();

                // skip this item if it's already been added to the vertices
                if (!vertices.Add(currentItem)) { continue; }

                // include dependencies of included items
                foreach (var dependency in
                    from edge in sourceGraph.DependencyEdges
                    where edge.Dependent == currentItem && !vertices.Contains(edge.DependsOn)
                    select edge.DependsOn)
                {
                    stack.Push(dependency);
                }

                // include children of included items
                foreach (var child in
                    from edge in sourceGraph.ParentChildEdges
                    where edge.Parent == currentItem && !vertices.Contains(edge.Child)
                    select edge.Child)
                {
                    stack.Push(child);
                }
            }
            return vertices;
        }
    }
}
