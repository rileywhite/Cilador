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

using Cilador.Fody.Config;
using Cilador.Fody.Core;
using Cilador.Fody.InterfaceMixins;
using Cilador.Fody.Tests.Common;
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

namespace Cilador.Fody.Tests.ModuleWeaverGeneralTests
{
    using Cilador.Fody.Core;
    using Cilador.Fody.Config;

    [TestFixture]
    internal class ConfigTests
    {
        [TestCase(
@"<Cilador><CiladorConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:Cilador:Fody:Config"">
</CiladorConfig></Cilador>")]
        [TestCase(
@"<Cilador>
    <bmfc:CiladorConfig xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:bmfc=""urn:Cilador:Fody:Config"">
      <WeaveConfig xsi:type=""bmfc:InterfaceMixinConfigType"">
        <InterfaceMixinMap Interface=""My.Interface.Assembly.Type, My.Interface.Assembly"" Mixin=""My.Mixin.Assembly.Type, My.Mixin.Assembly"" />
      </WeaveConfig>
    </bmfc:CiladorConfig>
</Cilador>", typeof(InterfaceMixinConfigType))]
        public void CanReadConfig(string xmlString, params Type[] WeaveConfigTypes)
        {
            var configXElement = XElement.Parse(xmlString);
            Assert.NotNull(configXElement);

            var config = ModuleWeaver.ReadCiladorConfig(configXElement);

            Assert.NotNull(config);

            if (WeaveConfigTypes == null || WeaveConfigTypes.Length == 0)
            {
                Assert.That(config.WeaveConfig == null || config.WeaveConfig.Length == 0);
            }
            else
            {
                Assert.NotNull(config.WeaveConfig);

                Assert.That(config.WeaveConfig.Length == WeaveConfigTypes.Length);
                for (int i = 0; i < config.WeaveConfig.Length; i++)
                {
                    Contract.Assume(i < WeaveConfigTypes.Length);
                    Assert.NotNull(config.WeaveConfig[i]);
                    Assert.That(config.WeaveConfig[i].GetType() == WeaveConfigTypes[i]);
                }
            }
        }
    }
}
