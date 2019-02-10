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

using Cilador.Clone;
using NUnit.Framework;
using System;

namespace Cilador.Tests.CloneTests
{
    /// <summary>
    /// Tests <see cref="ClonerBase{TCloned}"/>.
    /// </summary>
    [TestFixture]
    internal class ClonerBaseFixture
    {
        /// <summary>
        /// Used in the create method of a fake cloner
        /// </summary>
        private static readonly object FakeTarget = new object();

        /// <summary>
        /// Fake cloner for testing
        /// </summary>
        private class SomethingCloner : ClonerBase<object>
        {
            public SomethingCloner(ICloningContext cloningContext, object source)
                : base(cloningContext, source) { }

            public int CreateCallCount { get; set; }

            protected override object GetTarget()
            {
                ++this.CreateCallCount;
                return ClonerBaseFixture.FakeTarget;
            }

            protected override void DoClone()
            {
            }
        }

        /// <summary>
        /// Fake cloner for testing that will fail on the create step
        /// </summary>
        private class BadCloner : ClonerBase<object>
        {
            public BadCloner(ICloningContext cloningContext, object source)
                : base(cloningContext, source) { }

            protected override object GetTarget()
            {
                return null;
            }

            protected override void DoClone()
            {
            }
        }

        /// <summary>
        /// Tests that the target is created on access.
        /// </summary>
        [Test]
        public void CreatesTargetTest()
        {
            var source = new object();

            var cloner = new SomethingCloner(
                new FakeCloningConext(),
                source);

            Assert.AreSame(source, cloner.Source);

            Assert.IsFalse(cloner.IsTargetSet);
            Assert.AreEqual(0, cloner.CreateCallCount);

            var target = cloner.Target;
            Assert.IsTrue(cloner.IsTargetSet);
            Assert.AreEqual(1, cloner.CreateCallCount);

            Assert.IsNotNull(target);
            Assert.AreSame(target, cloner.Target);

            target = cloner.Target; // third call, at this point
            Assert.AreEqual(1, cloner.CreateCallCount); // should only have been invoked once
        }

        /// <summary>
        /// Tests that a cloner that returns null on create will cause an error.
        /// </summary>
        [Test]
        public void MustCreateTargetTest()
        {
            var source = new object();
            var cloner = new BadCloner(new FakeCloningConext(), source);
            Assert.Catch(() => { var t = cloner.Target; });
        }
    }
}
