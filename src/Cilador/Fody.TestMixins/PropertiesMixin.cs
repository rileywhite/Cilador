/***************************************************************************/
// Copyright 2013-2019 Riley White
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
using System.Collections;
using System.Collections.Generic;

namespace Cilador.Fody.TestMixins
{
    public class PropertiesMixin : IEmptyInterface
    {
        public int AutoPublicGetPublicSet { get; set; }
        public int AutoPublicGetPrivateSet { get; private set; }
        public int AutoPublicGetInternalSet { get; internal set; }
        public int AutoPublicGetProtectedSet { get; protected set; }
        public int AutoPublicGetProtectedInternalSet { get; protected internal set; }

        public int AutoPrivateGetPublicSet { private get; set; }
        private int AutoPrivateGetPrivateSet { get; set; }
        internal int AutoPrivateGetInternalSet { private get; set; }
        protected int AutoPrivateGetProtectedSet { private get; set; }
        protected internal int AutoPrivateGetProtectedInternalSet { private get; set; }

        public int AutoInternalGetPublicSet { internal get; set; }
        internal int AutoInternalGetPrivateSet { get; private set; }
        internal int AutoInternalGetInternalSet { get; set; }
        protected internal int AutoInternalGetProtectedInternalSet { internal get; set; }

        public int AutoProtectedGetPublicSet { protected get; set; }
        protected int AutoProtectedGetPrivateSet { get; private set; }
        protected int AutoProtectedGetProtectedSet { get; set; }
        protected internal int AutoProtectedGetProtectedInternalSet { protected get; set; }

        public int AutoProtectedInternalGetPublicSet { protected internal get; set; }
        protected internal int AutoProtectedInternalGetPrivateSet { get; private set; }
        protected internal int AutoProtectedInternalGetInternalSet { get; internal set; }
        protected internal int AutoProtectedInternalGetProtectedSet { get; protected set; }
        protected internal int AutoProtectedInternalGetProtectedInternalSet { get; set; }

        public int PublicGetPublicSet { get { return 0; } set { } }
        public int PublicGetPrivateSet { get { return 0; } private set { } }
        public int PublicGetInternalSet { get { return 0; } internal set { } }
        public int PublicGetProtectedSet { get { return 0; } protected set { } }
        public int PublicGetProtectedInternalSet { get { return 0; } protected internal set { } }

        public int PrivateGetPublicSet { private get { return 0; } set { } }
        private int PrivateGetPrivateSet { get { return 0; } set { } }
        internal int PrivateGetInternalSet { private get { return 0; } set { } }
        protected int PrivateGetProtectedSet { private get { return 0; } set { } }
        protected internal int PrivateGetProtectedInternalSet { private get { return 0; } set { } }

        public int InternalGetPublicSet { internal get { return 0; } set { } }
        internal int InternalGetPrivateSet { get { return 0; } private set { } }
        internal int InternalGetInternalSet { get { return 0; } set { } }
        protected internal int InternalGetProtectedInternalSet { internal get { return 0; } set { } }

        public int ProtectedGetPublicSet { protected get { return 0; } set { } }
        protected int ProtectedGetPrivateSet { get { return 0; } private set { } }
        protected int ProtectedGetProtectedSet { get { return 0; } set { } }
        protected internal int ProtectedGetProtectedInternalSet { protected get { return 0; } set { } }

        public int ProtectedInternalGetPublicSet { protected internal get { return 0; } set { } }
        protected internal int ProtectedInternalGetPrivateSet { get { return 0; } private set { } }
        protected internal int ProtectedInternalGetInternalSet { get { return 0; } internal set { } }
        protected internal int ProtectedInternalGetProtectedSet { get { return 0; } protected set { } }
        protected internal int ProtectedInternalGetProtectedInternalSet { get { return 0; } set { } }

