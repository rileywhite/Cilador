using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class ImplicitExplicitTestingImplicitOnlyMixin : IInterfaceForImplicitExplicitTesting
    {
        [Skip]
        public ImplicitExplicitTestingImplicitOnlyMixin() { }

        public string Method1() { return "Implicit Method 1"; }
        public string Method2() { return "Implicit Method 2"; }
        public string Method3() { return "Implicit Method 3"; }

        public string Property1 { get { return "Implicit Property 1"; } }
        public string Property2 { get { return "Implicit Property 2"; } }
        public string Property3 { get { return "Implicit Property 3"; } }

#pragma warning disable 67
        public event EventHandler Event1;
        public event EventHandler Event2;
        public event EventHandler Event3;
#pragma warning restore 67
    }
}
