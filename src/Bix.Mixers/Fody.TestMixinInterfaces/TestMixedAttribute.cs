using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixinInterfaces
{
    [Serializable]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class TestMixedAttribute : Attribute
    {
        public TestMixedAttribute() { }

        public TestMixedAttribute(int constructorArg)
        {
            this.ConstructorArg = constructorArg;
        }

        public int ConstructorArg { get; set; }

        public int NamedArgument { get; set; }
    }
}
