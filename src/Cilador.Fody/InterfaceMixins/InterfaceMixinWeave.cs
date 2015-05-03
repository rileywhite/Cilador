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
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace Cilador.Fody.InterfaceMixins
{
    /// <summary>
    /// This weave will add interfaces to target types and will copy contents
    /// of mixin implementations into those target types.
    /// </summary>
    [Export(typeof(IWeave))]
    [ExportMetadata("AttributeType", typeof(InterfaceMixinAttribute))]
    [ExportMetadata("ConfigType", typeof(InterfaceMixinConfigType))]
    internal class InterfaceMixinWeave : IWeave
    {
        /// <summary>
        /// Gets or sets whether the command has been initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the command using context and configuration information.
        /// </summary>
        /// <param name="weavingContext">Context information for configuration.</param>
        /// <param name="weaveConfig">Command configuration information. For this command type, the value must be of type <see cref="WeaveConfigTypeBase"/>.</param>
        /// <exception cref="ArgumentException">The <paramref name="weaveConfig"/> is not of type <see cref="WeaveConfigTypeBase"/></exception>
        public void Initialize(IWeavingContext weavingContext, WeaveConfigTypeBase weaveConfig)
        {
            this.Config = weaveConfig as InterfaceMixinConfigType;
            this.IsInitialized = true;
        }

        private InterfaceMixinConfigType config;
        /// <summary>
        /// Gets or sets the strongly typed configuration for this command.
        /// </summary>
        private InterfaceMixinConfigType Config
        {
            get { return this.config; }
            set
            {
                Contract.Ensures(this.Config != null);

                value = value ?? new InterfaceMixinConfigType();
                value.InterfaceMixinMap = value.InterfaceMixinMap ?? new InterfaceMixinMapType[0];
                this.config = value;
            }
        }

        /// <summary>
        /// Applies the weave to a target type.
        /// </summary>
        /// <param name="weavingContext">Context data for weaving.</param>
        /// <param name="target">The type to which the weave will be applied/</param>
        /// <param name="weaveAttribute">Attribute that may contain arguments for the weave invocation. This must be of type <see cref="InterfaceMixinAttribute"/>.</param>
        /// <exception cref="ArgumentException">
        /// The <paramref name="weaveAttribute"/> is not of type <see cref="InterfaceMixinAttribute"/>, or it does not
        /// contain the expected argument values.
        /// </exception>
        public void Weave(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute weaveAttribute)
        {
            if (weaveAttribute.AttributeType.FullName != weavingContext.GetTypeDefinition(typeof(InterfaceMixinAttribute)).FullName)
            {
                throw new ArgumentException("Must be a valid CustomAttribute with AttributeType InterfaceMixAttribute",
                    "weaveAttribute");
            }

            if (weaveAttribute.ConstructorArguments.Count != 1)
            {
                throw new ArgumentException("Expected a single constructor argument", "weaveAttribute");
            }

            var mixinInterfaceTypeReference = weaveAttribute.ConstructorArguments[0].Value as TypeReference;

            if (mixinInterfaceTypeReference == null)
            {
                throw new ArgumentException("Expected constructor argument to be a TypeReference", "weaveAttribute");
            }

            var mixinInterfaceType = mixinInterfaceTypeReference.Resolve();

            if (!mixinInterfaceType.IsInterface)
            {
                weavingContext.LogError(string.Format("Configured mixin interface type is not an interface: [{0}]", mixinInterfaceType.FullName));
                // let execution continue...an error will be thrown for this particular weave if invocation is attempted
            }

            var matchedMap = this.Config.InterfaceMixinMap.SingleOrDefault(
                map => map.GetInterfaceType(weavingContext).FullName == mixinInterfaceType.FullName);

            if(matchedMap == null)
            {
                weavingContext.LogWarning("Could not find the a configuration for the requested interface: " + mixinInterfaceType.FullName);
                return;
            }

            new InterfaceMixinWeaver(mixinInterfaceType, matchedMap.GetMixinType(weavingContext), target).Execute();
        }
    }
}
