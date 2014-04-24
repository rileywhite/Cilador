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
    [ContractClassFor(typeof(IMixCommand))]
    internal abstract class IMixCommandContract : IMixCommand
    {
        public bool IsInitialized
        {
            get { throw new NotSupportedException(); }
        }

        public void Initialize(IWeavingContext weavingContext, MixCommandConfigTypeBase config)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(config != null);
            Contract.Requires(!this.IsInitialized);
            Contract.Ensures(this.IsInitialized);

            throw new NotSupportedException();
        }

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
