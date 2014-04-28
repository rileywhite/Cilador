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
    internal abstract class MemberClonerBase<TMemberDefinition> : IMemberCloner
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        /// <summary>
        /// Creates a new <see cref="MemberClonerBase<,>"/>
        /// </summary>
        /// <param name="rootContext">Root context for cloning.</param>
        /// <param name="target">Resolved cloning target.</param>
        /// <param name="sourceWithRoot">Resolved cloning source.</param>
        public MemberClonerBase(RootContext rootContext, TMemberDefinition target, TMemberDefinition source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.RootContext != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Source != null);

            this.RootContext = rootContext;
            this.Target = target;
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the resolved cloning target.
        /// </summary>
        public RootContext RootContext { get; private set; }

        /// <summary>
        /// Gets or sets the resolved cloning target.
        /// </summary>
        public TMemberDefinition Target { get; private set; }

        /// <summary>
        /// Gets or sets the resolved cloning source.
        /// </summary>
        public TMemberDefinition Source { get; private set; }

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
