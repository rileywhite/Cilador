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
    public class UnsupportedFunctionalityFixture
    {
        [Test]
        public void CannotUseAbstractMixin()
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
                            Mixin = typeof(AbstractMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Mixin source type cannot be abstract: [{0}]", typeof(AbstractMixin).FullName));
        }

        [Test]
        public void CannotUseOpenGenericMixinWithoutProvidingTypeArguments()
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
                            Mixin = typeof(OpenGenericMixin<>).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Mixin source type cannot be an open generic type: [{0}]", typeof(AbstractMixin).FullName));
        }

        [Test]
        public void CannotUseInterfaceTypeMixin()
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
                            Mixin = typeof(InterfaceTypeMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Mixin source type must be a reference type: [{0}]", typeof(InterfaceTypeMixin).FullName));
        }

        [Test]
        public void CannotUseValueTypeMixin()
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
                            Mixin = typeof(ValueTypeMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Mixin source type must be a reference type: [{0}]", typeof(ValueTypeMixin).FullName));
        }

        [Test]
        public void MustImplementInterfaceForMixin()
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
                            Mixin = typeof(InterfacelessMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format(
                    "The mixin source [{0}] must implement the interface specified in the interfaceType argument [{1}]",
                    typeof(InterfacelessMixin).FullName,
                    typeof(IEmptyInterface).FullName));
        }

        [Test]
        public void MixinInterfaceMustBeAnInterface()
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
                            Interface = typeof(NotAValidMixinInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(IncorrectMixinSpecifyingClassInsteadOfInterface).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin interface type is not an interface: [{0}]", typeof(NotAValidMixinInterface).FullName));
        }
    }
}
