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

        public void Mix(TypeDefinition target, IMixCommandAttribute mixCommandAttribute)
        {
            var interfaceMixAttribute = mixCommandAttribute as InterfaceMixAttribute;

            if (interfaceMixAttribute == null) { throw new ArgumentException("Must be a valid InterfaceMixAttribute", "mixCommandAttribute"); }

            var matchedMap = this.Config.InterfaceMap.SingleOrDefault(
                map => map.InterfaceType.AssemblyQualifiedName == interfaceMixAttribute.Interface.AssemblyQualifiedName);

            new InterfaceMixCommandMixer(interfaceMixAttribute.Interface, matchedMap.TemplateType, target).Execute();
        }
    }
}
