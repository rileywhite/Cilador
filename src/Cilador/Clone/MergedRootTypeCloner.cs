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

using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Clone
{
    /// <summary>
    /// Clones <see cref="TypeDefinition"/> contents from a source to a target when the source is an existing type that
    /// should have the target's contents merged in.
    /// </summary>
    internal class MergedRootTypeCloner : ClonerBase<TypeDefinition>, ICloningRoot<TypeDefinition> 
    {
        /// <summary>
        /// Creates a new <see cref="MergedRootTypeCloner"/>.
        /// </summary>
        /// <param name="cloningContext">cloning context.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public MergedRootTypeCloner(ICloningContext cloningContext, TypeDefinition source, TypeDefinition target)
            : base(cloningContext, source)
        {
            Contract.Requires(cloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.Target != null);
            this.RootTarget = target;
        }

        /// <summary>
        /// Gets or sets the cloning target. In this case, the type is pre-existing.
        /// </summary>
        private TypeDefinition RootTarget { get; set; }

        /// <summary>
        /// In this special case, the type already exists. Simply return the existing type.
        /// </summary>
        /// <returns>Cloning target.</returns>
        protected override TypeDefinition GetTarget()
        {
            return this.RootTarget;
        }

        /// <summary>
        /// Clones the type.
        /// </summary>
        protected override void DoClone()
        {
            Contract.Assert(this.Target == this.CloningContext.RootTarget);

            if (!(this.Source.IsClass && !this.Source.IsValueType))
            {
                throw new InvalidOperationException(string.Format("Cloning root source type must be a reference type: [{0}]", this.Source.FullName));
            }

            if (this.Source.IsAbstract)
            {
                throw new InvalidOperationException(string.Format("Cloning root source type cannot be abstract: [{0}]", this.Source.FullName));
            }

            if (this.Source.HasGenericParameters)
            {
                throw new InvalidOperationException(string.Format("Cloning root source type cannot be an open generic type: [{0}]", this.Source.FullName));
            }

            if (this.Source.HasSecurityDeclarations)
            {
                throw new InvalidOperationException(string.Format(
                    "Cloning root source type may not be annotated with security attributes: [{0}]",
                    this.Source.FullName));
            }

            if (this.Source.BaseType.Resolve().FullName != this.Source.Module.ImportReference(typeof(object)).Resolve().FullName)
            {
                throw new InvalidOperationException(string.Format(
                    "Cloning root source type cannot have a base type other than System.Object: [{0}]",
                    this.Source.FullName));
            }
        }
    }
}
