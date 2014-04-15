using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixinTargets
{
    [InterfaceMixin(typeof(IInterfaceWithOnlyPrimitiveTypes))]
    public class InterfaceWithOnlyPrimitiveTypesTarget
    {
    }
}
