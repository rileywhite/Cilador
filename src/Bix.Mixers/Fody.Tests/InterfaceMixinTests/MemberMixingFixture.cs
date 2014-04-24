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

            targetType.ValidateMemberSources(typeof(FieldsMixin));
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
            targetType.ValidateMemberCountsAre(1, 30, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(MethodsMixin));
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

            targetType.ValidateMemberSources(typeof(PropertiesMixin));
        }

        [Test]
        public void CanMixinEvents()
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
                            Mixin = typeof(EventsMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 28, 12, 0, 14, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(EventsMixin));
        }

        [Test]
        public void CanMixinNestedTypes()
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
                            Mixin = typeof(NestedTypesMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 1, 0, 0, 0, 64);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(NestedTypesMixin));

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject, Is.Not.Null);
            
            var getTypesMethod = targetType.GetMethod("GetTypes", TestContent.BindingFlagsForMixedMembers);
            Assert.That(getTypesMethod, Is.Not.Null);
            
            var typesObject = getTypesMethod.Invoke(instanceObject, new object[0]);
            Assert.That(getTypesMethod, Is.Not.Null);
            
            var types = (Type[])typesObject;
            foreach(var type in types)
            {
                Assert.That(type.FullName.StartsWith(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName));
            }
        }

        [Test]
        public void CanRedirectMixedMembers()
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
                            Mixin = typeof(SelfReferencingMembersMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 27, 10, 2, 2, 8);  // lambda expressions cause methods and fields to be added by the compiler
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            targetType.ValidateMemberSources(typeof(SelfReferencingMembersMixin));
        }
    }
}
