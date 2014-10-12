using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;

namespace Bix.Mixers.Fody.TestMixinTargets
{
    public class MultipleConstructorsTargetBase
    {
        public MultipleConstructorsTargetBase()
        {
            this.WhichConstructor = "First";
        }

        public MultipleConstructorsTargetBase(int i)
        {
            this.WhichConstructor = "Second";
        }

        public string WhichConstructor;
    }

    [InterfaceMixin(typeof(IForTargetWithMultipleConstructors))]
    public class MultipleConstructorsTarget : MultipleConstructorsTargetBase
    {
        public MultipleConstructorsTarget()
        {
            this.OriginalUninitializedInt = 783535;
            this.OriginalUninitializedString = "KNion wineofn oianweiof nqiognui ndf";
            this.OriginalUninitializedObject = new object();
        }

        public MultipleConstructorsTarget(int i) : this(i, "A iuohiogfniouhe uihui iu.", new object()) { }

        public MultipleConstructorsTarget(int i, string j) : this(i, j, new object()) { }

        public MultipleConstructorsTarget(int i, string j, object k)
            : base(i)
        {
            this.OriginalUninitializedInt = i;
            this.OriginalUninitializedString = j;
            this.OriginalUninitializedObject = k;
        }

        public int OriginalInitializedInt = 48685;
        public string OriginalInitializedString = "Tion3lao ehiuawh iuh buib ld";
        public object OriginalInitializedObject = new object();

        public int OriginalUninitializedInt;
        public string OriginalUninitializedString;
        public object OriginalUninitializedObject;
    }
}
