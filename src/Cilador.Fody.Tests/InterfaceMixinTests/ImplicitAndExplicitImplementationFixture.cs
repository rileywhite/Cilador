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
using Cilador.Fody.Core;
using Cilador.Fody.InterfaceMixins;
using Cilador.Fody.TestMixinInterfaces;
using Cilador.Fody.TestMixins;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{
    using Cilador.Fody.Config;
    using Cilador.Fody.TestMixinInterfaces;
    using Cilador.Fody.TestMixins;
    using Cilador.Fody.TestMixinTargets;
    using Cilador.Fody.Tests.Common;

    [TestFixture]
    internal class ImplicitAndExplicitImplementationFixture
    {
        [Test]
        public void CanImplementImplicitly()
        {
            var config = new CiladorConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IInterfaceForImplicitExplicitTesting).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(ImplicitExplicitTestingImplicitOnlyMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(InterfaceForImplicitExplicitTestingTarget).FullName);

            Assert.That(typeof(IInterfaceForImplicitExplicitTesting).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 12, 3, 3, 3, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceForImplicitExplicitTesting);
            var instanceInterface = (IInterfaceForImplicitExplicitTesting)instanceObject;

            Assert.That("Implicit Method 1".Equals(
                targetType.GetMethod("Method1", TestContent.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That("Implicit Method 2".Equals(
                targetType.GetMethod("Method2", TestContent.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That("Implicit Method 3".Equals(
                targetType.GetMethod("Method3", TestContent.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));

            Assert.That("Implicit Method 1".Equals(instanceInterface.Method1()));
            Assert.That("Implicit Method 2".Equals(instanceInterface.Method2()));
            Assert.That("Implicit Method 3".Equals(instanceInterface.Method3()));

            Assert.That("Implicit Property 1".Equals(
                targetType.GetProperty("Property1", TestContent.BindingFlagsForMixedMembers).GetValue(instanceObject)));
            Assert.That("Implicit Property 2".Equals(
                targetType.GetProperty("Property2", TestContent.BindingFlagsForMixedMembers).GetValue(instanceObject)));
            Assert.That("Implicit Property 3".Equals(
                targetType.GetProperty("Property3", TestContent.BindingFlagsForMixedMembers).GetValue(instanceObject)));

            Assert.That("Implicit Property 1".Equals(instanceInterface.Property1));
            Assert.That("Implicit Property 2".Equals(instanceInterface.Property2));
            Assert.That("Implicit Property 3".Equals(instanceInterface.Property3));

            EventHandler eventHandler = (sender, eventArgs) => { return; };

            var typeEvent1 = targetType.GetEvent("Event1", TestContent.BindingFlagsForMixedMembers);
            Assert.That(typeEvent1 != null);
            var typeEvent2 = targetType.GetEvent("Event2", TestContent.BindingFlagsForMixedMembers);
            Assert.That(typeEvent2 != null);
            var typeEvent3 = targetType.GetEvent("Event3", TestContent.BindingFlagsForMixedMembers);
            Assert.That(typeEvent2 != null);

            var implicitEventHandler1 = targetType.GetField("Event1", TestContent.BindingFlagsForMixedMembers);
            Assert.That(implicitEventHandler1 != null);
            var implicitEventHandler2 = targetType.GetField("Event2", TestContent.BindingFlagsForMixedMembers);
            Assert.That(implicitEventHandler2 != null);
            var implicitEventHandler3 = targetType.GetField("Event3", TestContent.BindingFlagsForMixedMembers);
            Assert.That(implicitEventHandler3 != null);

            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);

            typeEvent1.AddEventHandler(instanceObject, eventHandler);
            Assert.That(object.Equals(implicitEventHandler1.GetValue(instanceObject), eventHandler));
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);
            typeEvent1.RemoveEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);

            typeEvent2.AddEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(object.Equals(implicitEventHandler2.GetValue(instanceObject), eventHandler));
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);
            typeEvent2.RemoveEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);

            typeEvent3.AddEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(object.Equals(implicitEventHandler3.GetValue(instanceObject), eventHandler));
            typeEvent3.RemoveEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event1 += eventHandler;
            Assert.That(object.Equals(implicitEventHandler1.GetValue(instanceObject), eventHandler));
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);
            instanceInterface.Event1 -= eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event2 += eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(object.Equals(implicitEventHandler2.GetValue(instanceObject), eventHandler));
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);
            instanceInterface.Event2 -= eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event3 += eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(object.Equals(implicitEventHandler3.GetValue(instanceObject), eventHandler));
            instanceInterface.Event3 -= eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler3.GetValue(instanceObject) == null);
        }

        [Test]
        public void CanImplementExplicitly()
        {
            var config = new CiladorConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IInterfaceForImplicitExplicitTesting).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(ImplicitExplicitTestingExplicitOnlyMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(InterfaceForImplicitExplicitTestingTarget).FullName);

            Assert.That(typeof(IInterfaceForImplicitExplicitTesting).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 12, 3, 3, 3, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceForImplicitExplicitTesting);
            var instanceInterface = (IInterfaceForImplicitExplicitTesting)instanceObject;

            Assert.That(targetType.GetMethod("Method1", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetMethod("Method2", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetMethod("Method3", TestContent.BindingFlagsForMixedMembers) == null);

            Assert.That("Explicit Method 1".Equals(instanceInterface.Method1()));
            Assert.That("Explicit Method 2".Equals(instanceInterface.Method2()));
            Assert.That("Explicit Method 3".Equals(instanceInterface.Method3()));

            Assert.That(targetType.GetProperty("Property1", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetProperty("Property2", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetProperty("Property3", TestContent.BindingFlagsForMixedMembers) == null);

            Assert.That("Explicit Property 1".Equals(instanceInterface.Property1));
            Assert.That("Explicit Property 2".Equals(instanceInterface.Property2));
            Assert.That("Explicit Property 3".Equals(instanceInterface.Property3));

            EventHandler eventHandler = (sender, eventArgs) => { return; };

            Assert.That(targetType.GetEvent("Event1", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetEvent("Event2", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetEvent("Event3", TestContent.BindingFlagsForMixedMembers) == null);

            Assert.That(targetType.GetField("Event1", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetField("Event2", TestContent.BindingFlagsForMixedMembers) == null);
            Assert.That(targetType.GetField("Event3", TestContent.BindingFlagsForMixedMembers) == null);

            var explicitEventHandler1 = targetType.GetField("explicitEventHandler1", TestContent.BindingFlagsForMixedMembers);
            Assert.That(explicitEventHandler1 != null);
            var explicitEventHandler2 = targetType.GetField("explicitEventHandler2", TestContent.BindingFlagsForMixedMembers);
            Assert.That(explicitEventHandler2 != null);
            var explicitEventHandler3 = targetType.GetField("explicitEventHandler3", TestContent.BindingFlagsForMixedMembers);
            Assert.That(explicitEventHandler3 != null);

            Assert.That(explicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event1 += eventHandler;
            Assert.That(object.Equals(explicitEventHandler1.GetValue(instanceObject), eventHandler));
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
            instanceInterface.Event1 -= eventHandler;
            Assert.That(explicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event2 += eventHandler;
            Assert.That(explicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(object.Equals(explicitEventHandler2.GetValue(instanceObject), eventHandler));
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
            instanceInterface.Event2 -= eventHandler;
            Assert.That(explicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event3 += eventHandler;
            Assert.That(explicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(object.Equals(explicitEventHandler3.GetValue(instanceObject), eventHandler));
            instanceInterface.Event3 -= eventHandler;
            Assert.That(explicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
        }

        [Test]
        public void CanImplementMixed()
        {
            var config = new CiladorConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IInterfaceForImplicitExplicitTesting).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(ImplicitExplicitTestingMixedMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(InterfaceForImplicitExplicitTestingTarget).FullName);

            Assert.That(typeof(IInterfaceForImplicitExplicitTesting).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 16, 4, 4, 4, 0);

            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceForImplicitExplicitTesting);
            var instanceInterface = (IInterfaceForImplicitExplicitTesting)instanceObject;

            Assert.That("Implicit Method 1".Equals(
                targetType.GetMethod("Method1", TestContent.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That("Independent Method 2".Equals(
                targetType.GetMethod("Method2", TestContent.BindingFlagsForMixedMembers).Invoke(instanceObject, new object[] { })));
            Assert.That(targetType.GetMethod("Method3", TestContent.BindingFlagsForMixedMembers) == null);

            Assert.That("Implicit Method 1".Equals(instanceInterface.Method1()));
            Assert.That("Explicit Method 2".Equals(instanceInterface.Method2()));
            Assert.That("Explicit Method 3".Equals(instanceInterface.Method3()));

            Assert.That("Implicit Property 1".Equals(
                targetType.GetProperty("Property1", TestContent.BindingFlagsForMixedMembers).GetValue(instanceObject)));
            Assert.That("Independent Property 2".Equals(
                targetType.GetProperty("Property2", TestContent.BindingFlagsForMixedMembers).GetValue(instanceObject)));
            Assert.That(targetType.GetProperty("Property3", TestContent.BindingFlagsForMixedMembers) == null);

            Assert.That("Implicit Property 1".Equals(instanceInterface.Property1));
            Assert.That("Explicit Property 2".Equals(instanceInterface.Property2));
            Assert.That("Explicit Property 3".Equals(instanceInterface.Property3));

            EventHandler eventHandler = (sender, eventArgs) => { return; };

            var typeEvent1 = targetType.GetEvent("Event1", TestContent.BindingFlagsForMixedMembers);
            Assert.That(typeEvent1 != null);
            var typeEvent2 = targetType.GetEvent("Event2", TestContent.BindingFlagsForMixedMembers);
            Assert.That(typeEvent2 != null);
            Assert.That(targetType.GetEvent("Event3", TestContent.BindingFlagsForMixedMembers) == null);

            var implicitEventHandler1 = targetType.GetField("Event1", TestContent.BindingFlagsForMixedMembers);
            Assert.That(implicitEventHandler1 != null);
            var implicitEventHandler2 = targetType.GetField("Event2", TestContent.BindingFlagsForMixedMembers);
            Assert.That(implicitEventHandler2 != null);
            Assert.That(targetType.GetField("Event3", TestContent.BindingFlagsForMixedMembers) == null);
            var explicitEventHandler2 = targetType.GetField("explicitEventHandler2", TestContent.BindingFlagsForMixedMembers);
            Assert.That(explicitEventHandler2 != null);
            var explicitEventHandler3 = targetType.GetField("explicitEventHandler3", TestContent.BindingFlagsForMixedMembers);
            Assert.That(explicitEventHandler3 != null);

            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            typeEvent1.AddEventHandler(instanceObject, eventHandler);
            Assert.That(object.Equals(implicitEventHandler1.GetValue(instanceObject), eventHandler));
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
            typeEvent1.RemoveEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            typeEvent2.AddEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(object.Equals(implicitEventHandler2.GetValue(instanceObject), eventHandler));
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
            typeEvent2.RemoveEventHandler(instanceObject, eventHandler);
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event1 += eventHandler;
            Assert.That(object.Equals(implicitEventHandler1.GetValue(instanceObject), eventHandler));
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
            instanceInterface.Event1 -= eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event2 += eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(object.Equals(explicitEventHandler2.GetValue(instanceObject), eventHandler));
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
            instanceInterface.Event2 -= eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);

            instanceInterface.Event3 += eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(object.Equals(explicitEventHandler3.GetValue(instanceObject), eventHandler));
            instanceInterface.Event3 -= eventHandler;
            Assert.That(implicitEventHandler1.GetValue(instanceObject) == null);
            Assert.That(implicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler2.GetValue(instanceObject) == null);
            Assert.That(explicitEventHandler3.GetValue(instanceObject) == null);
        }
    }
}
