/***************************************************************************/
// Copyright 2013-2014 Riley White
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

using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestMixinInterfaces;
using Bix.Mixers.Fody.TestMixins;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation type cannot be abstract: [{0}]",
                    typeof(AbstractMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation type cannot be an open generic type: [{0}]",
                    typeof(OpenGenericMixin<>).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation type must be a reference type: [{0}]",
                    typeof(InterfaceTypeMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation type must be a reference type: [{0}]",
                    typeof(ValueTypeMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation type [{0}] must implement the interface specified mixin interface definition [{1}]",
                    typeof(InterfacelessMixin).FullName,
                    typeof(IEmptyInterface).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin definition interface type is not an interface: [{0}]",
                    typeof(NotAValidMixinInterface).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation cannot have a type initializer (i.e. static constructor): [{0}]",
                    typeof(TypeInitializerMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation cannot have a base type other than System.Object: [{0}]",
                    typeof(InheritingMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation [{0}] may implement only the mixin definition interface [{1}]",
                    typeof(ExtraInterfaceMixin).FullName,
                    typeof(IEmptyInterface).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation may not include any generic methods: [{0}]",
                    typeof(GenericMethodMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation may not include any open generic nested types: [{0}]",
                    typeof(OpenGenericNestedTypeMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
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

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation may not contain extern methods: [{0}]",
                    typeof(UnmanagedCallMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
        }

        [Test]
        public void CannotHaveSecurityAttributeOnMixinImplementation()
        {
            var config = new BixMixersConfigType();
            ;
            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(SecurityDeclarationMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation may not be annotated with security attributes: [{0}]",
                    typeof(SecurityDeclarationMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
        }

        [Test]
        public void CannotHaveSecurityAttributeOnNestedType()
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
                            Mixin = typeof(SecurityDeclarationOnNestedTypeMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation may not contain nested types annotated with security attributes: [{0}]",
                    typeof(SecurityDeclarationOnNestedTypeMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
        }

        [Test]
        public void CannotHaveSecurityAttributeOnMethod()
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
                            Mixin = typeof(SecurityDeclarationOnMethodMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws(
                Is.TypeOf((typeof(WeavingException)))
                .And.Message.EqualTo(string.Format(
                    "Configured mixin implementation may not contain methods annotated with security attributes: [{0}]",
                    typeof(SecurityDeclarationOnMethodMixin).FullName)),
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config));
        }
    }
}
