using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.InterfaceMixins
{
    public partial class InterfaceMixinConfigType : MixCommandConfigTypeBase
    {
    }
}

namespace Bix.Mixers.Fody.Core
{
    [XmlInclude(typeof(InterfaceMixinConfigType))]
    public abstract partial class MixCommandConfigTypeBase
    {
    }
}
