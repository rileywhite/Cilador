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

using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;

namespace Bix.Mixers.Fody.InterfaceMixins
{
    [Export(typeof(IMixCommand))]
    [ExportMetadata("AttributeType", typeof(InterfaceMixinAttribute))]
    [ExportMetadata("ConfigType", typeof(InterfaceMixinConfigType))]
    internal class InterfaceMixinCommand : IMixCommand
    {
        public bool IsInitialized { get; private set; }

        public void Initialize(IWeavingContext weavingContext, MixCommandConfigTypeBase config)
        {
            this.Config = config as InterfaceMixinConfigType;
            if(this.Config == null)
            {
                throw new ArgumentException("Must be of type InterfaceMixConfigType", "config");
            }
            this.IsInitialized = true;
        }

        private InterfaceMixinConfigType config;
        private InterfaceMixinConfigType Config
        {
            get { return this.config; }
            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.Config != null);

                value.InterfaceMap = value.InterfaceMap ?? new InterfaceMapType[0];
                this.config = value;
            }
        }

        public void Mix(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute mixCommandAttribute)
        {
            if (mixCommandAttribute.AttributeType.FullName != weavingContext.GetTypeDefinition(typeof(InterfaceMixinAttribute)).FullName)
            {
                throw new ArgumentException("Must be a valid CustomAttribute with AttributeType InterfaceMixAttribute",
                    "mixCommandAttribute");
            }

            if (mixCommandAttribute.ConstructorArguments.Count != 1)
            {
                throw new ArgumentException("Expected a single constructor argument", "mixCommandAttribute");
            }

            var commandInterfaceTypeReference = mixCommandAttribute.ConstructorArguments[0].Value as TypeReference;

            if (commandInterfaceTypeReference == null)
            {
                throw new ArgumentException("Expected constructor argument to be a TypeReference", "mixCommandAttribute");
            }

            var commandInterfaceType = commandInterfaceTypeReference.Resolve();

            if (!commandInterfaceType.IsInterface)
            {
                weavingContext.LogError(string.Format("Configured mixin interface type is not an interface: [{0}]", commandInterfaceType.FullName));
                // let execution continue...an error will be thrown for this particular command if invocation is attempted
            }

            var matchedMap = this.Config.InterfaceMap.SingleOrDefault(
                map => map.GetInterfaceType(weavingContext).FullName == commandInterfaceType.FullName);

            if(matchedMap == null)
            {
                weavingContext.LogWarning("Could not find the a configuration for the requested interface: " + commandInterfaceType.FullName);
                return;
            }

            new InterfaceMixinCommandMixer(commandInterfaceType, matchedMap.GetMixinType(weavingContext), target).Execute();
        }
    }
}
