/***************************************************************************/
// Copyright 2013-2015 Riley White
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
    /// Contracts for <see cref="IWeave"/> implementations.
    /// </summary>
    [ContractClassFor(typeof(IWeave))]
    internal abstract class WeaveContract : IWeave
    {
        /// <summary>
        /// Contracts for <see cref="IWeave.IsInitialized"/>
        /// </summary>
        public bool IsInitialized
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Contracts for <see cref="IWeave.Initialize"/>
        /// </summary>
        /// <param name="weavingContext">Context data for command initialization.</param>
        /// <param name="weaveConfig">Configuration data for the command. Commands may require particular types for this argument that are subtypes of <see cref="WeaveConfigTypeBase"/></param>
        public void Initialize(IWeavingContext weavingContext, WeaveConfigTypeBase weaveConfig)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(!this.IsInitialized);
            Contract.Ensures(this.IsInitialized);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for <see cref="IWeave.Weave"/>
        /// </summary>
        /// <param name="weavingContext">Context data for weaving.</param>
        /// <param name="target">The type to which the weave will be applied/</param>
        /// <param name="weaveAttribute">Attribute that may contain arguments for the weave invocation.</param>
        public void Weave(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute weaveAttribute)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(target != null);
            Contract.Requires(weaveAttribute != null);
            Contract.Requires(this.IsInitialized);

            throw new NotSupportedException();
        }
    }
}
