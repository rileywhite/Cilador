/***************************************************************************/
// Copyright 2013-2014 Riley White
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

using Bix.Mixers.Fody.Config;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bix.Mixers.Fody.Core
{
    /// <summary>
    /// Interface that must be implemented for all mix commands.
    /// </summary>
    [ContractClass(typeof(IMixCommandContract))]
    public interface IMixCommand
    {
        /// <summary>
        /// Gets whether the command has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        /// <param name="weavingContext">Context data for command initialization.</param>
        /// <param name="config">Configuration data for the command. Commands may require particular types for this argument that are subtypes of <see cref="MixCommandConfigTypeBase"/></param>
        void Initialize(IWeavingContext weavingContext, MixCommandConfigTypeBase config);

        /// <summary>
        /// Invokes the mix command on a target type.
        /// </summary>
        /// <param name="weavingContext">Context data for mixing.</param>
        /// <param name="target">The type to which the mix action will be applied/</param>
        /// <param name="mixCommandAttribute">Attribute that may contain arguments for the mix command invocation.</param>
        void Mix(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute mixCommandAttribute);
    }
}
