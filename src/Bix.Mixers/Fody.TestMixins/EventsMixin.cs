using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class EventsMixin : IEmptyInterface
    {
        [Skip]
        public EventsMixin() { }

#pragma warning disable 67

        public event EventHandler PublicEvent;
        internal event EventHandler InternalEvent;
        protected event EventHandler ProtectedEvent;
        protected event EventHandler ProtectedInternalEvent;
        private event EventHandler PrivateEvent;

        public static event EventHandler PublicStaticEvent;
        internal static event EventHandler InternalStaticEvent;
        protected static event EventHandler ProtectedStaticEvent;
        protected internal static event EventHandler ProtectedInternalStaticEvent;
        private static event EventHandler PrivateStaticEvent;

        public event EventHandler<UnhandledExceptionEventArgs> ClosedGenericArgsEvent;

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public event EventHandler EventWithCustomAttributes;

        public event EventHandler ExplicitEvent
        {
            add { }
            remove { }
        }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(2)]
        [TestMixed(3, NamedArgument = 2)]
        [TestMixed(4, NamedTypeArgument = typeof(int))]
        public event EventHandler ExplicitEventWithCustomAttributes
        {
            [TestMixed(5, NamedArgument = 3)]
            [TestMixed(6)]
            [TestMixed(7, NamedArgument = 4)]
            [TestMixed(8, NamedTypeArgument = typeof(int))]
            add { }

            [TestMixed(9, NamedArgument = 5)]
            [TestMixed(10)]
            [TestMixed(11, NamedArgument = 6)]
            [TestMixed(12, NamedTypeArgument = typeof(int))]
            remove { }
        }

#pragma warning restore 67
    }
}
