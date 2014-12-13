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
        public void CloneTest()
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
    }
}
