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

        public void Initialize(MixCommandConfigTypeBase config)
        {
            this.Config = config as InterfaceMixConfigType;
            if(this.Config == null)
            {
                throw new ArgumentException("Must be of type InterfaceMixConfigType", "config");
            }
            this.IsInitialized = true;
        }

        private InterfaceMixConfigType Config { get; set; }

        public void Mix(TypeDefinition target, CustomAttribute mixCommandAttribute)
        {
            var module = mixCommandAttribute.AttributeType.Module;

            if (mixCommandAttribute.AttributeType.Resolve() != module.Import(typeof(InterfaceMixAttribute)).Resolve())
            {
                throw new ArgumentException("Must be a valid CustomAttribute with AttributeType InterfaceMixAttribute", "mixCommandAttribute");
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
                map => module.Import(map.InterfaceType).Resolve() == commandInterfaceType);

            new InterfaceMixCommandMixer(commandInterfaceType, module.Import(matchedMap.TemplateType).Resolve(), target).Execute();
        }
    }
}
