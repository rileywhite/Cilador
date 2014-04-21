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

        [Test]
        public void MixinImplementationCannotHaveTypeInitializer()
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
                            Mixin = typeof(TypeInitializerMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin implementation cannot have a type initializer (i.e. static constructor): [{0}]", typeof(TypeInitializerMixin).FullName));
        }

        [Test]
        public void MixinImplementationMustInheritDirectlyFromObject()
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
                            Mixin = typeof(InheritingMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin implementation cannot have a base type other than System.Object: [{0}]", typeof(InheritingMixin).FullName));
        }

        [Test]
        public void CannotImplementOtherInterfaces()
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
                            Mixin = typeof(ExtraInterfaceMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin implementation [{0}] may implement only the mixin definition interface [{1}]",
                typeof(ExtraInterfaceMixin).FullName,
                typeof(IEmptyInterface).FullName));
        }

        [Test]
        public void CannotMixGenericMethod()
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
                            Mixin = typeof(GenericMethodMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin implementation may not include any generic methods: [{0}]",
                typeof(GenericMethodMixin).FullName));
        }

        [Test]
        public void CannotMixOpenGenericNestedType()
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
                            Mixin = typeof(OpenGenericNestedTypeMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin implementation may not include any open generic nested types: [{0}]",
                typeof(OpenGenericNestedTypeMixin).FullName));
        }

        [Test]
        public void CannotMakeUnmanagedCall()
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
                            Mixin = typeof(UnmanagedCallMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Configured mixin implementation may not contain extern methods: [{0}]",
                typeof(UnmanagedCallMixin).FullName));
        }
    }
}
