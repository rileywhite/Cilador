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
