/***************************************************************************/
// Copyright 2013-2015 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Bix.Mixers.Fody.Config;
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
@"<Bix.Mixers><BixMixersConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:Bix:Mixers:Fody:Config"">
</BixMixersConfig></Bix.Mixers>")]
        [TestCase(
@"<Bix.Mixers>
    <bmfc:BixMixersConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:bmfc=""urn:Bix:Mixers:Fody:Config"">
      <MixCommandConfig xsi:type=""bmfc:InterfaceMixinConfigType"">
        <InterfaceMixinMap Interface=""My.Interface.Assembly.Type, My.Interface.Assembly"" Mixin=""My.Mixin.Assembly.Type, My.Mixin.Assembly"" />
      </MixCommandConfig>
    </bmfc:BixMixersConfig>
</Bix.Mixers>", typeof(InterfaceMixinConfigType))]
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
