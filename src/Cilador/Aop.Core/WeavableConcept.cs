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
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Aop.Core
{
    /// <summary>
    /// An implementation of <see cref="IAopWeavableConcept"/> that can apply a
    /// <see cref="WeavableConcept{TTarget}"/> to <see cref="ICilGraph"/> instances
    /// to points identified by a <see cref="PointCut{TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class WeavableConcept<TTarget> : IAopWeavableConcept
    {
        /// <summary>
        /// Creates a new <see cref="WeavableConcept{TTarget}"/> which
        /// can apply a given <see cref="IConceptWeaver{TTarget}"/> to target CIL
        /// graphs as filtered by a given <see cref="PointCut{TTarget}"/>
        /// </summary>
        /// <param name="pointCut">Point cut that will be applied to a CIL graph to select weaving concept targets.</param>
        /// <param name="conceptWeaver">Weaving concept that will be applied at each item selected by the <paramref name="pointCut"/>.</param>
        public WeavableConcept(PointCut<TTarget> pointCut, IConceptWeaver<TTarget> conceptWeaver)
        {
            Contract.Requires(pointCut != null);
            Contract.Requires(conceptWeaver != null);
            Contract.Ensures(this.PointCut != null);
            Contract.Ensures(this.ConceptWeaver != null);

            this.PointCut = pointCut;
            this.ConceptWeaver = conceptWeaver;
        }

        public PointCut<TTarget> PointCut { get; }
        public IConceptWeaver<TTarget> ConceptWeaver { get; }

        /// <summary>
        /// Applies an IL modification to the given source graph.
        /// </summary>
        /// <param name="targetCilGraph">Graph to which an IL modification will be applied.</param>
        /// <remarks>
        /// The <see cref="PointCut"/> will be applied to the <paramref name="targetCilGraph"/>, and
        /// each identified target will have the <see cref="ConceptWeaver"/> applied.
        /// </remarks>
        public void Weave(ICilGraph targetCilGraph)
        {
            foreach (var joinPoint in this.PointCut.GetJoinPoints(targetCilGraph))
            {
                joinPoint.Weave(this.ConceptWeaver);
            }
        }
    }
}
