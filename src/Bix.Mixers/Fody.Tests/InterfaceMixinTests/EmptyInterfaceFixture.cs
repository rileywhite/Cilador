﻿using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
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
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = "Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface, Bix.Mixers.Fody.TestMixinInterfaces",
                            Template = "Bix.Mixers.Fody.TestMixins.EmptyInterfaceTemplate, Bix.Mixers.Fody.TestMixins"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget");
            Assert.That(typeof(Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instance is Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface);
        }

        [Test]
        public void CanAddInterfaceWithContent()
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
                            Interface = "Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface, Bix.Mixers.Fody.TestMixinInterfaces",
                            Template = "Bix.Mixers.Fody.TestMixins.EmptyInterfaceTemplateWithContent, Bix.Mixers.Fody.TestMixins"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget");

            Assert.That(typeof(Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface).IsAssignableFrom(targetType));

            targetType.ValidateMemberCountsAre(2, 3, 1, 1, 0, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            Assert.That(targetType.GetConstructor(new Type[] { typeof(int) }) != null, "Couldn't find new constructor");

            var property = targetType.GetProperty("SomeValue");
            Assert.That(property != null, "Couldn't find new property SomeValue");

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
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[0]
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget");
            Assert.That(!typeof(Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
            Assert.That(targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 1, "Expected 1 constructor");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");
            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(!(instance is Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface));
        }
    }
}