using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestTargets
{
    [InterfaceMixin(typeof(IInterfaceForImplicitExplicitTesting))]
    public class InterfaceForImplicitExplicitTestingTarget
    {
    }
}
