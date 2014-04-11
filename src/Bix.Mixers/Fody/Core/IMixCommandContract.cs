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
