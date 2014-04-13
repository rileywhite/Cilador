using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;

namespace Bix.Mixers.Fody.InterfaceMixing
{
    [Export(typeof(IMixCommand))]
    [ExportMetadata("AttributeType", typeof(InterfaceMixAttribute))]
    [ExportMetadata("ConfigType", typeof(InterfaceMixConfigType))]
    internal class InterfaceMixCommand : IMixCommand
    {
        public bool IsInitialized { get; private set; }

        public void Initialize(IWeavingContext weavingContext, MixCommandConfigTypeBase config)
        {
            this.Config = config as InterfaceMixConfigType;
            if(this.Config == null)
            {
                throw new ArgumentException("Must be of type InterfaceMixConfigType", "config");
            }
            this.IsInitialized = true;
        }

        private InterfaceMixConfigType config;
        private InterfaceMixConfigType Config
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
            if (mixCommandAttribute.AttributeType.FullName != weavingContext.GetTypeDefinition(typeof(InterfaceMixAttribute)).FullName)
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
                throw new ArgumentException("Expected the constructor argument to be a TypeDefinition that represents an interface", "mixCommandAttribute");
            }

            var matchedMap = this.Config.InterfaceMap.SingleOrDefault(
                map => map.GetInterfaceType(weavingContext).FullName == commandInterfaceType.FullName);

            if(matchedMap == null)
            {
                weavingContext.LogWarning("Could not find the a configuration for the requested interface: " + commandInterfaceType.FullName);
                return;
            }

            new InterfaceMixCommandMixer(commandInterfaceType, matchedMap.GetTemplateType(weavingContext), target).Execute();
        }
    }
}
