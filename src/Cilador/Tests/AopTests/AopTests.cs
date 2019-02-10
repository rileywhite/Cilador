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

namespace Cilador.Tests
{
    [TestFixture]
    public class AopTests
    {
        [Test]
        public void CanDecorateActionMethodWithReplacement()
        {
            using (var resolver = new DefaultAssemblyResolver())
            using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
            {
                var loom = new Loom();
                var graphGetter = new CilGraphGetter();

                loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(
                    new PointCut<MethodDefinition>(m => $"{m.DeclaringType.FullName}.{m.Name}" == "Cilador.TestAopTarget.Program.Run"),
                    new ActionDecorator<string[]>(
                        resolver,
                        graphGetter,
                        args =>
                        {
                            Console.WriteLine("Before...");
                            Console.WriteLine("---Args---");
                            foreach (var arg in args)
                            {
                                Console.WriteLine(arg);
                            }
                            Console.WriteLine("----------");
                            Forwarders.ForwardToOriginalAction(args);
                            Console.WriteLine("...After");
                        })));

                loom.Weave(targetAssembly);

                targetAssembly.Write();
            }
        }

        [Test]
        public void CanDecorateActionMethodWithReplacementAndArgAutoForwarding()
        {
            using (var resolver = new DefaultAssemblyResolver())
            using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
            {
                var loom = new Loom();
                var graphGetter = new CilGraphGetter();

                loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(
                    new PointCut<MethodDefinition>(m => $"{m.DeclaringType.FullName}.{m.Name}".StartsWith("Cilador.TestAopTarget.Program.RunAutoForwarding")),
                    new ActionDecorator(
                        resolver,
                        graphGetter,
                        () =>
                        {
                            Console.WriteLine("Before...");
                            Forwarders.ForwardToOriginalAction();
                            Console.WriteLine("...After");
                        })));

                loom.Weave(targetAssembly);

                targetAssembly.Write();
            }
        }

        [Test]
        public void CanDecorateActionMethodWithoutReplacement()
        {
            using (var resolver = new DefaultAssemblyResolver())
            using (var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true }))
            {
                var loom = new Loom();
                var graphGetter = new CilGraphGetter();

                loom.WeavableConcepts.Add(new WeavableConcept<MethodDefinition>(
                    new PointCut<MethodDefinition>(m => $"{m.DeclaringType.FullName}.{m.Name}" == "Cilador.TestAopTarget.Program.RunAgain"),
                    new ActionDecorator<string>(
                        resolver,
                        graphGetter,
                        delimitedArgs =>
                        {
                            Console.WriteLine("Before...");
                            var x = 2;
                            Forwarders.ForwardToOriginalAction(delimitedArgs.Split(new char[] {  ' ' }));
                            Console.WriteLine("...After");
                            Console.WriteLine(x);
                        },
                        name => $"{name}_Wrapper")));

                loom.Weave(targetAssembly);

                targetAssembly.Write();
            }
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
