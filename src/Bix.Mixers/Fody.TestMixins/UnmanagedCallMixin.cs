using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class UnmanagedCallMixin : IEmptyInterface
    {
        [Skip]
        public UnmanagedCallMixin() { }

        [DllImport("msvcrt.dll")]
        public static extern int puts(string c);
    }
}
