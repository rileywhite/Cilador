/***************************************************************************/
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
using Cilador.Fody.TestMixinInterfaces;
using Cilador.Fody.TestMixins;
using Cilador.Fody.TestMixinTargets;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    public class StaticInitializationFixture
    {
        [Test]
        public void StaticConstructorIsClonedForTargetWithoutStaticConstructor()
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
                            Mixin = typeof(StaticInitializationMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);

            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(2, 0, 6, 0, 0, 0);

            Assert.That(targetType.GetField("MixedUninitializedInt", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(3788448));
            Assert.That(targetType.GetField("MixedUninitializedString", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo("APONION ioniosdnfionaiuhg "));
            var uriBuilder = targetType.GetField("MixedUninitializedObject", TestContent.BindingFlagsForWeavedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("j.k.l"));

            Assert.That(targetType.GetField("MixedInitializedInt", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(438974789));
            Assert.That(targetType.GetField("MixedInitializedString", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo("QKLdsionioj ioenu buif"));
            uriBuilder = targetType.GetField("MixedInitializedObject", TestContent.BindingFlagsForWeavedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("g.h.i"));
        }

        [Test]
        public void StaticConstructorIsClonedForTargetWithStaticConstructor()
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
                            Interface = typeof(IForTargetWithStaticConstructors).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(StaticInitializationMixinForTargetWithStaticConstructor).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(StaticConstructorsTarget).FullName);

            Assert.That(typeof(IForTargetWithStaticConstructors).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(2, 1, 12, 0, 0, 0);

            Assert.That(targetType.BaseType.GetField("SomeNumberSetTo56561InStaticConstructor", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(56561));
            Assert.That(targetType.BaseType.GetField("SomeNumberInitializedTo8188", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(8188));

            Assert.That(targetType.GetField("OriginalUninitializedInt", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(9834897));
            Assert.That(targetType.GetField("OriginalUninitializedString", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo("QWEhinIOnsonf nui uif"));
            var uriBuilder = targetType.GetField("OriginalUninitializedObject", TestContent.BindingFlagsForWeavedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("d.e.f"));

            Assert.That(targetType.GetField("OriginalInitializedInt", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(35881));
            Assert.That(targetType.GetField("OriginalInitializedString", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo("Elak Ion fiugrnn"));
            uriBuilder = targetType.GetField("OriginalInitializedObject", TestContent.BindingFlagsForWeavedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("a.b.c"));

            Assert.That(targetType.GetField("MixedUninitializedInt", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(3788448));
            Assert.That(targetType.GetField("MixedUninitializedString", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo("APONION ioniosdnfionaiuhg "));
            uriBuilder = targetType.GetField("MixedUninitializedObject", TestContent.BindingFlagsForWeavedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("j.k.l"));

            Assert.That(targetType.GetField("MixedInitializedInt", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo(438974789));
            Assert.That(targetType.GetField("MixedInitializedString", TestContent.BindingFlagsForWeavedMembers).GetValue(null), Is.EqualTo("QKLdsionioj ioenu buif"));
            uriBuilder = targetType.GetField("MixedInitializedObject", TestContent.BindingFlagsForWeavedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("g.h.i"));
        }
    }
}
