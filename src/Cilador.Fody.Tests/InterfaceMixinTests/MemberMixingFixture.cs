﻿/***************************************************************************/
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
    internal class MemberMixingFixture
    {
        [Test]
        public void CanMixinFields()
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
                            Mixin = typeof(FieldsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 30, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(FieldsMixin));
        }

        [Test]
        public void CanMixinMethods()
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
                            Mixin = typeof(MethodsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 30, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(MethodsMixin));
        }

        [Test]
        public void CanMixinProperties()
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
                            Mixin = typeof(PropertiesMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 226, 47, 123, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(PropertiesMixin));
        }

        [Test]
        public void CanMixinEvents()
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
                            Mixin = typeof(EventsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 28, 12, 0, 14, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(EventsMixin));
        }

        [Test]
        public void CanMixinNestedTypes()
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
                            Mixin = typeof(NestedTypesMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 1, 0, 0, 0, 64);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(NestedTypesMixin));

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject, Is.Not.Null);
            
            var getTypesMethod = targetType.GetMethod("GetTypes", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(getTypesMethod, Is.Not.Null);
            
            var typesObject = getTypesMethod.Invoke(instanceObject, new object[0]);
            Assert.That(getTypesMethod, Is.Not.Null);
            
            var types = (Type[])typesObject;
            foreach(var type in types)
            {
                Assert.That(type.FullName.StartsWith(typeof(EmptyInterfaceTarget).FullName));
            }
        }

        [Test]
        public void CanRedirectMixedMembers()
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
                            Mixin = typeof(SelfReferencingMembersMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 27, 10, 2, 2, 8);  // lambda expressions cause methods and fields to be added by the compiler
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(SelfReferencingMembersMixin));
        }
    }
}