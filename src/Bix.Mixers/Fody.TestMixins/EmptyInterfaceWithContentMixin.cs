using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class EmptyInterfaceWithContentMixin : IEmptyInterface
    {
        public EmptyInterfaceWithContentMixin(int someValue)
        {
            this.SomeValue = someValue;
        }

        public int SomeValue { get; set; }

        public void SomeMethod()
        {
            int a = 5;
            a += 1;
        }
    }
}
