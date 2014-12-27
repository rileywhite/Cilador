/***************************************************************************/
// Copyright 2013-2014 Riley White
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

using Bix.Mixers.ILCloning;
using Mono.Cecil;
using NUnit.Framework;
using System;

namespace Bix.Mixers.Tests.ILCloningTests
{
    [TestFixture]
    internal class ILCloningExtensionsFixture
    {
        private class TestCloner : ICloner
        {
            public IILCloningContext ILCloningContext { get; set; }

            public bool IsCloned { get; set; }

            public void Clone()
            {
                this.IsCloned = true;
            }
        }

        [Test]
        public void CloneAllTest()
        {
            var cloners = new TestCloner[0];

            cloners.CloneAll();
            Assert.That(cloners, Is.Empty);

            cloners = new TestCloner[] { new TestCloner() };
            cloners.CloneAll();
            Assert.That(cloners, Has.Length.EqualTo(1));
            Assert.That(cloners, Has.All.Property("IsCloned").True);

            cloners = new TestCloner[] { new TestCloner(), new TestCloner(), new TestCloner() };
            cloners.CloneAll();
            Assert.That(cloners, Has.Length.EqualTo(3));
            Assert.That(cloners, Has.All.Property("IsCloned").True);
        }


        private class Nested
        {
            public class Level1_1
            {
                public class Level1_1_1
                {
                    public class Level1_1_1_1
                    {
                    }
                }
                public class Level1_1_2
                {
                }
            }

            public class Level1_2
            {
                public class Level1_2_1<T>
                {
                    public class Level1_2_1_1
                    {
                    }
                }
            }
        }

        [Test]
        public void IsNestedWithinTest()
        {
            var resolver = new DefaultAssemblyResolver();
            var currentAssembly = resolver.Resolve(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            Assert.That(currentAssembly, Is.Not.Null);
            var currentModule = currentAssembly.MainModule;
            Assert.That(currentModule, Is.Not.Null);

            var nestedType = currentModule.Import(typeof(Nested)).Resolve();
            var level1_1Type = currentModule.Import(typeof(Nested.Level1_1)).Resolve();
            var level1_1_1Type = currentModule.Import(typeof(Nested.Level1_1.Level1_1_1)).Resolve();
            var level1_1_1_1Type = currentModule.Import(typeof(Nested.Level1_1.Level1_1_1.Level1_1_1_1)).Resolve();
            var level1_1_2Type = currentModule.Import(typeof(Nested.Level1_1.Level1_1_2)).Resolve();
            var level1_2Type = currentModule.Import(typeof(Nested.Level1_2)).Resolve();
            var level1_2_1TType = currentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>)).Resolve();
            var level1_2_1TTypeReference = currentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>));
            var level1_2_1T_1Type = currentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>.Level1_2_1_1)).Resolve();
            var level1_2_1T_1TypeReference = currentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>.Level1_2_1_1));

            var level1_2_1IntTypeReference = currentModule.Import(typeof(Nested.Level1_2.Level1_2_1<int>));
            var level1_2_1Int_1TypeReference = currentModule.Import(typeof(Nested.Level1_2.Level1_2_1<int>.Level1_2_1_1));

            // not nested within self
            Assert.That(nestedType.IsNestedWithin(nestedType), Is.False);

            // direct nesting is correct
            Assert.That(level1_1Type.IsNestedWithin(nestedType), Is.True);
            Assert.That(nestedType.IsNestedWithin(level1_1Type), Is.False);

            // deep nesting is correct
            Assert.That(level1_1_1_1Type.IsNestedWithin(nestedType), Is.True);
            Assert.That(nestedType.IsNestedWithin(level1_1_1_1Type), Is.False);

            // not confused by sibling types
            Assert.That(level1_1Type.IsNestedWithin(level1_2Type), Is.False);
            Assert.That(level1_1_1Type.IsNestedWithin(level1_2Type), Is.False);
            Assert.That(level1_1_1_1Type.IsNestedWithin(level1_2Type), Is.False);
            Assert.That(level1_2Type.IsNestedWithin(level1_1Type), Is.False);
            Assert.That(level1_2_1TType.IsNestedWithin(level1_1Type), Is.False);
            Assert.That(level1_2_1T_1Type.IsNestedWithin(level1_1Type), Is.False);

            // works with generic types and open generic type references
            Assert.That(level1_2_1TType.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1TType.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1TTypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1TTypeReference.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1T_1Type.IsNestedWithin(level1_2_1TType), Is.True);
            Assert.That(level1_2_1T_1Type.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1T_1Type.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1T_1TypeReference.IsNestedWithin(level1_2_1TType), Is.True);
            Assert.That(level1_2_1T_1TypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1T_1TypeReference.IsNestedWithin(nestedType), Is.True);

            // works with closed generic type references
            Assert.That(level1_2_1IntTypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1IntTypeReference.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1Int_1TypeReference.IsNestedWithin(level1_2_1TType), Is.True);
            Assert.That(level1_2_1Int_1TypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1Int_1TypeReference.IsNestedWithin(nestedType), Is.True);
        }
    }
}
