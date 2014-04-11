using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.InterfaceMixing
{
    public partial class InterfaceMapType
    {
        public TypeDefinition GetInterfaceType(IWeavingContext weavingContext)
        {
            Contract.Requires(weavingContext != null);
            return weavingContext.GetTypeDefinition(this.Interface);
        }

        public TypeDefinition GetTemplateType(IWeavingContext weavingContext)
        {
            Contract.Requires(weavingContext != null);
            return weavingContext.GetTypeDefinition(this.Template);
        }
    }
}
