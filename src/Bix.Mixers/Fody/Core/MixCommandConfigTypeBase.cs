using Bix.Mixers.Fody.InterfaceMixing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Core
{
    [XmlInclude(typeof(InterfaceMixConfigType))]
    public abstract partial class MixCommandConfigTypeBase
    {
    }
}
