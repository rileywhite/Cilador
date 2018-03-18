/***************************************************************************/
// Copyright 2013-2018 Riley White
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
    public class ArrayFixture
    {
        [Test]
        public void CanMixArraysOfNonMixedType()
        {
            var config = new CiladorConfigType();

            config.WeaveConfig = new WeaveConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(ArraysMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 21, 0, 0, 1);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(ArraysMixin));
        }

        [Test]
        public void CanMixArraysOfMixedType()
        {
            var config = new CiladorConfigType();

            config.WeaveConfig = new WeaveConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(ArraysOfMixedTypeMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 7, 0, 0, 1);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(ArraysOfMixedTypeMixin));
        }
    }
}