        public int PublicGet { get { return 0; } }
        private int PrivateGet { get { return 0; } }
        internal int InternalGet { get { return 0; } }
        protected int ProtectedGet { get { return 0; } }
        protected internal int ProtectedInternalGet { get { return 0; } }
        public int PublicSet { set { } }
        private int PrivateSet { set { } }
        internal int InternalSet { set { } }
        protected int ProtectedSet { set { } }
        protected internal int ProtectedInternalSet { set { } }

        public static int StaticAutoPublicGetPublicSet { get; set; }
        public static int StaticAutoPublicGetPrivateSet { get; private set; }
        public static int StaticAutoPublicGetInternalSet { get; internal set; }
        public static int StaticAutoPublicGetProtectedSet { get; protected set; }
        public static int StaticAutoPublicGetProtectedInternalSet { get; protected internal set; }

        public static int StaticAutoPrivateGetPublicSet { private get; set; }
        private static int StaticAutoPrivateGetPrivateSet { get; set; }
        internal static int StaticAutoPrivateGetInternalSet { private get; set; }
        protected static int StaticAutoPrivateGetProtectedSet { private get; set; }
        protected internal static int StaticAutoPrivateGetProtectedInternalSet { private get; set; }

        public static int StaticAutoInternalGetPublicSet { internal get; set; }
        internal static int StaticAutoInternalGetPrivateSet { get; private set; }
        internal static int StaticAutoInternalGetInternalSet { get; set; }
        protected internal static int StaticAutoInternalGetProtectedInternalSet { internal get; set; }

        public static int StaticAutoProtectedGetPublicSet { protected get; set; }
        protected static int StaticAutoProtectedGetPrivateSet { get; private set; }
        protected static int StaticAutoProtectedGetProtectedSet { get; set; }
        protected internal static int StaticAutoProtectedGetProtectedInternalSet { protected get; set; }

        public static int StaticAutoProtectedInternalGetPublicSet { protected internal get; set; }
        protected internal static int StaticAutoProtectedInternalGetPrivateSet { get; private set; }
        protected internal static int StaticAutoProtectedInternalGetInternalSet { get; internal set; }
        protected internal static int StaticAutoProtectedInternalGetProtectedSet { get; protected set; }
        protected internal static int StaticAutoProtectedInternalGetProtectedInternalSet { get; set; }

        public static int StaticPublicGetPublicSet { get { return 0; } set { } }
        public static int StaticPublicGetPrivateSet { get { return 0; } private set { } }
        public static int StaticPublicGetInternalSet { get { return 0; } internal set { } }
        public static int StaticPublicGetProtectedSet { get { return 0; } protected set { } }
        public static int StaticPublicGetProtectedInternalSet { get { return 0; } protected internal set { } }

        public static int StaticPrivateGetPublicSet { private get { return 0; } set { } }
        private static int StaticPrivateGetPrivateSet { get { return 0; } set { } }
        internal static int StaticPrivateGetInternalSet { private get { return 0; } set { } }
        protected static int StaticPrivateGetProtectedSet { private get { return 0; } set { } }
        protected internal static int StaticPrivateGetProtectedInternalSet { private get { return 0; } set { } }

        public static int StaticInternalGetPublicSet { internal get { return 0; } set { } }
        internal static int StaticInternalGetPrivateSet { get { return 0; } private set { } }
        internal static int StaticInternalGetInternalSet { get { return 0; } set { } }
        protected internal static int StaticInternalGetProtectedInternalSet { internal get { return 0; } set { } }

        public static int StaticProtectedGetPublicSet { protected get { return 0; } set { } }
        protected static int StaticProtectedGetPrivateSet { get { return 0; } private set { } }
        protected static int StaticProtectedGetProtectedSet { get { return 0; } set { } }
        protected internal static int StaticProtectedGetProtectedInternalSet { protected get { return 0; } set { } }

