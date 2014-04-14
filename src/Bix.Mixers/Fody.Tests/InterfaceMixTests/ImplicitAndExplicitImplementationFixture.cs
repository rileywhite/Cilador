using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixing;
using Bix.Mixers.Fody.TestInterfaces;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.InterfaceMixTests
{
    [TestFixture]
    internal class ImplicitAndExplicitImplementationFixture
    {
        [Test]
        public void CanImplementImplicitly()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IInterfaceForImplicitExplicitTesting, Bix.Mixers.Fody.TestInterfaces",
                            Template = "Bix.Mixers.Fody.TestSources.InterfaceForImplicitExplicitTestingImplicitOnlyTemplate, Bix.Mixers.Fody.TestSources"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.InterfaceForImplicitExplicitTestingTarget");

            Assert.That(typeof(IInterfaceForImplicitExplicitTesting).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 3, 0, 0, 0, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceForImplicitExplicitTesting);

            Assert.That("Implicit 1".Equals(
                targetType.GetMethod("Method1", CommonExtensions.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That("Implicit 2".Equals(
                targetType.GetMethod("Method2", CommonExtensions.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That("Implicit 3".Equals(
                targetType.GetMethod("Method3", CommonExtensions.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));

            var instance = (IInterfaceForImplicitExplicitTesting)instanceObject;

            Assert.That("Implicit 1".Equals(instance.Method1()));
            Assert.That("Implicit 2".Equals(instance.Method2()));
            Assert.That("Implicit 3".Equals(instance.Method3()));
        }

        [Test]
        public void CanImplementExplicitly()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IInterfaceForImplicitExplicitTesting, Bix.Mixers.Fody.TestInterfaces",
                            Template = "Bix.Mixers.Fody.TestSources.InterfaceForImplicitExplicitTestingExplicitOnlyTemplate, Bix.Mixers.Fody.TestSources"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.InterfaceForImplicitExplicitTestingTarget");

            Assert.That(typeof(IInterfaceForImplicitExplicitTesting).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 3, 0, 0, 0, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceForImplicitExplicitTesting);

            Assert.That(targetType.GetMethod("Method1", CommonExtensions.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetMethod("Method2", CommonExtensions.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetMethod("Method3", CommonExtensions.BindingFlagsForMixedMembers) == null);

            var instance = (IInterfaceForImplicitExplicitTesting)instanceObject;

            Assert.That("Explicit 1".Equals(instance.Method1()));
            Assert.That("Explicit 2".Equals(instance.Method2()));
            Assert.That("Explicit 3".Equals(instance.Method3()));
        }

        [Test]
        public void CanImplementMixed()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IInterfaceForImplicitExplicitTesting, Bix.Mixers.Fody.TestInterfaces",
                            Template = "Bix.Mixers.Fody.TestSources.InterfaceForImplicitExplicitTestingMixedTemplate, Bix.Mixers.Fody.TestSources"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.InterfaceForImplicitExplicitTestingTarget");

            Assert.That(typeof(IInterfaceForImplicitExplicitTesting).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 4, 0, 0, 0, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceForImplicitExplicitTesting);

            Assert.That("Implicit 1".Equals(
                targetType.GetMethod("Method1", CommonExtensions.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That("Independent 2".Equals(
                targetType.GetMethod("Method2", CommonExtensions.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That(targetType.GetMethod("Method3", CommonExtensions.BindingFlagsForMixedMembers) == null);

            var instance = (IInterfaceForImplicitExplicitTesting)instanceObject;

            Assert.That("Implicit 1".Equals(instance.Method1()));
            Assert.That("Explicit 2".Equals(instance.Method2()));
            Assert.That("Explicit 3".Equals(instance.Method3()));
        }
    }
}
