using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    [ContractClassFor(typeof(IMemberCloner))]
    internal abstract class IMemberClonerContract : IMemberCloner
    {
        public bool IsStructureCloned { get; private set; }

        public void CloneStructure()
        {
            Contract.Requires(!this.IsStructureCloned);
            Contract.Ensures(this.IsStructureCloned);
        }
    }
}
