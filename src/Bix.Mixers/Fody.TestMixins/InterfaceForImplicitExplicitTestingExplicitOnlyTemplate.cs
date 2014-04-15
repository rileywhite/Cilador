using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class InterfaceForImplicitExplicitTestingExplicitOnlyTemplate : IInterfaceForImplicitExplicitTesting
    {
        [Skip]
        public InterfaceForImplicitExplicitTestingExplicitOnlyTemplate() { }

        string IInterfaceForImplicitExplicitTesting.Method1()
        {
            return "Explicit 1";
        }

        string IInterfaceForImplicitExplicitTesting.Method2()
        {
            return "Explicit 2";
        }

        string IInterfaceForImplicitExplicitTesting.Method3()
        {
            return "Explicit 3";
        }
    }
}
