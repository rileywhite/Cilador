/***************************************************************************/
// Copyright 2013-2017 Riley White
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
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    internal class MemberMixingFixture
    {
        [Test]
        public void CanMixinFields()
        {
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (FieldsMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 30, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(FieldsMixin));

            MemberMixingFixture.ValidateNonTargetTypeAndAttributesAreUntouched(assembly);
        }

        private static void ValidateNonTargetTypeAndAttributesAreUntouched(Assembly mixedAssembly)
        {
            Contract.Requires(mixedAssembly != null);

            var nonTargetType = mixedAssembly.GetType(typeof(NonTargetType).FullName);
            nonTargetType.ValidateMemberCountsAre(3, 15, 3, 3, 3, 15);

            System.ComponentModel.DescriptionAttribute descriptionAttribute;

            foreach (var field in nonTargetType.GetFields(TestContent.BindingFlagsForWeavedMembers))
            {
                descriptionAttribute = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == field.Name);
            }

            foreach (var constructor in nonTargetType.GetConstructors(TestContent.BindingFlagsForWeavedMembers))
            {
                descriptionAttribute = constructor.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == constructor.Name);

                foreach (var parameter in constructor.GetParameters())
                {
                    descriptionAttribute = parameter.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    Assert.That(descriptionAttribute != null);
                    Assert.That(descriptionAttribute.Description == parameter.Name);
                }
            }

            foreach (var method in nonTargetType.GetMethods(TestContent.BindingFlagsForWeavedMembers))
            {
                descriptionAttribute = method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == method.Name);

                descriptionAttribute = method.ReturnParameter.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == "return");

                foreach (var parameter in method.GetParameters())
                {
                    descriptionAttribute = parameter.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    Assert.That(descriptionAttribute != null);
                    Assert.That(descriptionAttribute.Description == parameter.Name);
                }
            }

            foreach (var property in nonTargetType.GetProperties(TestContent.BindingFlagsForWeavedMembers))
            {
                descriptionAttribute = property.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == property.Name);
            }

            foreach (var @event in nonTargetType.GetEvents(TestContent.BindingFlagsForWeavedMembers))
            {
                descriptionAttribute = @event.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == @event.Name);
            }

            foreach (var nestedType in nonTargetType.GetNestedTypes(TestContent.BindingFlagsForWeavedMembers))
            {
                descriptionAttribute = nestedType.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                Assert.That(descriptionAttribute != null);
                Assert.That(descriptionAttribute.Description == nestedType.Name);
            }
        }

        [Test]
        public void CanMixinMethods()
        {
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (MethodsMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 30, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(MethodsMixin));
        }

        [Test]
        public void CanMixinProperties()
        {
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (PropertiesMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 226, 47, 123, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(PropertiesMixin));
        }

        [Test]
        public void CanMixinEvents()
        {
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (EventsMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 28, 12, 0, 14, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(EventsMixin));
        }

        [Test]
        public void CanMixinNestedTypes()
        {
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (NestedTypesMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
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
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (SelfReferencingMembersMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 23, 6, 2, 2, 9);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(SelfReferencingMembersMixin));
        }
    }
}
