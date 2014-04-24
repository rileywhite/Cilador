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

using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal abstract class MemberClonerBase<TMemberDefinition, TMemberWithRoot> : IMemberCloner
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
        where TMemberWithRoot : MemberSourceWithRootBase<TMemberDefinition>
    {
        public MemberClonerBase(TMemberDefinition target, TMemberWithRoot sourceWithRoot)
        {
            Contract.Requires(target != null);
            Contract.Requires(sourceWithRoot != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.SourceWithRoot != null);

            if (sourceWithRoot.Source.IsSkipped())
            {
                throw new InvalidOperationException("Cannot clone a skipped member");
            }

            this.Target = target;
            this.SourceWithRoot = sourceWithRoot;
        }

        public TMemberDefinition Target { get; private set; }

        public TMemberWithRoot SourceWithRoot { get; private set; }

        public bool IsStructureCloned { get; protected set; }

        public abstract void CloneStructure();
    }
}
