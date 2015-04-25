using Cilador.Fody.Config;
using Cilador.Fody.Core;
using System;
using System.ComponentModel.Composition;

namespace Cilador.Fody.Projections
{
    [Export(typeof(IWeave))]
    [ExportMetadata("AttributeType", typeof(DtoProjectionAttribute))]
    [ExportMetadata("ConfigType", typeof(DtoProjectionConfigType))]
    internal class DtoProjectionWeave : IWeave
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
