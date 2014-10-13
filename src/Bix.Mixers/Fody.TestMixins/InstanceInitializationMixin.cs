using Bix.Mixers.Fody.TestMixinInterfaces;
using System;

namespace Bix.Mixers.Fody.TestMixins
{
    public class InstanceInitializationMixin : IForTargetWithMultipleConstructors
    {
        public int SomeNumber = 684865;
        public string SomeString = "Tawhlej oisahoeh 8ohf 4ifh8ohe fni dlgj";
        public object SomeObject = new object();
        public InnerType SomeInnerType = new InnerType { SomeInt = 4235, SomeString = "JLKOIN  oin aon oingori d", SomeObject = new object() };
        public Func<int, string, object, Tuple<int, string, object>> SomeFunc = InnerType.SomeMethod;
        public SomeMethodDelegate SomeMethodDelegateInstance = InnerType.SomeMethod;

        public delegate Tuple<int, string, object> SomeMethodDelegate(int i, string j, object k);
        
        public class InnerType
        {
            public static Tuple<int, string, object> SomeMethod(int i, string j, object k)
            {
                return Tuple.Create(i, j, k);
            }

            public int SomeInt { get; set; }
            public string SomeString { get; set; }
            public object SomeObject { get; set; }
        }
    }
}
