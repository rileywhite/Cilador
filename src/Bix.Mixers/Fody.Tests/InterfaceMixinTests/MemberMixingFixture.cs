using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestMixinInterfaces;
using Bix.Mixers.Fody.TestMixins;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    internal class MemberMixingFixture
    {
        [Test]
        public void CanMixinFields()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(FieldsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 30, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            foreach (var targetField in targetType.GetFields(TestContent.BindingFlagsForMixedMembers))
            {
                targetField.ValidateSourceEqual(typeof(FieldsMixin).GetField(targetField.Name, TestContent.BindingFlagsForMixedMembers));
            }
        }

        [Test]
        public void CanMixinMethods()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(MethodsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 23, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            foreach (var targetMethod in targetType.GetMethods(TestContent.BindingFlagsForMixedMembers))
            {
                targetMethod.ValidateSourceEqual(typeof(MethodsMixin).GetMethod(targetMethod.Name, TestContent.BindingFlagsForMixedMembers));
            }
        }

        [Test]
        public void CanMixinProperties()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(PropertiesMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 226, 47, 123, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            foreach (var targetField in targetType.GetFields(TestContent.BindingFlagsForMixedMembers))
            {
                targetField.ValidateSourceEqual(typeof(PropertiesMixin).GetField(targetField.Name, TestContent.BindingFlagsForMixedMembers));
            }

            foreach (var targetMethod in targetType.GetMethods(TestContent.BindingFlagsForMixedMembers))
            {
                targetMethod.ValidateSourceEqual(typeof(PropertiesMixin).GetMethod(
                    targetMethod.Name,
                    TestContent.BindingFlagsForMixedMembers,
                    null,
                    targetMethod.GetParameters().Select(each => each.ParameterType).ToArray(),
                    null));
            }

            foreach (var targetProperty in targetType.GetProperties(TestContent.BindingFlagsForMixedMembers))
            {
                targetProperty.ValidateSourceEqual(typeof(PropertiesMixin).GetProperty(
                    targetProperty.Name,
                    TestContent.BindingFlagsForMixedMembers,
                    null,
                    targetProperty.PropertyType,
                    targetProperty.GetIndexParameters().Select(each => each.ParameterType).ToArray(),
                    null));
            }
        }
    }
}
