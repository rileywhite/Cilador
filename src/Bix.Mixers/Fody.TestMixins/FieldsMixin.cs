/***************************************************************************/
// Copyright 2013-2014 Riley White
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
    public class FieldsMixin : IEmptyInterface
    {
        [Skip]
        public FieldsMixin() { }

#pragma warning disable 169, 649

        public int publicField;
        internal int internalField;
        protected int protectedField;
        protected internal int protectedInternalField;
        private int privateField;

        public static int publicStaticField;
        internal static int internalStaticField;
        protected static int protectedStaticField;
        protected internal static int protectedInternalStaticField;
        private static int privateStaticField;

        public object referenceTypeField;
        public DictionaryEntry valueTypeField;
        public TypeCode enumField;
        public EventHandler delegateField;
        public List<int> closedGenericTypeField;

        public readonly int readonlyPrimitiveField;
        public readonly TypeCode readonlyEnumField;
        public readonly string readonlyStringField;
        public readonly object readonlyReferenceTypeField;
        public readonly DictionaryEntry readonlyValueTypeField;

        public static readonly int staticReadonlyPrimitiveField;
        public static readonly TypeCode staticReadonlyEnumField;
        public static readonly string staticReadonlyStringField;
        public static readonly object staticReadonlyReferenceTypeField;
        public static readonly DictionaryEntry staticReadonlyValueTypeField;

        public const int constPrimitiveField = 12433;
        public const TypeCode constEnumField = TypeCode.DateTime;
        public const string constStringField = "Constant value";
        public const object constReferenceTypeField = null;

        [ThreadStatic]
        [NonSerialized]
        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public int fieldWithCustomAttributes;

#pragma warning restore 169, 649
    }
}
