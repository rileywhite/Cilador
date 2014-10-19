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

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    public class StaticInitializationFixture
    {
        //[Test]
        //public void StaticFieldsAreInitialized()
        //{
        //    var config = new BixMixersConfigType();

        //    config.MixCommandConfig = new MixCommandConfigTypeBase[]
        //    {
        //        new InterfaceMixinConfigType
        //        {
        //            InterfaceMixinMap = new InterfaceMixinMapType[]
        //            {
        //                new InterfaceMixinMapType
        //                {
        //                    Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
        //                    Mixin = typeof(StaticInitializationMixin).GetShortAssemblyQualifiedName()
        //                }
        //            }
        //        },
        //    };

        //    var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
        //    var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.MultipleConstructorsTarget).FullName);

        //    Assert.That(typeof(IForTargetWithMultipleConstructors).IsAssignableFrom(targetType));
        //    targetType.ValidateMemberCountsAre(0, 0, 0, 4, 0, 1);

        //    Assert.That(targetType.GetField("StaticNumberSetTo832778InInitializer", TestContent.BindingFlagsForMixedMembers), Is.EqualTo(832778));
        //    Assert.That(targetType.GetField("StaticNumberInitilizedTo7279848InDeclaration", TestContent.BindingFlagsForMixedMembers), Is.EqualTo(7279848));

        //    var tuple = (Tuple<int, string>)targetType.GetField(
        //        "StaticTupleInitilizedTo485AndBlahInDeclaration",
        //        TestContent.BindingFlagsForMixedMembers).GetValue(null);
        //    Assert.That(tuple.Item1, Is.EqualTo(485));
        //    Assert.That(tuple.Item2, Is.EqualTo("Blah"));

        //    var innerTypeInstance = targetType.GetField(
        //        "InnerTypeSetTo49874AndBlah2AndNewObjectWithObjectInitilizerInDelcaration",
        //        TestContent.BindingFlagsForMixedMembers).GetValue(null);
        //    Assert.That(innerTypeInstance.GetType().GetField("SomeInt").GetValue(innerTypeInstance), Is.EqualTo(49874));
        //    Assert.That(innerTypeInstance.GetType().GetField("SomeString").GetValue(innerTypeInstance), Is.EqualTo("Blah2"));
        //    Assert.That(innerTypeInstance.GetType().GetField("SomeObject").GetValue(innerTypeInstance), Is.Not.Null);
        //}
    }
}
