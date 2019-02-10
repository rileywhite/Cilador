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

                loom.WeavableConcepts.Add(RunWithoutReplacementDecorationConcept(resolver, graphGetter));

                loom.Weave(targetAssembly);

                targetAssembly.Write();
            }
        }

        #region Concepts

        private static WeavableConcept<MethodDefinition> RunWithoutReplacementDecorationConcept(DefaultAssemblyResolver resolver, CilGraphGetter graphGetter)
        {
            return new WeavableConcept<MethodDefinition>(
                new PointCut<MethodDefinition>(m => m.Name == "RunWithoutReplacement"),
                new ActionDecorator<string[]>(
                    resolver,
                    graphGetter,
                    RunWithoutReplacementDecoration,
                    name => $"{name}_Decorated"));
        }

        private static void RunWithoutReplacementDecoration(string[] args)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var thingsThatHaveRun = (Dictionary<string, object[]>)currentType
                .GetProperty("ThingsThatHaveRun", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null);

            thingsThatHaveRun.Add($"{currentType.Name}.RunWithoutReplacement_Decorated", args);

            Forwarders.ForwardToOriginalAction(string.Join(", ", args));
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
                $"{nameof(TestAopTarget.InstanceTarget)}.RunWithoutReplacement_Decorated",
                out var decoratedMethodArgs));
            Assert.AreSame(sentArgs, decoratedMethodArgs);

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"{nameof(TestAopTarget.InstanceTarget)}.RunWithoutReplacement",
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
                $"{nameof(TestAopTarget.StaticTarget)}.RunWithoutReplacement_Decorated",
                out var decoratedMethodArgs));
            Assert.AreSame(sentArgs, decoratedMethodArgs);

            Assert.IsTrue(thingsThatHaveRun.TryGetValue(
                $"{nameof(TestAopTarget.StaticTarget)}.RunWithoutReplacement",
                out var forwardedArgs));

            Assert.NotNull(forwardedArgs);
            Assert.That(forwardedArgs.Length == 1);
            Assert.AreEqual("1, 2, 3", forwardedArgs[0]);
        }

        //[Test]
        //public void CanDecorateInstanceActionMethodWithReplacement()
        //{
        //    using (var resolver = new DefaultAssemblyResolver())
        //    using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
        //    {
        //        var loom = new Loom();
        //        var graphGetter = new CilGraphGetter();

        //        loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(
        //            new PointCut<MethodDefinition>(m => $"{m.DeclaringType.FullName}.{m.Name}" == "Cilador.TestAopTarget.Program.Run"),
        //            new ActionDecorator<string[]>(
        //                resolver,
        //                graphGetter,
        //                args =>
        //                {
        //                    Console.WriteLine("Before...");
        //                    Console.WriteLine("---Args---");
        //                    foreach (var arg in args)
        //                    {
        //                        Console.WriteLine(arg);
        //                    }
        //                    Console.WriteLine("----------");
        //                    Forwarders.ForwardToOriginalAction(args);
        //                    Console.WriteLine("...After");
        //                })));

        //        loom.Weave(targetAssembly);

        //        targetAssembly.Write();
        //    }
        //}

        //[Test]
        //public void CanDecorateInstanceActionMethodWithReplacementAndArgAutoForwarding()
        //{
        //    using (var resolver = new DefaultAssemblyResolver())
        //    using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
        //    {
        //        var loom = new Loom();
        //        var graphGetter = new CilGraphGetter();

        //        loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(
        //            new PointCut<MethodDefinition>(m => $"{m.DeclaringType.FullName}.{m.Name}".StartsWith("Cilador.TestAopTarget.Program.RunAutoForwarding")),
        //            new ActionDecorator(
        //                resolver,
        //                graphGetter,
        //                () =>
        //                {
        //                    Console.WriteLine("Before...");
        //                    Forwarders.ForwardToOriginalAction();
        //                    Console.WriteLine("...After");
        //                })));

        //        loom.Weave(targetAssembly);

        //        targetAssembly.Write();
        //    }
        //}

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

                    loom.WeavableConcepts.Add(new WeavableConcept<ModuleDefinition>(
                        new PointCut<ModuleDefinition>(m => m.IsMain),
                        new TypeIntroduction(
                            graphGetter,
                            sourceType,
                            "Cilador.TestAopTarget")));

                    loom.Weave(targetAssembly);
                }

                targetAssembly.Write();
            }
        }
    }
}
