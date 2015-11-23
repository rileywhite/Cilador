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

using Cilador.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TopologicalSort;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Represents a projection of a source IL graph to a target IL graph.
    /// </summary>
    internal class ILProjection
    {
        /// <summary>
        /// Creates a new <see cref="ILProjection"/>.
        /// </summary>
        /// <param name="source">Source IL graph for the projection.</param>
        /// <param name="target">Target IL graph for the projection.</param>
        /// <param name="projectionEdges">Edges mapping source graph vertices to projection graph vertices.</param>
        public ILProjection(ILGraph source, ILGraph target, IEnumerable<IILEdge> projectionEdges)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Requires(projectionEdges != null);
            Contract.Ensures(this.Source != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.ProjectionEdges != null);
            Contract.Ensures(this.TargetItemsBySourceItem != null);
            Contract.Ensures(this.SourceItemByTargetItem != null);

            this.Source = source;
            this.Target = target;
            this.ProjectionEdges = projectionEdges;

            this.ProcessProjectionEdges();
        }

        /// <summary>
        /// Processes projection edges for indexing and validation purposes.
        /// </summary>
        private void ProcessProjectionEdges()
        {
            Contract.Ensures(this.TargetItemsBySourceItem != null);
            Contract.Ensures(this.SourceItemByTargetItem != null);

            var targetItemsBySourceItem = new Dictionary<object, List<object>>(this.Source.VertexCount);
            var sourceItemByTargetItem = new Dictionary<object, object>(this.Target.VertexCount);

            foreach (var projectionEdge in this.ProjectionEdges)
            {
                sourceItemByTargetItem.Add(projectionEdge.To, projectionEdge.From);

                List<object> targetItems;
                if (!targetItemsBySourceItem.TryGetValue(projectionEdge.From, out targetItems))
                {
                    targetItems = new List<object>();
                    targetItemsBySourceItem.Add(projectionEdge.From, targetItems);
                }
                Contract.Assume(targetItems != null);
                targetItems.Add(projectionEdge.To);
            }

            if (targetItemsBySourceItem.Count != this.Source.VertexCount)
            {
                throw new InvalidOperationException("Did not find at least one projection edge for every source vertex.");
            }

            if (sourceItemByTargetItem.Count != this.Target.VertexCount)
            {
                throw new InvalidOperationException("Did not find exactly one projection edge for every target vertex.");
            }

            this.TargetItemsBySourceItem = targetItemsBySourceItem;
            this.SourceItemByTargetItem = sourceItemByTargetItem;
        }
        
        /// <summary>
        /// Gets or sets the source IL graph for the projection.
        /// </summary>
        public ILGraph Source { get; private set; }

        /// <summary>
        /// Gets or sets the target IL graph for the projection.
        /// </summary>
        public ILGraph Target { get; private set; }

        /// <summary>
        /// Gets or sets edges from source IL items to target IL items.
        /// </summary>
        public IEnumerable<IILEdge> ProjectionEdges { get; private set; }

        /// <summary>
        /// Gets or sets the collection of target items keyed by the source item.
        /// </summary>
        private IReadOnlyDictionary<object, List<object>> TargetItemsBySourceItem { get; set; }

        /// <summary>
        /// Gets the collection of targets for a given source item.
        /// </summary>
        /// <param name="sourceItem">Source IL item to find targets for.</param>
        /// <returns>Collection of target projection items for the <paramref name="sourceItem"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sourceItem"/> is not a node in <see cref="Source"/>.
        /// </exception>
        public IEnumerable<object> GetTargetItemsFor(object sourceItem)
        {
            Contract.Requires(sourceItem != null);

            List<object> targetItems;
            if (this.TargetItemsBySourceItem.TryGetValue(sourceItem, out targetItems)) { return targetItems; }

            throw new ArgumentException("Requested source item not found in the source graph.", "sourceItem");
        }

        /// <summary>
        /// Gets or sets the source item keyed by each target item.
        /// </summary>
        private IReadOnlyDictionary<object, object> SourceItemByTargetItem { get; set; }

        /// <summary>
        /// Gets the collection of targets for a given source item.
        /// </summary>
        /// <param name="targetItem">Target item to find source for.</param>
        /// <returns>Source item that was projected to the <paramref name="targetItem"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="targetItem"/> is not a node in <see cref="Target"/>.
        /// </exception>
        public object GetSoureItemFor(object targetItem)
        {
            Contract.Requires(targetItem != null);

            object sourceItem;
            if (this.SourceItemByTargetItem.TryGetValue(targetItem, out sourceItem)) { return sourceItem; }

            throw new ArgumentException("Requested target item not found in the target graph.", "targetItem");
        }
    }
}
