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

using Cilador.Core;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Cilador.Tests.CoreTests
{
    [TestFixture]
    internal class CoreExtensionsFixture
    {
        [Test]
        public void AreAnyNullTest()
        {
            var items = new List<object>();
            Assert.IsFalse(items.AreAnyNull());

            items.Add(null);
            Assert.IsTrue(items.AreAnyNull());

            items.Clear();
            Assert.IsFalse(items.AreAnyNull());

            items.Add(new object());
            items.Add(new object());
            items.Add(new object());
            items.Add(new object());
            items.Add(new object());
            Assert.IsFalse(items.AreAnyNull());

            items[2] = null;
            Assert.IsTrue(items.AreAnyNull());
        }
    }
}
