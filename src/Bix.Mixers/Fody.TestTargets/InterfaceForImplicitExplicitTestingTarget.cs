using Bix.Mixers.Fody.InterfaceMixing;
using Bix.Mixers.Fody.TestInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestTargets
{
    [InterfaceMix(typeof(IInterfaceForImplicitExplicitTesting))]
    public class InterfaceForImplicitExplicitTestingTarget
    {
    }
}
