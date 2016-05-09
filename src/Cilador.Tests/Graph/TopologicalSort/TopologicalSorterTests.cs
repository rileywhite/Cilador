/***************************************************************************/
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

using Cilador.Graph.TopologicalSort;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Cilador.Tests.Graph.TopologicalSort
{
    [TestFixture]
    public class TopologicalSorterTests
    {
        private List<string> RunTest(int numNodes, params string[] edges)
        {
            var result = TopologicalSorter.FindAndTopologicallySortStronglyConnectedComponents(Enumerable.Range('a', numNodes).Select(c => ((char)c)), edges.Select(e => Edge.Create(e[0], e[1])));
            return result.Select(x => new string(x.ToArray())).ToList();
        }

        [Test]
        public void TestAlgorithm()
        {
            var result = RunTest(12, "ab", "bc", "be", "bf", "cd", "cg", "dc", "dh", "ea", "ef", "fg", "gf", "hd", "hg", "ii", "kl");
            Assert.That(result, Has.Count.EqualTo(7));
            var idx1 = result.FindIndex(s => s.Length == 3 && s.Contains("a") && s.Contains("b") && s.Contains("e"));
            var idx2 = result.FindIndex(s => s.Length == 3 && s.Contains("c") && s.Contains("d") && s.Contains("h"));
            var idx3 = result.FindIndex(s => s.Length == 2 && s.Contains("f") && s.Contains("g"));
            var idx4 = result.FindIndex(s => s.Length == 1 && s.Contains("i"));
            var idx5 = result.FindIndex(s => s.Length == 1 && s.Contains("j"));
            var idx6 = result.FindIndex(s => s.Length == 1 && s.Contains("k"));
            var idx7 = result.FindIndex(s => s.Length == 1 && s.Contains("l"));
            Assert.That(idx1, Is.GreaterThanOrEqualTo(0), "Component abe not found");
            Assert.That(idx2, Is.GreaterThanOrEqualTo(0), "Component cdh not found");
            Assert.That(idx3, Is.GreaterThanOrEqualTo(0), "Component fg not found");
            Assert.That(idx4, Is.GreaterThanOrEqualTo(0), "Component i not found");
            Assert.That(idx5, Is.GreaterThanOrEqualTo(0), "Component j not found");
            Assert.That(idx6, Is.GreaterThanOrEqualTo(0), "Component k not found");
            Assert.That(idx7, Is.GreaterThanOrEqualTo(0), "Component l not found");

            Assert.That(idx3, Is.LessThanOrEqualTo(idx1), "fg should come before abe");
            Assert.That(idx3, Is.LessThanOrEqualTo(idx2), "fg should come before cdh");
            Assert.That(idx2, Is.LessThanOrEqualTo(idx1), "cdh should come before abe");
            Assert.That(idx7, Is.LessThanOrEqualTo(idx6), "l should come before k");
        }
    }
}
