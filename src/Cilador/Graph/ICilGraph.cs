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
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cilador.Graph
{
    /// <summary>
    /// Represents CIL as a directed acyclic graph.
    /// </summary>
    [ContractClass(typeof(CilGraphContracts))]
    public interface ICilGraph
    {
        /// <summary>
        /// Represents the vertices of the graph.
        /// </summary>
        IEnumerable<object> Vertices { get; }

        /// <summary>
        /// Gets or sets the number of vertices in the graph.
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Represents the vertices of the graph that are root nodes. I.e., those with no parent.
        /// </summary>
        /// <remarks>
        /// Even though roots have no parents, it's important to note that they may have siblings,
        /// and they may have dependents. The idea of a root node only applies to the parent/child
        /// relationship.
        /// </remarks>
        IEnumerable<object> Roots { get; }

        /// <summary>
        /// Represents the edges of the graph for the parent/child relationship.
        /// E.g. a field F is a child of a type T, so there will be an edge
        /// from T to F to represent that relationship.
        /// </summary>
        IEnumerable<ParentChildCilEdge> ParentChildEdges { get; }

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
        TParent GetParentOf<TParent>(object child) where TParent : class;

        /// <summary>
        /// Try to get the parent of a given child item.
        /// </summary>
        /// <param name="child">DependsOn to find parent for.</param>
        /// <param name="parent">DependsOn be populated with the parent, if found.</param>
        /// <returns><c>true</c> if the parent was found, else <c>false</c>.</returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when the parent cannot be held in a reference of type <typeparamref name="TParent"/>.
        /// </exception>
        bool TryGetParentOf<TParent>(object child, out TParent parent) where TParent : class;

        /// <summary>
        /// Gets the depth of an item with respect to the parent/child aspect of the graph
        /// (which is actually a tree).
        /// </summary>
        /// <param name="item">Item to get depth of.</param>
        /// <returns>Depth of the item. E.g. root items are depth 0, their children are 1, etc.</returns>
        int GetDepth(object item);

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
        IEnumerable<SiblingCilEdge> SiblingEdges { get; }

        /// <summary>
        /// Get the previous sibling of a given sibling item.
        /// </summary>
        /// <typeparam name="TSibling">Type of the sibling and previous sibling.</typeparam>
        /// <param name="sibling">Sibling to find previous sibling of.</param>
        /// <returns>The previous sibling.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sibling"/> has no previous sibling.
        /// </exception>
        TSibling GetPreviousSiblingOf<TSibling>(TSibling sibling) where TSibling : class;

        /// <summary>
        /// Try to get the previous sibling of a given sibling item.
        /// </summary>
        /// <typeparam name="TSibling">Type of the sibling and previous sibling.</typeparam>
        /// <param name="sibling">Sibling to find previous sibling of.</param>
        /// <param name="previousSibling">DependsOn be populated with the previous sibling, if found.</param>
        /// <returns><c>true</c> if the previous sibling was found, else <c>false</c>.</returns>
        bool TryGetPreviousSiblingOf<TSibling>(TSibling sibling, out TSibling previousSibling) where TSibling : class;

        /// <summary>
        /// Represents the edges of the graph for dependencies between items.
        /// E.g. a generic type T depends on a generic parameter G, so there will
        /// be an edge from T to G.
        /// </summary>
        IEnumerable<DependencyCilEdge> DependencyEdges { get; }
    }
}
