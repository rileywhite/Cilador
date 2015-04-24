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
using Cilador.Fody.TestMixinInterfaces;
using Cilador.Fody.TestMixins;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{
    using Cilador.Fody.Config;
    using Cilador.Fody.TestMixinInterfaces;
    using Cilador.Fody.TestMixins;
    using Cilador.Fody.TestMixinTargets;
    using Cilador.Fody.Tests.Common;

    [TestFixture]
    public class DynamicFixture
    {
        [Test]
        public void CanContainDynamicMembersAndLocalVariables()
        {
            var config = new CiladorConfigType();

            config.WeaverConfig = new WeaverConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(DynamicMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 7, 3, 1, 1, 1);   // MS compiler is adding a nested type...not sure it should actually be tested for since it's a compiler implementation detail, but leaving it for now
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(DynamicMixin));
        }
    }
}
