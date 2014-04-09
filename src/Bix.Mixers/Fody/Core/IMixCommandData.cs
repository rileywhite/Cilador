using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Core
{
    public interface IMixCommandData
    {
        string Name { get; }

        Type ConfigType { get; }
    }
}