        public static int StaticProtectedInternalGetPublicSet { protected internal get { return 0; } set { } }
        protected internal static int StaticProtectedInternalGetPrivateSet { get { return 0; } private set { } }
        protected internal static int StaticProtectedInternalGetInternalSet { get { return 0; } internal set { } }
        protected internal static int StaticProtectedInternalGetProtectedSet { get { return 0; } protected set { } }
        protected internal static int StaticProtectedInternalGetProtectedInternalSet { get { return 0; } set { } }

        public static int StaticPublicGet { get { return 0; } }
        private static int StaticPrivateGet { get { return 0; } }
        internal static int StaticInternalGet { get { return 0; } }
        protected static int StaticProtectedGet { get { return 0; } }
        protected internal static int StaticProtectedInternalGet { get { return 0; } }
        public static int StaticPublicSet { set { } }
        private static int StaticPrivateSet { set { } }
        internal static int StaticInternalSet { set { } }
        protected static int StaticProtectedSet { set { } }
        protected internal static int StaticProtectedInternalSet { set { } }

        public object ReferenceTypeProperty { get { return null; } set { } }
        public DictionaryEntry ValueTypeProperty { get { return default(DictionaryEntry); } set { } }
        public StringSplitOptions EnumProperty { get { return default(StringSplitOptions); } set { } }
        public EventHandler DelegateProperty { get { return null; } set { } }
        public List<int> ClosedGenericTypeProperty { get { return null; } set { } }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(2)]
        [TestMixed(3, NamedArgument = 2)]
        [TestMixed(4, NamedTypeArgument = typeof(int))]
        public int AutoPropertyWithCustomAttributes
        {
            [TestMixed(5, NamedArgument = 3)]
            [TestMixed(6)]
            [TestMixed(7, NamedArgument = 4)]
            [TestMixed(8, NamedTypeArgument = typeof(object))]
            get;

            [TestMixed(9, NamedArgument = 5)]
            [TestMixed(10)]
            [TestMixed(11, NamedArgument = 6)]
            [TestMixed(12, NamedTypeArgument = typeof(TypeCode))]
            set;
        }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(2)]
        [TestMixed(3, NamedArgument = 2)]
        [TestMixed(4, NamedTypeArgument = typeof(int))]
        public int PropertyWithGetSetCustomAttributes
        {
            [TestMixed(5, NamedArgument = 3)]
            [TestMixed(6)]
            [TestMixed(7, NamedArgument = 4)]
            [TestMixed(8, NamedTypeArgument = typeof(object))]
            get { return default(int); }

            [TestMixed(9, NamedArgument = 5)]
            [TestMixed(10)]
            [TestMixed(11, NamedArgument = 6)]
            [TestMixed(12, NamedTypeArgument = typeof(TypeCode))]
            set { }
        }

        public int this[int index] { get { return index; } set { } }
        public int this[string name] { get { return 0; } set { } }
        public int this[int @int, long @long, string @string, DictionaryEntry dictionaryEntry, object @object] { get { return 0; } set { } }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(2)]
        [TestMixed(3, NamedArgument = 2)]
        [TestMixed(4, NamedTypeArgument = typeof(int))]
        public int this[
            [TestMixed(5, NamedArgument = 3)]
            [TestMixed(6)]
            [TestMixed(7, NamedArgument = 4)]
            [TestMixed(8, NamedTypeArgument = typeof(int))]
            int @int,

            [TestMixed(9, NamedArgument = 5)]
            [TestMixed(10)]
            [TestMixed(11, NamedArgument = 6)]
            [TestMixed(12, NamedTypeArgument = typeof(int))]
            object @object]
        {
            [TestMixed(13, NamedArgument = 7)]
            [TestMixed(14)]
            [TestMixed(15, NamedArgument = 8)]
            [TestMixed(16, NamedTypeArgument = typeof(int))]
            get { return 0; }

            [TestMixed(17, NamedArgument = 9)]
            [TestMixed(18)]
            [TestMixed(19, NamedArgument = 10)]
            [TestMixed(20, NamedTypeArgument = typeof(int))]
            set { }
        }
    }
}
