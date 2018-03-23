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

using Cilador.Fody.Config;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Fody.Core
{

    /// <summary>
    /// Interface that must be implemented for all weaves.
    /// </summary>
    [ContractClass(typeof(WeaveContract))]
    public interface IWeave
    {
        /// <summary>
        /// Gets whether the command has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        /// <param name="weavingContext">Context data for command initialization.</param>
        /// <param name="weaveConfig">Configuration data for the command. Commands may require particular types for this argument that are subtypes of <see cref="WeaveConfigTypeBase"/></param>
        void Initialize(IWeavingContext weavingContext, WeaveConfigTypeBase weaveConfig);

        /// <summary>
        /// Applies the weave to a target type.
        /// </summary>
        /// <param name="weavingContext">Context data for weaving.</param>
        /// <param name="target">The type to which the weave will be applied/</param>
        /// <param name="weaveAttribute">Attribute that may contain arguments for the weave invocation.</param>
        void Weave(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute weaveAttribute);
    }
}
