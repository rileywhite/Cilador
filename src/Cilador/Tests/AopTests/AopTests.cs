/***************************************************************************/
// Copyright 2013-2019 Riley White
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

using Cilador.Aop.Core;
using Cilador.Aop.IntroduceType;
using Cilador.Aop.Decorate;
using Cilador.Graph.Factory;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Cilador.Tests
{
    [TestFixture]
    public class AopTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            using (var resolver = new DefaultAssemblyResolver())
            using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
            {
                var loom = new Loom();
                var graphGetter = new CilGraphGetter();

                using (var runWithoutReplacementDecoration = new ActionDecorator<string[]>(resolver, graphGetter, RunWithoutReplacementDecoration, name => $"{name}_Decorated"))
                using (var runWithReplacementDecorator = new ActionDecorator<string>(resolver, graphGetter, RunWithReplacementDecorator))
                {
                    loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(new PointCut<MethodDefinition>(m => m.Name == "RunWithoutReplacement"), runWithoutReplacementDecoration));
                    loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(new PointCut<MethodDefinition>(m => m.Name == "RunWithReplacement"), runWithReplacementDecorator));

                    loom.Weave(targetAssembly);
                }
                targetAssembly.Write();
            }
        }

        #region Decorations

        private static void RunWithoutReplacementDecoration(string[] args)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var thingsThatHaveRun = (Dictionary<string, object[]>)currentType
                .GetProperty("ThingsThatHaveRun", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            thingsThatHaveRun.Add("Decorated RunWithoutReplacement", args);

            Forwarders.ForwardToOriginalAction(string.Join(", ", args));
        }

        private static void RunWithReplacementDecorator(string arg)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var thingsThatHaveRun = (Dictionary<string, object[]>)currentType
                .GetProperty("ThingsThatHaveRun", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            thingsThatHaveRun.Add("Decorated RunWithReplacement", new object[] { arg });

            Forwarders.ForwardToOriginalAction(arg);
        }

        #endregion

        [Test]
        public void CanDecorateInstanceActionMethodWithoutReplacement()
        {
            var sentArgs = new string[] { "1", "2", "3" };

            var thingsThatHaveRun = new Dictionary<string, object[]>();
            try
            {
                TestAopTarget.InstanceTarget.ThingsThatHaveRun = thingsThatHaveRun;
                var instanceTarget = new TestAopTarget.InstanceTarget();
                typeof(TestAopTarget.InstanceTarget).GetMethod("RunWithoutReplacement_Decorated").Invoke(instanceTarget, new object[] { sentArgs });
            }
            finally
            {
                TestAopTarget.InstanceTarget.ThingsThatHaveRun = null;
            }

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Decorated RunWithoutReplacement",
                out var decoratedMethodArgs));
            Assert.AreSame(sentArgs, decoratedMethodArgs);

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Instance RunWithoutReplacement",
                out var forwardedArgs));

            Assert.NotNull(forwardedArgs);
            Assert.That(forwardedArgs.Length == 1);
            Assert.AreEqual("1, 2, 3", forwardedArgs[0]);
        }

        [Test]
        public void CanDecorateStaticActionMethodWithoutReplacement()
        {
            var sentArgs = new string[] { "1", "2", "3" };

            var thingsThatHaveRun = new Dictionary<string, object[]>();
            try
            {
                TestAopTarget.StaticTarget.ThingsThatHaveRun = thingsThatHaveRun;
                typeof(TestAopTarget.StaticTarget).GetMethod("RunWithoutReplacement_Decorated").Invoke(null, new object[] { sentArgs });
            }
            finally
            {
                TestAopTarget.StaticTarget.ThingsThatHaveRun = null;
            }

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Decorated RunWithoutReplacement",
                out var decoratedMethodArgs));
            Assert.AreSame(sentArgs, decoratedMethodArgs);

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Static RunWithoutReplacement",
                out var forwardedArgs));

            Assert.NotNull(forwardedArgs);
            Assert.That(forwardedArgs.Length == 1);
            Assert.AreEqual("1, 2, 3", forwardedArgs[0]);
        }


        [Test]
        public void CanDecorateInstanceActionMethodWithReplacement()
        {
            var thingsThatHaveRun = new Dictionary<string, object[]>();
            try
            {
                TestAopTarget.InstanceTarget.ThingsThatHaveRun = thingsThatHaveRun;
                var instanceTarget = new TestAopTarget.InstanceTarget();
                instanceTarget.RunWithReplacement("iuehtoinjf98ewrhnfew");
            }
            finally
            {
                TestAopTarget.InstanceTarget.ThingsThatHaveRun = null;
            }

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Decorated RunWithReplacement",
                out var decoratedMethodArgs));
            Assert.AreSame("iuehtoinjf98ewrhnfew", decoratedMethodArgs);

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                "Instance RunWithReplacement",
                out var forwardedArgs));

            Assert.NotNull(forwardedArgs);
            Assert.That(forwardedArgs.Length == 1);
            Assert.AreEqual("iuehtoinjf98ewrhnfew", forwardedArgs[0]);
        }

        [Test]
        public void CanDecorateStaticActionMethodWithReplacement()
        {
            var thingsThatHaveRun = new Dictionary<string, object[]>();
            try
            {
                TestAopTarget.StaticTarget.ThingsThatHaveRun = thingsThatHaveRun;
                TestAopTarget.StaticTarget.RunWithReplacement("fkjsadhfjksdjkf");
            }
            finally
            {
                TestAopTarget.StaticTarget.ThingsThatHaveRun = null;
            }

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Decorated RunWithReplacement",
                out var decoratedMethodArgs));
            Assert.AreSame("fkjsadhfjksdjkf", decoratedMethodArgs);

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"Static RunWithReplacement",
                out var forwardedArgs));

            Assert.NotNull(forwardedArgs);
            Assert.That(forwardedArgs.Length == 1);
            Assert.AreEqual("fkjsadhfjksdjkf", forwardedArgs[0]);
        }

        [Test]
        public void CanIntroduceEnumToAssembly()
        {
            using (var resolver = new DefaultAssemblyResolver())
            using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
            {
                TypeDefinition sourceType;
                using (var sourceAssembly = resolver.Resolve(AssemblyNameReference.Parse(typeof(Status).Assembly.FullName)))
                {
                    sourceType = sourceAssembly.MainModule.GetType(typeof(Status).FullName);
                    var loom = new Loom();
                    var graphGetter = new CilGraphGetter();

                    using (var introduction = new TypeIntroduction(graphGetter, sourceType, "Cilador.TestAopTarget"))
                    {
                        loom.WeavableConcepts.Add(new WeavableConcept<ModuleDefinition>(new PointCut<ModuleDefinition>(m => m.IsMain), introduction));
                        loom.Weave(targetAssembly);
                    }
                }

                targetAssembly.Write();
            }
        }
    }
}
