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

using System;
using Bix.Mixers.ILCloning;
using Mono.Cecil;
using NUnit.Framework;

namespace Bix.Mixers.Tests.ILCloningTests
{
    /// <summary>
    /// Tests <see cref="LazyClonerBase{T}"/>.
    /// </summary>
    [TestFixture]
    internal class LazyClonerBaseFixture
    {
        /// <summary>
        /// Used in the create method of a fake cloner
        /// </summary>
        private static readonly object FakeTarget = new object();

        /// <summary>
        /// Fake cloner for testing
        /// </summary>
        private class SomethingCloner : LazyClonerBase<object>
        {
            public SomethingCloner(IILCloningContext ilCloningContext, object source, Func<object> targetGetter, Action<object> targetSetter)
                : base(ilCloningContext, source, targetGetter, targetSetter) { }

            protected override object CreateTarget()
            {
                return LazyClonerBaseFixture.FakeTarget;
            }

            public override void Clone()
            {
                this.IsCloned = true;
            }
        }

        /// <summary>
        /// Fake cloner for testing that will fail on the create step
        /// </summary>
        private class BadCloner : LazyClonerBase<object>
        {
            public BadCloner(IILCloningContext ilCloningContext, object source, Func<object> targetGetter, Action<object> targetSetter)
                : base(ilCloningContext, source, targetGetter, targetSetter) { }

            protected override object CreateTarget()
            {
                return null;
            }

            public override void Clone()
            {
                this.IsCloned = true;
            }
        }

        /// <summary>
        /// Fake IL cloning context.
        /// </summary>
        private class FakeILCloningConext : IILCloningContext
        {
            public TypeDefinition RootSource
            {
                get { throw new NotSupportedException(); }
            }

            public TypeDefinition RootTarget
            {
                get { throw new NotSupportedException(); }
            }

            public TItem DynamicRootImport<TItem>(TItem item)
            {
                throw new NotSupportedException();
            }

            public TypeReference RootImport(TypeReference type)
            {
                throw new NotSupportedException();
            }

            public MethodReference RootImport(MethodReference method)
            {
                throw new NotSupportedException();
            }

            public FieldReference RootImport(FieldReference field)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Tests that the target is created on access.
        /// </summary>
        [Test]
        public void CreatesTargetTest()
        {
            var source = new object();
            object target = null;

            var hasGetBeenCalled = false;
            var hasSetBeenCalled = false;

            var cloner = new SomethingCloner(
                new FakeILCloningConext(),
                source, () => { hasGetBeenCalled = true; return target; },
                o => { hasSetBeenCalled = true; target = o; });

            Assert.IsNull(target);
            Assert.IsFalse(hasGetBeenCalled);
            Assert.IsNull(cloner.Item2.Getter());
            Assert.IsTrue(hasGetBeenCalled);
            Assert.IsFalse(hasSetBeenCalled);
            Assert.AreSame(LazyClonerBaseFixture.FakeTarget, cloner.Target);
            Assert.IsNotNull(target);
            Assert.AreSame(LazyClonerBaseFixture.FakeTarget, target);
            Assert.AreSame(LazyClonerBaseFixture.FakeTarget, cloner.Item2.Getter());
            Assert.IsTrue(hasSetBeenCalled);
        }

        /// <summary>
        /// Tests that a cloner that returns null on create will cause an error.
        /// </summary>
        [Test]
        public void MustCreateTargetTest()
        {
            var source = new object();
            object target = null;

            var hasGetBeenCalled = false;
            var hasSetBeenCalled = false;

            var cloner = new BadCloner(
                new FakeILCloningConext(),
                source, () => { hasGetBeenCalled = true; return target; },
                o => { hasSetBeenCalled = true; target = o; });

            Assert.IsFalse(hasGetBeenCalled);
            Assert.IsFalse(hasSetBeenCalled);
            Assert.Catch(() => { var t = cloner.Target; });
            Assert.IsFalse(hasGetBeenCalled);
            Assert.IsFalse(hasSetBeenCalled);
        }
    }
}
