using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    [ContractClassFor(typeof(IMemberCloner))]
    internal abstract class IMemberClonerContract : IMemberCloner
    {
        public bool IsCloned { get; private set; }

        public void Clone()
        {
            Contract.Requires(!this.IsCloned);
            Contract.Ensures(this.IsCloned);
        }
    }
}
