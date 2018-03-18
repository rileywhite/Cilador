/***************************************************************************/
// Copyright 2013-2017 Riley White
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

using Mono.Cecil;
using NUnit.Framework;
using System;

namespace Cilador.Core
{
    [TestFixture]
    public class AddAdviceToMethod
    {
        [Test]
        public void Test()
        {
            var resolver = new DefaultAssemblyResolver();
            var targetAssembly = resolver.Resolve("Cilador.TestAopTarget");
            var loom = new Loom();

            loom.Aspects.Add(Tuple.Create<Func<MethodDefinition, bool>, ActionAdvice<string[]>>(
                m => $"{m.DeclaringType.FullName}.{m.Name}" == "Cilador.TestAopTarget.Program.Run",
                arg =>
                {
                    Console.WriteLine("Before...");
                    AdviceForwarder.ForwardToOriginalAction(arg);
                    Console.WriteLine("...After");
                }));

            loom.Weave(targetAssembly);

            targetAssembly.Write("Cilador.TestAopTarget.Modified.exe", new WriterParameters { WriteSymbols = true });
        }
    }
}
