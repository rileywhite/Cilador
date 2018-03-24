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
    public class WeavableConcept<TTarget> : IAopWeavableConcept
    {
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

        public void Weave(ICilGraph sourceGraph)
        {
            foreach (var joinPoint in this.PointCut.GetJoinPoints(sourceGraph))
            {
                joinPoint.Weave(this.ConceptWeaver);
            }
        }
    }
}
