using Cilador.Fody.Config;
using Cilador.Fody.Core;
using Cilador.Fody.InterfaceMixins;
using System;
using System.ComponentModel.Composition;

namespace Cilador.Fody.Projections
{
    //[Export(typeoIWeaveer))]
    //[ExportMetadata("AttributeType", typeof(InterfaceMixinAttribute))]
    //[ExportMetadata("ConfigType", typeof(InterfaceMixinConfigType))]
    internal class ProjectionCommand : IWeave
    {
        public bool IsInitialized
        {
            get { throw new NotImplementedException(); }
        }

        public void Initialize(IWeavingContext weavingContext, WeaveConfigTypeBase config)
        {
            throw new NotImplementedException();
        }

        public void Weave(IWeavingContext weavingContext, Mono.Cecil.TypeDefinition target, Mono.Cecil.CustomAttribute weaveAttribute)
        {
            throw new NotImplementedException();
        }
    }
}
