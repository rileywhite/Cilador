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
using Cilador.Fody.Core;
using Mono.Cecil;
using System;
using System.ComponentModel.Composition;

namespace Cilador.Fody.DtoProjector
{

    [Export(typeof(IWeave))]
    [ExportMetadata("AttributeType", typeof(DtoProjectorAttribute))]
    internal class DtoProjectorWeave : IWeave
    {
        /// <summary>
        /// Gets or sets whether this weave has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the weave.
        /// </summary>
        /// <param name="weavingContext">Weaving context that is initializing the weave.</param>
        /// <param name="config">Must be null for this type of weave.</param>
        public void Initialize(IWeavingContext weavingContext, WeaveConfigTypeBase config)
        {
            if (config != null)
            {
                throw new ArgumentException("DtoProjector weave does not accept a config", "config");
            }
            this.IsInitialized = true;
        }

        public void Weave(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute weaveAttribute)
        {
            throw new NotImplementedException();
        }
    }
}
