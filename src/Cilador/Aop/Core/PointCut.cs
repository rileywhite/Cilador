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
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.Aop.Core
{


    /// <summary>
    /// Represents an AOP Pointcut (https://en.wikipedia.org/wiki/Pointcut).
    /// </summary>
    public class PointCut<TTarget>
    {
        public PointCut(Func<TTarget, bool> selector)
        {
            Contract.Requires(selector != null);
            Contract.Ensures(this.Selector != null);

            this.Selector = selector;
        }

        public Func<TTarget, bool> Selector { get; }

        public IEnumerable<JoinPoint<TTarget>> GetJoinPoints(ICilGraph sourceCilGraph)
        {
            return sourceCilGraph
                .Vertices
                .OfType<TTarget>()
                .Where(m => this.Selector(m))
                .Select(m => new JoinPoint<TTarget>(m));
        }
    }
}
