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

using Cilador.Aop.Core;
using Cilador.Clone;
using Cilador.Graph.Factory;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Aop.IntroduceType
{
    public class TypeIntroduction : IConceptWeaver<ModuleDefinition>
    {
        /// <summary>
        /// Introduces a type into an assembly that will be cloned from a source type.
        /// </summary>
        /// <param name="graphGetter">Allows a source IL graph to be retrieved</param>
        /// <param name="sourceType">Source type from which the clone into the target assembly</param>
        /// <param name="targetTypeNamespace">Namespace of the introduced type. Defaults to the namespace of <paramref name="sourceType"/>.</param>
        /// <param name="targetTypeName">Name of the introduced type. Defaults to the name of <paramref name="sourceType"/>.</param>
        public TypeIntroduction(CilGraphGetter graphGetter, TypeDefinition sourceType, string targetTypeNamespace = null, string targetTypeName = null)
        {
            Contract.Requires(graphGetter != null);
            Contract.Requires(sourceType != null);
            Contract.Requires(targetTypeNamespace == null || !string.IsNullOrWhiteSpace(targetTypeNamespace));
            Contract.Ensures(this.GraphGetter != null);
            Contract.Ensures(this.SourceType != null);
            Contract.Ensures(this.TargetTypeNamespace != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(this.TargetTypeName));

            this.GraphGetter = graphGetter;
            this.SourceType = sourceType;
            this.TargetTypeNamespace = targetTypeNamespace ?? sourceType.Namespace;
            this.TargetTypeName = targetTypeName ?? sourceType.Name;
        }

        public CilGraphGetter GraphGetter { get; }
        public TypeDefinition SourceType { get; }
        public string TargetTypeNamespace { get; }
        public string TargetTypeName { get; }

        public void Weave(ModuleDefinition target)
        {
            Contract.Requires(this.SourceType != null);
            Contract.Requires(this.TargetTypeNamespace != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(this.TargetTypeName));

            var sourceGraph = this.GraphGetter.Get(this.SourceType);
            var targetType = new TypeDefinition(this.TargetTypeNamespace, this.TargetTypeName, this.SourceType.Attributes);
            target.Types.Add(targetType);
            var cloningContext = new CloningContext(sourceGraph, this.SourceType, targetType);
        }
    }
}
