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

using Cilador.Clone;
using Cilador.Graph.Factory;
using Cilador.Graph.Operations;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Linq;

namespace Cilador.Core
{
    [TestFixture]
    public class AddAdviceToMethod
    {
        [Test]
        public void Test()
        {
            ActionAdvice<string[]> advice = (action, arg) =>
            {
                Console.WriteLine("Before...");
                action(arg);
                Console.WriteLine("...After");
            };

            var resolver = new DefaultAssemblyResolver();

            var targetAssembly = resolver.Resolve("Cilador.TestAopTarget");
            var targetType = targetAssembly.MainModule.GetType("Cilador.TestAopTarget.Program");
            var targetMethod = targetType.Methods.Single(m => m.Name == "Run");

            var adviceAssembly = resolver.Resolve("Cilador.Tests");
            var adviceParentType = adviceAssembly.MainModule.GetType(this.GetType().FullName);
            var adviceMethod = adviceParentType.NestedTypes.SelectMany(t => t.Methods).Single(m => m.Name == advice.Method.Name);
            var adviceType = adviceMethod.DeclaringType;

            var graphGetter = new CilGraphGetter();

            targetMethod.Name = $"cilador_{Guid.NewGuid().ToString("N")}";

            var adviceGraph = graphGetter.Get(adviceMethod);

            var cloningContext = new CloningContext(adviceGraph, adviceMethod.DeclaringType, targetType);
            cloningContext.TargetTransforms.Add(adviceMethod, t => { ((MethodDefinition)t).Name = "Run"; });
            cloningContext.Execute();

            targetType.CustomAttributes.Clear();

            targetAssembly.Write("Cilador.TestAopTarget.Modified.exe", new WriterParameters { WriteSymbols = true });
        }
    }
}
