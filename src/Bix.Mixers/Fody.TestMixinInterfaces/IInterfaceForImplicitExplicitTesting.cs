using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixinInterfaces
{
    public interface IInterfaceForImplicitExplicitTesting
    {
        string Method1();
        string Method2();
        string Method3();

        string Property1 { get; }
        string Property2 { get; }
        string Property3 { get; }

        event EventHandler Event1;
        event EventHandler Event2;
        event EventHandler Event3;
    }
}
