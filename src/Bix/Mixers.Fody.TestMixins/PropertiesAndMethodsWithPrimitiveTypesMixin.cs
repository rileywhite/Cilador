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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class PropertiesAndMethodsWithPrimitiveTypesMixin : IInterfaceWithOnlyPrimitiveTypes
    {
        public bool BooleanProperty { get; set; }

        public byte ByteProperty { get; set; }

        public sbyte SByteProperty { get; set; }

        public short Int16Property { get; set; }

        public ushort UInt16Property { get; set; }

        public int Int32Property { get; set; }

        public uint UInt32Property { get; set; }

        public long Int64Property { get; set; }

        public ulong UInt64Property { get; set; }

        public IntPtr IntPtrProperty { get; set; }

        public UIntPtr UIntPtrProperty { get; set; }

        public char CharProperty { get; set; }

        public double DoubleProperty { get; set; }

        public float SingleProperty { get; set; }

        public bool BooleanReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg00;
        }

        public byte ByteReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg01;
        }

        public sbyte SByteReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg02;
        }

        public short Int16ReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg03;
        }

        public ushort UInt16ReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg04;
        }

        public int Int32ReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg05;
        }

        public uint UInt32ReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg06;
        }

        public long Int64ReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg07;
        }

        public ulong UInt64ReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg08;
        }

        public IntPtr IntPtrReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg09;
        }

        public UIntPtr UIntPtrReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg10;
        }

        public char CharReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg11;
        }

        public double DoubleReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg12;
        }

        public float SingleReturnMethod(bool arg00, byte arg01, sbyte arg02, short arg03, ushort arg04, int arg05, uint arg06, long arg07, ulong arg08, IntPtr arg09, UIntPtr arg10, char arg11, double arg12, float arg13)
        {
            return arg13;
        }
    }
}
