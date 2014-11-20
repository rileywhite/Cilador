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
    /// Contracts for <see cref="IMixCommand"/> implementations.
    /// </summary>
    [ContractClassFor(typeof(IMixCommand))]
    internal abstract class IMixCommandContract : IMixCommand
    {
        /// <summary>
        /// Contracts for <see cref="IMixCommand.IsInitialized"/>
        /// </summary>
        public bool IsInitialized
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Contracts for <see cref="IMixCommand.Initialize"/>
        /// </summary>
        /// <param name="weavingContext">Context data for command initialization.</param>
        /// <param name="config">Configuration data for the command. Commands may require particular types for this argument that are subtypes of <see cref="MixCommandConfigTypeBase"/></param>
        public void Initialize(IWeavingContext weavingContext, MixCommandConfigTypeBase config)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(config != null);
            Contract.Requires(!this.IsInitialized);
            Contract.Ensures(this.IsInitialized);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for <see cref="IMixCommand.Mix"/>
        /// </summary>
        /// <param name="weavingContext">Context data for mixing.</param>
        /// <param name="target">The type to which the mix action will be applied/</param>
        /// <param name="mixCommandAttribute">Attribute that may contain arguments for the mix command invocation.</param>
        public void Mix(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute mixCommandAttribute)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(target != null);
            Contract.Requires(mixCommandAttribute != null);
            Contract.Requires(this.IsInitialized);

            throw new NotSupportedException();
        }
    }
}
