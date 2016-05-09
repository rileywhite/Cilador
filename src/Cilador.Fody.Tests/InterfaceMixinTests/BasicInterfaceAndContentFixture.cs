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
using System.Linq;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{

    [TestFixture]
    internal class BasicInterfaceAndContentFixture
    {
        [Test]
        public void CanAddInterface()
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
                            Mixin = typeof(EmptyMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instance is IEmptyInterface);
        }

        [Test]
        public void CanAddInterfaceWithContent()
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
                            Mixin = typeof(EmptyInterfaceWithContentMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);

            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));

            targetType.ValidateMemberCountsAre(1, 3, 1, 1, 0, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var property = targetType.GetProperty("SomeValue");
            Assert.That(property != null, "Couldn't find new property SomeValue");

            var method = targetType.GetMethod("SomeMethod");
            Assert.That(method != null, "Couldn't find new method SomeMethod");

            var instance = Activator.CreateInstance(targetType, new object[0]);
            method.Invoke(instance, new object[0]);   // make sure no error is raised
            Assert.That(instance != null, "Failed to create instance with existing default constructor");
            Assert.That(0.Equals(property.GetValue(instance)));
            property.SetValue(instance, 45);
            Assert.That(45.Equals(property.GetValue(instance)));
        }

        [Test]
        public void UnconfiguredInterfaceIsSkipped()
        {
            var config = new CiladorConfigType();

            config.WeaveConfig = new WeaveConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[0]
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType("Cilador.Fody.TestMixinTargets.EmptyInterfaceTarget");
            Assert.That(!typeof(IEmptyInterface).IsAssignableFrom(targetType));
            Assert.That(targetType.GetConstructors(TestContent.BindingFlagsForWeavedMembers).Length == 1, "Expected 1 constructor");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(!(instance is IEmptyInterface));
        }

        [Test]
        public void CanHandleThisReference()
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
                            Mixin = typeof(ThisPameterMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType("Cilador.Fody.TestMixinTargets.EmptyInterfaceTarget");
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            Assert.That(targetType.GetConstructors(TestContent.BindingFlagsForWeavedMembers).Length == 1, "Expected 1 constructor");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instance = Activator.CreateInstance(targetType);
            Assert.That(instance, Is.Not.Null);

            var method = targetType.GetMethod("GetThis", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ReturnType.FullName, Is.EqualTo(targetType.FullName));
            Assert.That(instance, Is.SameAs(method.Invoke(instance, new object[0])));

            method = targetType.GetMethod("GetThisAsInterface", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ReturnType.FullName, Is.EqualTo(typeof(IEmptyInterface).FullName));
            Assert.That(instance, Is.SameAs(method.Invoke(instance, new object[0])));
        }

        [Test]
        public void DoesNotAddReferenceFromMixinTargetToMixinImplementationAssembly()
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
                            Mixin = typeof(EmptyInterfaceWithContentMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var referencedAssemblies = assembly.GetReferencedAssemblies();
            Assert.That(!referencedAssemblies.Any(reference => typeof(EmptyInterfaceWithContentMixin).Assembly.Equals(reference)));
        }
    }
}
