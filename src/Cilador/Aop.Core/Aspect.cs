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
    [ContractClass(typeof(AspectContracts))]
    public abstract class Aspect
    {
        public abstract void Apply(ICilGraph sourceGraph);
    }

    public class Aspect<TTarget> : Aspect
    {
        public Aspect(PointCut<TTarget> pointCut, IAdvisor<TTarget> advisor)
        {
            Contract.Requires(pointCut != null);
            Contract.Requires(advisor != null);
            Contract.Ensures(this.PointCut != null);
            Contract.Ensures(this.Advisor != null);

            this.PointCut = pointCut;
            this.Advisor = advisor;
        }

        public PointCut<TTarget> PointCut { get; }
        public IAdvisor<TTarget> Advisor { get; }

        public override void Apply(ICilGraph sourceGraph)
        {
            foreach (var joinPoint in this.PointCut.GetJoinPoints(sourceGraph))
            {
                joinPoint.ApplyAdvice(this.Advisor);
            }
        }
    }
}
