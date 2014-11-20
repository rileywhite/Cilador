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

using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class NestedTypesMixin : IEmptyInterface
    {
        public class PublicType { }
        internal class InternalType { }
        protected class ProtectedType { }
        protected internal class ProtectedInternalType { }
        private class PrivateType { }

        public class ReferenceType { }
        public sealed class ReferenceSealedType { }
        public abstract class ReferenceAbstractType { public abstract void Method(); public abstract int Property { get; set; } public abstract event EventHandler Event; }
        public class ReferenceTypeWithBase : ArrayList { }
        public class ReferenceTypeWithClosedGenericBase : List<int> { }
        public class ReferenceTypeWithInterface : IDisposable { public void Dispose() { } }
        public class ReferenceTypeWithClosedGenericInterface : IComparable<int> { public int CompareTo(int other) { return 0; } }
        public class ReferenceTypeWithBaseAndInterface : ArrayList, IDisposable { public void Dispose() { } }
        public class ReferenceTypeWithBaseAndClosedGenericInterface : IComparable<int> { public int CompareTo(int other) { return 0; } }
        public class ReferenceTypeWithClosedGenericBaseAndInterface : List<int>, IDisposable { public void Dispose() { } }
        public class ReferenceTypeWithClosedGenericBaseAndClosedGenericInterface : List<int>, IComparable<int> { public int CompareTo(int other) { return 0; } }
        public class ReferenceTypeWithInterfaceWithBaseImplementation : IEnumerable<int> { public IEnumerator<int> GetEnumerator() { return null; } IEnumerator IEnumerable.GetEnumerator() { return null; } }
        public class ReferenceTypeWithInnerTypes { public class InnerType { public class InnerInnerType { } public struct InnerInnerStruct { } public interface InnerInnerInterface { } } public struct InnerStruct { } public interface InnerInterface { } public enum InnerEnum { } public delegate void InnerDelegate(); }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public class ReferenceTypeWithCustomAttributes { }

        public struct ValueType { }
        public struct ValueTypeWithInterface : IDisposable { public void Dispose() { } }
        public struct ValueTypeWithClosedGenericInterface : IComparable<int> { public int CompareTo(int other) { return 0; } }
        public struct ValueTypeWithInterfaceWithBaseImplementation : IEnumerable<int> { public IEnumerator<int> GetEnumerator() { return null; } IEnumerator IEnumerable.GetEnumerator() { return null; } }
        public struct ValueTypeWithInnerTypes { public class InnerType { public class InnerInnerType { } public struct InnerInnerStruct { } public interface InnerInnerInterface { } } public struct InnerStruct { } public interface InnerInterface { } public enum InnerEnum { } public delegate void InnerDelegate(); }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public struct ValueTypeWithCustomAttributes { }

        public interface IntefaceType { }
        public interface interfaceTypeWithBase : IDisposable { }
        public interface interfaceTypeWithMultipleBases : IDisposable, IEnumerable<int> { }
        public interface InterfaceTypeWithClosedGenericBase : IComparable<int> { }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public interface InterfaceTypeWithCustomAttributes { }

        public enum EnumTypeEmpty { }
        public enum EnumTypeWithValues { One, Two, Three }
        public enum EnumTypeWithExplicitValues { One = 8938, Two = 922, Three = 5994 }
        [Flags]
        public enum FlagsEnumTypes { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypes.Two | FlagsEnumTypes.Five }
        public enum EnumTypeByte : byte { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeBytes : byte { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeBytes.Two | FlagsEnumTypeBytes.Five }
        public enum EnumTypeSByte : sbyte { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeSBytes : sbyte { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeSBytes.Two | FlagsEnumTypeSBytes.Five }
        public enum EnumTypeShort : short { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeShorts : short { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeShorts.Two | FlagsEnumTypeShorts.Five }
        public enum EnumTypeUShort : ushort { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeUShorts : ushort { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeUShorts.Two | FlagsEnumTypeUShorts.Five }
        public enum EnumTypeInt : int { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeInts : int { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeInts.Two | FlagsEnumTypeInts.Five }
        public enum EnumTypeUInt : uint { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeUInts : uint { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeUInts.Two | FlagsEnumTypeUInts.Five }
        public enum EnumTypeLong : long { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeLongs : long { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeLongs.Two | FlagsEnumTypeLongs.Five }
        public enum EnumTypeULong : ulong { One, Two, Three }
        [Flags]
        public enum FlagsEnumTypeULongs : ulong { Zero = 0, One = 1, Two = 2, Three = 4, Four = 8, Five = 16, Six = FlagsEnumTypeULongs.Two | FlagsEnumTypeULongs.Five }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(2)]
        [TestMixed(3, NamedArgument = 2)]
        [TestMixed(4, NamedTypeArgument = typeof(int))]
        public enum EnumTypeWithCustomAttributes
        {
            [TestMixed(5, NamedArgument = 3)]
            [TestMixed(6)]
            [TestMixed(7, NamedArgument = 4)]
            [TestMixed(8, NamedTypeArgument = typeof(long))]
            One,

            [TestMixed(9, NamedArgument = 5)]
            [TestMixed(10)]
            [TestMixed(11, NamedArgument = 6)]
            [TestMixed(12, NamedTypeArgument = typeof(DictionaryEntry))]
            Two,

            [TestMixed(13, NamedArgument = 7)]
            [TestMixed(14)]
            [TestMixed(15, NamedArgument = 8)]
            [TestMixed(16, NamedTypeArgument = typeof(TypeCode))]
            Three
        }

        public delegate void VoidReturnTypeDelegate();
        public delegate object ReferenceReturnTypeDelegate();
        public delegate DictionaryEntry ValueReturnTypeDelegate();
        public delegate TypeCode EnumReturnTypeDelegate();
        public delegate EventHandler DelegateReturnTypeDelegate();
        public delegate List<int> ClosedGenericReturnTypeDelegate();

        public delegate void DelegateWithInParameters(int arg0, DictionaryEntry arg1, TypeCode arg2, string arg3, object arg4, List<int> arg5);
        public delegate void DelegateWithRefParameters(ref int arg0, ref DictionaryEntry arg1, ref TypeCode arg2, ref string arg3, ref object arg4, ref List<int> arg5);
        public delegate void DelegateWithOutParameters(out int arg0, out DictionaryEntry arg1, out TypeCode arg2, out string arg3, out object arg4, out List<int> arg5);
        public delegate void DelegateWithOptionalParameters(
            int arg0 = 4,
            DictionaryEntry arg1 = default(DictionaryEntry),
            TypeCode arg2 = default(TypeCode),
            string arg3 = "ksjlkjsd",
            object arg4 = null,
            List<int> arg5 = null);

        public delegate void DelegateWithMixedParameters(
            int arg0,
            DictionaryEntry arg1,
            TypeCode arg2,
            ref string arg3,
            out object arg4,
            List<int> arg5 = null);

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        public delegate void DelegateWithCustomAttributes();

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public delegate void DelegateWithParametersWithCustomAttributes(
            [TestMixed(1, NamedArgument = 1)]
            [TestMixed(2)]
            [TestMixed(3, NamedArgument = 2)]
            [TestMixed(0, NamedTypeArgument = typeof(long))]
            int arg0,

            [TestMixed(4, NamedArgument = 3)]
            [TestMixed(5)]
            [TestMixed(6, NamedArgument = 4)]
            [TestMixed(0, NamedTypeArgument = typeof(short))]
            DictionaryEntry arg1,

            [TestMixed(7, NamedArgument = 5)]
            [TestMixed(8)]
            [TestMixed(9, NamedArgument = 6)]
            [TestMixed(0, NamedTypeArgument = typeof(object))]
            TypeCode arg2,

            [TestMixed(10, NamedArgument = 7)]
            [TestMixed(11)]
            [TestMixed(12, NamedArgument = 8)]
            [TestMixed(0, NamedTypeArgument = typeof(List<int>))]
            string arg3,

            [TestMixed(13, NamedArgument = 9)]
            [TestMixed(14)]
            [TestMixed(15, NamedArgument = 10)]
            [TestMixed(0, NamedTypeArgument = typeof(DictionaryEntry))]
            object arg4,

            [TestMixed(16, NamedArgument = 11)]
            [TestMixed(17)]
            [TestMixed(18, NamedArgument = 12)]
            [TestMixed(0, NamedTypeArgument = typeof(TypeCode))]
            List<int> arg5);

        public Type[] GetTypes()
        {
            return new Type[]
            {
                typeof(NestedTypesMixin),
                typeof(PublicType),
                typeof(ReferenceType),
                typeof(ValueType),
                typeof(EnumTypeInt),
                typeof(VoidReturnTypeDelegate),
                typeof(ReferenceTypeWithInnerTypes.InnerType.InnerInnerType)
            };
        }
    }
}
