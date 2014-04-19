﻿using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class ImplicitExplicitTestingMixedMixin : IInterfaceForImplicitExplicitTesting
    {
        [Skip]
        public ImplicitExplicitTestingMixedMixin() { }

        public string Method1() { return "Implicit Method 1"; }
        public string Method2() { return "Independent Method 2"; }
        string IInterfaceForImplicitExplicitTesting.Method2() { return "Explicit Method 2"; }
        string IInterfaceForImplicitExplicitTesting.Method3() { return "Explicit Method 3"; }

        public string Property1 { get { return "Implicit Property 1"; } }
        public string Property2 { get { return "Independent Property 2"; } }
        string IInterfaceForImplicitExplicitTesting.Property2 { get { return "Explicit Property 2"; } }
        string IInterfaceForImplicitExplicitTesting.Property3 { get { return "Explicit Property 3"; } }

#pragma warning disable 67
        public event EventHandler Event1;
        public event EventHandler Event2;
        private EventHandler explicitEventHandler2;
        event EventHandler IInterfaceForImplicitExplicitTesting.Event2 { add { this.explicitEventHandler2 += value; } remove { this.explicitEventHandler2 -= value; } }
        private EventHandler explicitEventHandler3;
        event EventHandler IInterfaceForImplicitExplicitTesting.Event3 { add { this.explicitEventHandler3 += value; } remove { this.explicitEventHandler3 -= value; } }
#pragma warning restore 67
    }
}