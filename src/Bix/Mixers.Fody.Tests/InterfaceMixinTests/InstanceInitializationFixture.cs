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

using Bix.Mixers.Fody.Config;
using Bix.Mixers.Fody.Tests.Common;
using Bix.Mixers.Fody.TestMixinInterfaces;
using Bix.Mixers.Fody.TestMixins;
using NUnit.Framework;
using System;

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    public class InstanceInitializationFixture
    {
        [Test]
        public void FieldsAreInitializedWithDefaultConstructor()
        {
            this.TestWith(
                new Type[0],
                new object[0],
                783535,
                "KNion wineofn oianweiof nqiognui ndf",
                "j.k.l",
                "First");
        }

        [Test]
        public void FieldsAreInitializedWithConstructorWithArguments()
        {
            this.TestWith(
                new Type[] { typeof(int), typeof(string), typeof(UriBuilder) },
                new object[] { 138785, "SAJio  oioihIouh UIH UIH PUIHG", new UriBuilder { Host = "m.n.o" } },
                138785,
                "SAJio  oioihIouh UIH UIH PUIHG",
                "m.n.o",
                "Second");
        }

        [Test]
        public void FieldsAreInitializedWithConstructorWithArgumentsThatCallsADifferentConstructor()
        {
            this.TestWith(
                new Type[] { typeof(int), typeof(string) },
                new object[] { 684684, ":POIenuiofh oie hioeh goiuh iu g" },
                684684,
                ":POIenuiofh oie hioeh goiuh iu g",
                "d.e.f",
                "Second");
        }

        [Test]
        public void FieldsAreInitializedWithConstructorWithArgumentsThatCallsThroughTwoConstructors()
        {
            this.TestWith(
                new Type[] { typeof(int) },
                new object[] { 90874389 },
                90874389,
                "A iuohiogfniouhe uihui iu.",
                "g.h.i",
                "Second");
        }

        private void TestWith(
            Type[] constructorTypes,
            object[] constructorArgs,
            int intValue,
            string stringValue,
            string uriHostValue,
            string whichBaseConstructor)
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
                            Interface = typeof(IForTargetWithMultipleConstructors).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(InstanceInitializationMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.MultipleConstructorsTarget).FullName);

            Assert.That(typeof(IForTargetWithMultipleConstructors).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(4, 1, 13, 0, 0, 2);

            var constructor = targetType.GetConstructor(constructorTypes);
            Assert.That(constructor, Is.Not.Null);

            var instanceObject = constructor.Invoke(constructorArgs);
            Assert.That(instanceObject, Is.Not.Null);
            Assert.That(instanceObject is IForTargetWithMultipleConstructors);
            var instance = (IForTargetWithMultipleConstructors)instanceObject;

            Assert.That(targetType.GetField("SomeNumberSetTo395493InConstructor").GetValue(instance), Is.EqualTo(395493));

            Assert.That(targetType.GetField("OriginalInitializedInt").GetValue(instance), Is.EqualTo(48685));
            Assert.That(targetType.GetField("OriginalInitializedString").GetValue(instance), Is.EqualTo("Tion3lao ehiuawh iuh buib ld"));
            var innerObject = (UriBuilder)targetType.GetField("OriginalInitializedObject").GetValue(instance);
            Assert.That(innerObject, Is.Not.Null);
            Assert.That(innerObject.Host, Is.EqualTo("a.b.c"));

            Assert.That(targetType.GetField("OriginalUninitializedInt").GetValue(instance), Is.EqualTo(intValue));
            Assert.That(targetType.GetField("OriginalUninitializedString").GetValue(instance), Is.EqualTo(stringValue));
            Assert.That(innerObject, Is.Not.Null);
            innerObject = (UriBuilder)targetType.GetField("OriginalUninitializedObject").GetValue(instance);
            Assert.That(innerObject, Is.Not.Null);
            Assert.That(innerObject.Host, Is.EqualTo(uriHostValue));

            Assert.That(targetType.GetField("WhichConstructor").GetValue(instance), Is.EqualTo(whichBaseConstructor));

            Assert.That(targetType.GetField("SomeNumber").GetValue(instance), Is.EqualTo(684865));
            Assert.That(targetType.GetField("SomeString").GetValue(instance), Is.EqualTo("Tawhlej oisahoeh 8ohf 4ifh8ohe fni dlgj"));
            Assert.That(targetType.GetField("SomeObject").GetValue(instance), Is.Not.Null);

            var innerTypeInstance = targetType.GetField("SomeInnerType").GetValue(instance);
            Assert.That(innerTypeInstance, Is.Not.Null);
            var innerType = innerTypeInstance.GetType();
            Assert.That(innerType.GetProperty("SomeInt").GetValue(innerTypeInstance), Is.EqualTo(4235));
            Assert.That(innerType.GetProperty("SomeString").GetValue(innerTypeInstance), Is.EqualTo("JLKOIN  oin aon oingori d"));
            Assert.That(innerType.GetProperty("SomeObject").GetValue(innerTypeInstance), Is.Not.Null);

            var someFunc = targetType.GetField("SomeFunc").GetValue(instance) as Delegate;
            Assert.That(someFunc, Is.Not.Null);
            var result = someFunc.DynamicInvoke(new object[] { 1, "2", new object() }) as Tuple<int, string, object>;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Item1, Is.EqualTo(1));
            Assert.That(result.Item2, Is.EqualTo("2"));
            Assert.That(result.Item3, Is.Not.Null);

            var someMethodDelegateInstance = targetType.GetField("SomeMethodDelegateInstance").GetValue(instance) as Delegate;
            Assert.That(someMethodDelegateInstance, Is.Not.Null);
            result = someMethodDelegateInstance.DynamicInvoke(new object[] { 3, "4", new object() }) as Tuple<int, string, object>;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Item1, Is.EqualTo(3));
            Assert.That(result.Item2, Is.EqualTo("4"));
            Assert.That(result.Item3, Is.Not.Null);
        }
    }
}
