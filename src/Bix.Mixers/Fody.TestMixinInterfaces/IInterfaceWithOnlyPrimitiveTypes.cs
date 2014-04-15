using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixinInterfaces
{
    public interface IInterfaceWithOnlyPrimitiveTypes
    {
        Boolean BooleanProperty { get; set; }
        Byte ByteProperty { get; set; }
        SByte SByteProperty { get; set; }
        Int16 Int16Property { get; set; }
        UInt16 UInt16Property { get; set; }
        Int32 Int32Property { get; set; }
        UInt32 UInt32Property { get; set; }
        Int64 Int64Property { get; set; }
        UInt64 UInt64Property { get; set; }
        IntPtr IntPtrProperty { get; set; }
        UIntPtr UIntPtrProperty { get; set; }
        Char CharProperty { get; set; }
        Double DoubleProperty { get; set; }
        Single SingleProperty { get; set; }

        Boolean BooleanReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Byte ByteReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        SByte SByteReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Int16 Int16ReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        UInt16 UInt16ReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Int32 Int32ReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        UInt32 UInt32ReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Int64 Int64ReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        UInt64 UInt64ReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        IntPtr IntPtrReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        UIntPtr UIntPtrReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Char CharReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Double DoubleReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
        Single SingleReturnMethod(Boolean arg00, Byte arg01, SByte arg02, Int16 arg03, UInt16 arg04, Int32 arg05, UInt32 arg06, Int64 arg07, UInt64 arg08, IntPtr arg09, UIntPtr arg10, Char arg11, Double arg12, Single arg13);
    }
}
