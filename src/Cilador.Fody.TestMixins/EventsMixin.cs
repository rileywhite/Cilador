/***************************************************************************/
// Copyright 2013-2017 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Cilador.Fody.TestMixinInterfaces;
using System;

namespace Cilador.Fody.TestMixins
{
    public class EventsMixin : IEmptyInterface
    {
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
