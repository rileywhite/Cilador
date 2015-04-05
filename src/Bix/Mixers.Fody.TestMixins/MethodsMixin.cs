/***************************************************************************/
// Copyright 2013-2015 Riley White
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
    public class MethodsMixin : IEmptyInterface
    {
        public void PublicMethod() { }
        internal void InternalMethod() { }
        protected void ProtectedMethod() { }
        protected internal void ProtectedInternalMethod() { }
        private void PrivateMethod() { }

        public static void PublicStaticMethod() { }
        internal static void InternalStaticMethod() { }
        protected static void ProtectedStaticMethod() { }
        protected internal static void ProtectedInternalStaticMethod() { }
        private static void PrivateStaticMethod() { }

        public void VoidReturnTypeMethod() { }
        public object ReferenceReturnTypeMethod() { return null; }
        public DictionaryEntry ValueReturnTypeMethod() { return default(DictionaryEntry); }
        public TypeCode EnumReturnTypeMethod() { return default(TypeCode); }
        public EventHandler DelegateReturnTypeMethod() { return null; }
        public List<int> ClosedGenericReturnTypeMethod() { return null; }

        public void MethodWithInParameters(int arg0, DictionaryEntry arg1, TypeCode arg2, string arg3, object arg4, List<int> arg5) { }
        public void MethodWithRefParameters(ref int arg0, ref DictionaryEntry arg1, ref TypeCode arg2, ref string arg3, ref object arg4, ref List<int> arg5) { }
        public void MethodWithOutParameters(out int arg0, out DictionaryEntry arg1, out TypeCode arg2, out string arg3, out object arg4, out List<int> arg5)
        {
            arg0 = default(int);
            arg1 = default(DictionaryEntry);
            arg2 = default(TypeCode);
            arg3 = default(string);
            arg4 = default(object);
            arg5 = default(List<int>);
        }

        public void MethodWithOptionalParameters(
            int arg0 = 4,
            DictionaryEntry arg1 = default(DictionaryEntry),
            TypeCode arg2 = default(TypeCode),
            string arg3 = "ksjlkjsd",
            object arg4 = null,
            List<int> arg5 = null) { }

        public void MethodWithMixedParameters(
            int arg0,
            DictionaryEntry arg1,
            TypeCode arg2,
            ref string arg3,
            out object arg4,
            List<int> arg5 = null) { arg4 = default(object); }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        public void MethodWithCustomAttributes() { }

        [TestMixed(1, NamedArgument = 1)]
        [TestMixed(0)]
        [TestMixed(2, NamedArgument = 2)]
        [TestMixed(0, NamedTypeArgument = typeof(int))]
        public void MethodWithParametersWithCustomAttributes(
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
            List<int> arg5) { }

        public void OverloadedMethod() { }
        public void OverloadedMethod(int arg0) { }
        public void ReferenceReturnTypeMethod(object arg0) { }
        public void ValueReturnTypeMethod(DictionaryEntry arg0) { }
        public void EnumReturnTypeMethod(TypeCode arg0) { }
        public void DelegateReturnTypeMethod(EventHandler arg0) { }
        public void ClosedGenericReturnTypeMethod(List<int> arg0) { }

        // TODO virtual/abstract
    }
}
