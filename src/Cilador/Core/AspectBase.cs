/***************************************************************************/
// Copyright 2013-2016 Riley White
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
using Cilador.Graph.Operations;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;
using System.Collections.Generic;

namespace Cilador.Core
{
    public abstract class AspectBase
    {
        public AspectBase(PointCut pointCut, MethodAdviceBase methodAdvice)
        {
            Contract.Requires(pointCut != null);
            Contract.Requires(methodAdvice != null);

            this.PointCut = pointCut;
            this.Advice = methodAdvice;
        }

        public MethodAdviceBase Advice { get; private set; }
        public PointCut PointCut { get; private set; }

        public ICilGraph ApplyTo(ICilGraph target)
        {
            Contract.Requires(target != null);
            if (target.VertexCount == 0) { return target; }

            var adviceAssembly = this.Advice.GetType().Assembly;
            var adviceModule = target.Roots.OfType<ModuleDefinition>().First().AssemblyResolver.Resolve(adviceAssembly.FullName).MainModule;
            var adviseMethod = adviceModule.GetType(this.Advice.GetType().FullName).Methods.First(method => method.Name == MethodAdviceBase.AdviseMethodName);
            var adviseSource = new CilGraphGetter().Get(adviseMethod);

            foreach (var joinPoint in this.PointCut.GetJoinPoints(target))
            {
                var targetMethod = joinPoint.Target.Module.Import(joinPoint.Target);
                target = target.Merge(
                    adviseSource,
                    new Dictionary<object, object>
                    {
                        { targetMethod.DeclaringType, joinPoint.Target.DeclaringType },
                        { adviseMethod, joinPoint.Target }
                    });
            }

            return target;
        }
    }
}
