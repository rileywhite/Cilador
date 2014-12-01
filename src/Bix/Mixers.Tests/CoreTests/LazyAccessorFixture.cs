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

using Bix.Mixers.Core;
using NUnit.Framework;
using System;

namespace Bix.Mixers.Tests.CoreTests
{
    /// <summary>
    /// Tests <see cref="LazyAccessor"/>, which is used for lazy cloning.
    /// </summary>
    [TestFixture]
    internal class LazyAccessorFixture
    {
        /// <summary>
        /// Tests a get-only accessor.
        /// </summary>
        [Test]
        public void GetAccessorTest()
        {
            int localInt = 55;
            var accessor = new LazyAccessor<int>(getter: () => localInt);

            Assert.IsTrue(accessor.IsGetAccessor);
            Assert.IsFalse(accessor.IsSetAccessor);

            Assert.AreEqual(55, accessor.Getter());
            localInt = 483;
            Assert.AreEqual(483, accessor.Getter());

            Assert.Catch(() => accessor.Setter(23));
        }

        /// <summary>
        /// Tests a set-only accessor.
        /// </summary>
        [Test]
        public void SetAccessorTest()
        {
            int localInt = 0;
            var accessor = new LazyAccessor<int>(setter: i => localInt = i);

            Assert.IsFalse(accessor.IsGetAccessor);
            Assert.IsTrue(accessor.IsSetAccessor);

            Assert.AreEqual(0, localInt);
            accessor.Setter(23);
            Assert.AreEqual(23, localInt);

            Assert.Catch(() => accessor.Getter());
        }

        /// <summary>
        /// Tests a get/set accessor.
        /// </summary>
        [Test]
        public void GetSetAccessorTest()
        {
            int localInt = 533;
            var accessor = new LazyAccessor<int>(
                getter: () => localInt,
                setter: i => localInt = i);

            Assert.IsTrue(accessor.IsGetAccessor);
            Assert.IsTrue(accessor.IsSetAccessor);

            Assert.AreEqual(533, accessor.Getter());
            accessor.Setter(8974);
            Assert.AreEqual(8974, localInt);
            Assert.AreEqual(8974, accessor.Getter());
        }
    }
}
