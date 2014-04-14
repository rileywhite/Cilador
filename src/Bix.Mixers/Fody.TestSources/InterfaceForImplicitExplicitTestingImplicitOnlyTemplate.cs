using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestSources
{
    public class InterfaceForImplicitExplicitTestingImplicitOnlyTemplate : IInterfaceForImplicitExplicitTesting
    {
        [Skip]
        public InterfaceForImplicitExplicitTestingImplicitOnlyTemplate() { }

        public string Method1()
        {
            return "Implicit 1";
        }

        public string Method2()
        {
            return "Implicit 2";
        }

        public string Method3()
        {
            return "Implicit 3";
        }
    }
}
