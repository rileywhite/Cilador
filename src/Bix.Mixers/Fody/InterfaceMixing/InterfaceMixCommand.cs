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
    [ExportMetadata("Name", "InterfaceMix")]
    [ExportMetadata("ConfigType", typeof(InterfaceMixConfigType))]
    internal class InterfaceMixCommand : IMixCommand
    {
        public bool IsInitialized { get; private set; }

        public void Initialize(XElement config)
        {
            this.Config = config.FromXElement<InterfaceMixConfigType>();
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
