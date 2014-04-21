using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public interface AnotherEmptyInterface { }

    public class ExtraInterfaceMixin : IEmptyInterface, AnotherEmptyInterface
    {
        [Skip]
        public ExtraInterfaceMixin() { }
    }
}
