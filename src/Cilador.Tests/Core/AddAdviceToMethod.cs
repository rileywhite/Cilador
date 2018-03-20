/***************************************************************************/
// Copyright 2013-2018 Riley White
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

using Cilador.Aop.Advisors.WrapMethod;
using Cilador.Aop.Core;
using Cilador.Graph.Factory;
using Mono.Cecil;
using NUnit.Framework;
using System;

namespace Cilador.Aop
{
    [TestFixture]
    public class AddAdviceToMethod
    {
        [Test]
        public void Test()
        {
            var resolver = new DefaultAssemblyResolver();
            var targetAssembly = resolver.Resolve(AssemblyNameReference.Parse("Cilador.TestAopTarget"), new ReaderParameters { ReadWrite = true });
            var loom = new Loom();
            var graphGetter = new CilGraphGetter();

            loom.Aspects.Add(new Aspect<MethodDefinition>(
                new PointCut<MethodDefinition>(m => $"{m.DeclaringType.FullName}.{m.Name}" == "Cilador.TestAopTarget.Program.Run"),
                new WrapMethodAdvisor<string>(
                    resolver,
                    graphGetter,
                    arg =>
                    {
                        Console.WriteLine("Before...");
                        AdviceForwarder.ForwardToOriginalAction(arg);
                        Console.WriteLine("...After");
                    })));

            loom.Weave(targetAssembly);

            targetAssembly.Write();
        }
    }
}
