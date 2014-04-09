using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    [ContractClass(typeof(IMemberClonerContract))]
    internal interface IMemberCloner
    {
        bool IsCloned { get; }

        void Clone();
    }
}
