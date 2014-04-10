using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixing;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Tests.ConfigTests
{
    [TestFixture]
    internal class ConfigSerializationTests
    {
        [TestCase(
@"<Bix.Mixers xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:Bix:Mixers:Fody:Core"">
</Bix.Mixers>")]
        [TestCase(
@"<Bix.Mixers xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:Bix:Mixers:Fody:Core"" xmlns:bmfim=""urn:Bix:Mixers:Fody:InterfaceMixing"">
  <MixCommandConfig xsi:type=""bmfim:InterfaceMixConfigType"" xmlns="""">
    <InterfaceMap Interface=""Bix.Mixers.Fody.TestInterfaces.IEmptyInterface, Bix.Mixers.Fody.TestInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" Template=""Bix.Mixers.Fody.TestSource.EmptyInterfaceTemplate, Bix.Mixers.Fody.TestSource, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" />
  </MixCommandConfig>
</Bix.Mixers>", typeof(InterfaceMixConfigType))]
        public void CanDeserialize(string xmlString, params Type[] mixCommandConfigTypes)
        {
            var configXElement = XElement.Parse(xmlString);
            Assert.NotNull(configXElement);

            var config = configXElement.FromXElement<BixMixersType>();

            Assert.NotNull(config);

            if (mixCommandConfigTypes == null || mixCommandConfigTypes.Length == 0)
            {
                Assert.That(config.MixCommandConfig == null || config.MixCommandConfig.Length == 0);
            }
            else
            {
                Assert.NotNull(config.MixCommandConfig);

                Assert.That(config.MixCommandConfig.Length == mixCommandConfigTypes.Length);
                for (int i = 0; i < config.MixCommandConfig.Length; i++)
                {
                    Contract.Assume(i < mixCommandConfigTypes.Length);
                    Assert.NotNull(config.MixCommandConfig[i]);
                    Assert.That(config.MixCommandConfig[i].GetType() == mixCommandConfigTypes[i]);
                }
            }
        }
    }
}
