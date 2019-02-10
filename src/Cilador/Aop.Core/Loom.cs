/***************************************************************************/
// Copyright 2013-2019 Riley White
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

using Cilador.Graph.Factory;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Cilador.Aop.Core
{
    public class Loom
    {
        public List<IAopWeavableConcept> WeavableConcepts { get; } = new List<IAopWeavableConcept>();

        public void Weave(AssemblyDefinition targetAssembly, IAssemblyResolver resolver = null, CilGraphGetter graphGetter = null)
        {
            resolver = resolver ?? targetAssembly.MainModule.AssemblyResolver;
            graphGetter = graphGetter ?? new CilGraphGetter();

            var sourceGraph = graphGetter.Get(targetAssembly);
            foreach(var concept in this.WeavableConcepts)
            {
                concept.Weave(sourceGraph);
            }
        }
    }
}
