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
    /// <summary>
    /// Tests <see cref="ClonerBase{TClonedItem}"/>. Since it inherits from <see cref="LazyClonerBase{TClonedItem}"/>,
    /// only the added functionality is tested.
    /// </summary>
    [TestFixture]
    internal class ClonerBaseFixture
    {
        /// <summary>
        /// Used for testing the cloner functionality.
        /// </summary>
        private class SomethingCloner : ClonerBase<object>
        {
            public SomethingCloner(IILCloningContext ilCloningContext, object source, object target)
                : base(ilCloningContext, source, target) { }

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
        /// Tests the the target is populated immediately.
        /// </summary>
        public void ActualTargetTest()
        {
            var source = new object();
            var target = new object();

            var cloner = new SomethingCloner(new FakeILCloningConext(), source, target);

            Assert.AreSame(target, cloner.Item2.Getter());
            Assert.AreSame(target, cloner.Target);
        }
    }
}
