/***************************************************************************/
// Copyright 2013-2016 Riley White
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
using Cilador.Fody.TestMixinInterfaces;
using Cilador.Fody.TestMixins;
using Cilador.Fody.TestMixinTargets;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{

    [TestFixture]
    internal class ConstructorsFixture
    {
        [Test]
        public void NestedTypeConstructorsAreCloned()
        {
            var config = new CiladorConfigType();

            config.WeaveConfig = new WeaveConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new []
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(NestedTypeConstructorsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 0, 0, 0, 1);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var nestedType = targetType.GetNestedType("InnerClass", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(nestedType != null);

            var staticProperty = nestedType.GetProperty("StaticSomeValue", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(staticProperty != null);
            Assert.That(488.Equals(staticProperty.GetValue(null)));

            var property = nestedType.GetProperty("SomeValue", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(property != null);
            var instance = Activator.CreateInstance(nestedType, new object[0]);
            Assert.That(42.Equals(property.GetValue(instance)));
            instance = Activator.CreateInstance(nestedType, new object[] { 5 });
            Assert.That(5.Equals(property.GetValue(instance)));
        }
    }
}
