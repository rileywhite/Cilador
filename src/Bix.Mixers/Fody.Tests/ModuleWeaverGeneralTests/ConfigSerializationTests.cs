using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixins;
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

namespace Bix.Mixers.Fody.Tests.ModuleWeaverGeneralTests
{
    [TestFixture]
    internal class ConfigTests
    {
        [TestCase(
@"<Bix.Mixers><BixMixersConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:Bix:Mixers:Fody:Core"">
</BixMixersConfig></Bix.Mixers>")]
        [TestCase(
@"<Bix.Mixers><BixMixersConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:Bix:Mixers:Fody:Core"" xmlns:bmfim=""urn:Bix:Mixers:Fody:InterfaceMixins"">
  <MixCommandConfig xsi:type=""bmfim:InterfaceMixinConfigType"" xmlns="""">
    <InterfaceMap Interface=""My.Interface.Assembly.Type, My.Interface.Assembly"" Template=""My.Template.Assembly.Type, My.Template.Assembly"" />
  </MixCommandConfig>
</BixMixersConfig></Bix.Mixers>", typeof(InterfaceMixinConfigType))]
        public void CanReadConfig(string xmlString, params Type[] mixCommandConfigTypes)
        {
            var configXElement = XElement.Parse(xmlString);
            Assert.NotNull(configXElement);

            var config = ModuleWeaver.ReadBixMixersConfig(configXElement);

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
