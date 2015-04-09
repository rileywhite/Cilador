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
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones <see cref="TypeDefinition"/> contents from a source to a target.
    /// </summary>
    /// <remarks>
    /// This is used by nested types. The root cloned type should use
    /// a <see cref="RootTypeCloner"/>.
    /// </remarks>
    internal class TypeCloner : ClonerBase<TypeDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="TypeCloner"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        public TypeCloner(ClonerBase<TypeDefinition> parentTypeCloner, TypeDefinition source)
            : base(parentTypeCloner.ILCloningContext, source)
        {
            Contract.Requires(parentTypeCloner != null);
            Contract.Requires(parentTypeCloner.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.ParentTypeCloner != null);

            this.ParentTypeCloner = parentTypeCloner;
        }

        /// <summary>
        /// Gets or sets the cloner for the parent type.
        /// All types should have parent types.
        /// </summary>
        private ClonerBase<TypeDefinition> ParentTypeCloner { get; set; }

        /// <summary>
        /// Creates the new target.
        /// </summary>
        /// <returns>Newly created target.</returns>
        protected override TypeDefinition CreateTarget()
        {
            var target = new TypeDefinition(this.Source.Namespace, this.Source.Name, 0);
            this.ParentTypeCloner.Target.NestedTypes.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the type.
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Target != this.ILCloningContext.RootTarget);
            Contract.Assert(this.Target.IsNested);
            Contract.Assert(this.Source.IsNested);

            if (this.Source.HasSecurityDeclarations)
            {
                // TODO Nested type security declarations
                throw new InvalidOperationException(string.Format(
                    "Configured mixin implementation may not contain nested types annotated with security attributes: [{0}]",
                    this.ILCloningContext.RootSource.FullName));
            }

            this.Target.Attributes = this.Source.Attributes;
            this.Target.DeclaringType = this.ILCloningContext.RootImport(this.Source.DeclaringType).Resolve();
            this.Target.BaseType = this.ILCloningContext.RootImport(this.Source.BaseType);

            foreach (var interfaceType in this.Source.Interfaces)
            {
                this.Target.Interfaces.Add(this.ILCloningContext.RootImport(interfaceType));
            }

            // TODO look more closely at type packing size
            this.Target.PackingSize = this.Source.PackingSize;

            // TODO look more closely at type class size
            this.Target.ClassSize = this.Source.ClassSize;

            this.Target.CloneAllCustomAttributes(this.Source, this.ILCloningContext);

            this.IsCloned = true;
        }
    }
}
