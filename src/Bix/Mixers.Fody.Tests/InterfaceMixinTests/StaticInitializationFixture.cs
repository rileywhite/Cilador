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

using Bix.Mixers.Fody.Config;
using Bix.Mixers.Fody.TestMixinInterfaces;
using Bix.Mixers.Fody.TestMixins;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    public class StaticInitializationFixture
    {
        [Test]
        public void StaticConstructorIsClonedForTargetWithoutStaticConstructor()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);

            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(2, 0, 6, 0, 0, 0);

            Assert.That(targetType.GetField("MixedUninitializedInt", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(3788448));
            Assert.That(targetType.GetField("MixedUninitializedString", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo("APONION ioniosdnfionaiuhg "));
            var uriBuilder = targetType.GetField("MixedUninitializedObject", TestContent.BindingFlagsForMixedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("j.k.l"));

            Assert.That(targetType.GetField("MixedInitializedInt", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(438974789));
            Assert.That(targetType.GetField("MixedInitializedString", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo("QKLdsionioj ioenu buif"));
            uriBuilder = targetType.GetField("MixedInitializedObject", TestContent.BindingFlagsForMixedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("g.h.i"));
        }

        [Test]
        public void StaticConstructorIsClonedForTargetWithStaticConstructor()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.StaticConstructorsTarget).FullName);

            Assert.That(typeof(IForTargetWithStaticConstructors).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(2, 1, 12, 0, 0, 0);

            Assert.That(targetType.BaseType.GetField("SomeNumberSetTo56561InStaticConstructor", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(56561));
            Assert.That(targetType.BaseType.GetField("SomeNumberInitializedTo8188", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(8188));

            Assert.That(targetType.GetField("OriginalUninitializedInt", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(9834897));
            Assert.That(targetType.GetField("OriginalUninitializedString", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo("QWEhinIOnsonf nui uif"));
            var uriBuilder = targetType.GetField("OriginalUninitializedObject", TestContent.BindingFlagsForMixedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("d.e.f"));

            Assert.That(targetType.GetField("OriginalInitializedInt", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(35881));
            Assert.That(targetType.GetField("OriginalInitializedString", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo("Elak Ion fiugrnn"));
            uriBuilder = targetType.GetField("OriginalInitializedObject", TestContent.BindingFlagsForMixedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("a.b.c"));

            Assert.That(targetType.GetField("MixedUninitializedInt", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(3788448));
            Assert.That(targetType.GetField("MixedUninitializedString", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo("APONION ioniosdnfionaiuhg "));
            uriBuilder = targetType.GetField("MixedUninitializedObject", TestContent.BindingFlagsForMixedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("j.k.l"));

            Assert.That(targetType.GetField("MixedInitializedInt", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo(438974789));
            Assert.That(targetType.GetField("MixedInitializedString", TestContent.BindingFlagsForMixedMembers).GetValue(null), Is.EqualTo("QKLdsionioj ioenu buif"));
            uriBuilder = targetType.GetField("MixedInitializedObject", TestContent.BindingFlagsForMixedMembers).GetValue(null) as UriBuilder;
            Assert.That(uriBuilder, Is.Not.Null);
            Assert.That(uriBuilder.Host, Is.EqualTo("g.h.i"));
        }
    }
}
