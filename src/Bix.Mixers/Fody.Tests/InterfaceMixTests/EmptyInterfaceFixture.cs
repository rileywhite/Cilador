using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixing;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Bix.Mixers.Fody.Tests.InterfaceMixTests
{
    [TestFixture]
    internal class EmptyInterfaceFixture
    {
        [Test]
        public void CanAddInterface()
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
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IEmptyInterface, Bix.Mixers.Fody.TestInterfaces",
                            Template = "Bix.Mixers.Fody.TestSources.EmptyInterfaceTemplate, Bix.Mixers.Fody.TestSources"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.EmptyInterfaceTarget");
            Assert.That(typeof(Bix.Mixers.Fody.TestInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
            Assert.That(targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 1, "Expected 1 constructor");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instance is Bix.Mixers.Fody.TestInterfaces.IEmptyInterface);
        }

        [Test]
        public void CanAddInterfaceWithContent()
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
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IEmptyInterface, Bix.Mixers.Fody.TestInterfaces",
                            Template = "Bix.Mixers.Fody.TestSources.EmptyInterfaceTemplateWithContent, Bix.Mixers.Fody.TestSources"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.EmptyInterfaceTarget");

            Assert.That(typeof(Bix.Mixers.Fody.TestInterfaces.IEmptyInterface).IsAssignableFrom(targetType));

            Assert.That(targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 2, "Expected 2 constructors");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            Assert.That(targetType.GetConstructor(new Type[] { typeof(int) }) != null, "Couldn't find new constructor");

            Assert.That(targetType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 1, "Expected 1 field");

            Assert.That(targetType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 1, "Expected 1 property");
            var property = targetType.GetProperty("SomeValue");
            Assert.That(property != null, "Couldn't find new property SomeValue");

            Assert.That(targetType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 3, "Expected 3 non-constructor methods");
            var method = targetType.GetMethod("SomeMethod");
            Assert.That(method != null, "Couldn't find new method SomeMethod");

            var instance = Activator.CreateInstance(targetType, new object[0]);
            method.Invoke(instance, new object[0]);   // make sure no error is raised
            Assert.That(instance != null, "Failed to create instance with existing default constructor");
            Assert.That(0.Equals(property.GetValue(instance)));
            instance = Activator.CreateInstance(targetType, new object[] { 32 });
            Assert.That(instance != null, "Failed to create instance with new constructor");
            Assert.That(32.Equals(property.GetValue(instance)));
            property.SetValue(instance, 45);
            Assert.That(45.Equals(property.GetValue(instance)));
        }

        [Test]
        public void UnconfiguredInterfaceIsSkipped()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixConfigType
                {
                    InterfaceMap = new InterfaceMapType[0]
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.EmptyInterfaceTarget");
            Assert.That(!typeof(Bix.Mixers.Fody.TestInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
            Assert.That(targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 1, "Expected 1 constructor");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(!(instance is Bix.Mixers.Fody.TestInterfaces.IEmptyInterface));
        }
    }
}
