using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class ImplicitExplicitTestingExplicitOnlyMixin : IInterfaceForImplicitExplicitTesting
    {
        [Skip]
        public ImplicitExplicitTestingExplicitOnlyMixin() { }

        string IInterfaceForImplicitExplicitTesting.Method1() { return "Explicit Method 1"; }
        string IInterfaceForImplicitExplicitTesting.Method2() { return "Explicit Method 2"; }
        string IInterfaceForImplicitExplicitTesting.Method3() { return "Explicit Method 3"; }


        string IInterfaceForImplicitExplicitTesting.Property1 { get { return "Explicit Property 1"; } }
        string IInterfaceForImplicitExplicitTesting.Property2 { get { return "Explicit Property 2"; } }
        string IInterfaceForImplicitExplicitTesting.Property3 { get { return "Explicit Property 3"; } }

        private EventHandler explicitEventHandler1;
        event EventHandler IInterfaceForImplicitExplicitTesting.Event1 { add { this.explicitEventHandler1 += value; } remove { this.explicitEventHandler1 -= value; } }
        private EventHandler explicitEventHandler2;
        event EventHandler IInterfaceForImplicitExplicitTesting.Event2 { add { this.explicitEventHandler2 += value; } remove { this.explicitEventHandler2 -= value; } }
        private EventHandler explicitEventHandler3;
        event EventHandler IInterfaceForImplicitExplicitTesting.Event3 { add { this.explicitEventHandler3 += value; } remove { this.explicitEventHandler3 -= value; } }
    }
}
