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
                "First");
        }

        [Test]
        public void FieldsAreInitializedWithConstructorWithArguments()
        {
            this.TestWith(
                new Type[] { typeof(int), typeof(string), typeof(object) },
                new object[] { 138785, "SAJio  oioihIouh UIH UIH PUIHG", new object() },
                138785,
                "SAJio  oioihIouh UIH UIH PUIHG",
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
                "Second");
        }

        private void TestWith(Type[] constructorTypes, object[] constructorArgs, int intValue, string stringValue, string whichBaseConstructor)
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
            targetType.ValidateMemberCountsAre(4, 0, 12, 0, 0, 2);

            var constructor = targetType.GetConstructor(constructorTypes);
            Assert.That(constructor, Is.Not.Null);

            var instanceObject = constructor.Invoke(constructorArgs);
            Assert.That(instanceObject, Is.Not.Null);
            Assert.That(instanceObject is IForTargetWithMultipleConstructors);
            var instance = (IForTargetWithMultipleConstructors)instanceObject;

            Assert.That(instance.GetType().GetField("OriginalInitializedInt").GetValue(instance), Is.EqualTo(48685));
            Assert.That(instance.GetType().GetField("OriginalInitializedString").GetValue(instance), Is.EqualTo("Tion3lao ehiuawh iuh buib ld"));
            Assert.That(instance.GetType().GetField("OriginalInitializedObject").GetValue(instance), Is.Not.Null);

            Assert.That(instance.GetType().GetField("OriginalUninitializedInt").GetValue(instance), Is.EqualTo(intValue));
            Assert.That(instance.GetType().GetField("OriginalUninitializedString").GetValue(instance), Is.EqualTo(stringValue));
            Assert.That(instance.GetType().GetField("OriginalUninitializedObject").GetValue(instance), Is.Not.Null);

            Assert.That(instance.GetType().GetField("WhichConstructor").GetValue(instance), Is.EqualTo(whichBaseConstructor));

            Assert.That(instance.GetType().GetField("SomeNumber").GetValue(instance), Is.EqualTo(684865));
            Assert.That(instance.GetType().GetField("SomeString").GetValue(instance), Is.EqualTo("Tawhlej oisahoeh 8ohf 4ifh8ohe fni dlgj"));
            Assert.That(instance.GetType().GetField("SomeObject").GetValue(instance), Is.Not.Null);

            Assert.That(instance.GetType().GetField("SomeInnerType").GetValue(instance), Is.Not.Null);

            var someFunc = instance.GetType().GetField("SomeFunc").GetValue(instance) as Delegate;
            Assert.That(someFunc, Is.Not.Null);
            var result = someFunc.DynamicInvoke(new object[] { 1, "2", new object() }) as Tuple<int, string, object>;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Item1, Is.EqualTo(1));
            Assert.That(result.Item2, Is.EqualTo("2"));
            Assert.That(result.Item3, Is.Not.Null);

            var someMethodDelegateInstance = instance.GetType().GetField("SomeMethodDelegateInstance").GetValue(instance) as Delegate;
            Assert.That(someMethodDelegateInstance, Is.Not.Null);
            result = someMethodDelegateInstance.DynamicInvoke(new object[] { 3, "4", new object() }) as Tuple<int, string, object>;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Item1, Is.EqualTo(3));
            Assert.That(result.Item2, Is.EqualTo("4"));
            Assert.That(result.Item3, Is.Not.Null);
        }
    }
}
