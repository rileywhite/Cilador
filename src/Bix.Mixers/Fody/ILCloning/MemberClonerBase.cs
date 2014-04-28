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
    /// <summary>
    /// Base type for all cloners that clone member items.
    /// </summary>
    /// <typeparam name="TMemberDefinition">Resolved type of the member.</typeparam>
    /// <typeparam name="TMemberWithRoot">Type of the source member with root information to be used for cloning.</typeparam>
    internal abstract class MemberClonerBase<TMemberDefinition, TMemberWithRoot> : IMemberCloner
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
        where TMemberWithRoot : MemberSourceWithRootBase<TMemberDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="MemberClonerBase<,>"/>
        /// </summary>
        /// <param name="target">Resolved type of the cloning target.</param>
        /// <param name="sourceWithRoot">Resolved source with root information for the cloning source.</param>
        public MemberClonerBase(TMemberDefinition target, TMemberWithRoot sourceWithRoot)
        {
            Contract.Requires(target != null);
            Contract.Requires(sourceWithRoot != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.SourceWithRoot != null);

            this.Target = target;
            this.SourceWithRoot = sourceWithRoot;
        }

        /// <summary>
        /// Gets or sets the resolved type of the cloning target.
        /// </summary>
        public TMemberDefinition Target { get; private set; }

        /// <summary>
        /// Gets or sets the resolved source with root information for the cloning source.
        /// </summary>
        public TMemberWithRoot SourceWithRoot { get; private set; }

        /// <summary>
        /// Gets or sets whether the structure has been cloned.
        /// </summary>
        public bool IsStructureCloned { get; protected set; }

        /// <summary>
        /// Clones the structure from the source to the target.
        /// </summary>
        public abstract void CloneStructure();
    }
}
