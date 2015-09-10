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
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Clones <see cref="TypeDefinition"/> contents from a source to a target.
    /// </summary>
    /// <remarks>
    /// This is used by nested types. The root cloned type should use
    /// a <see cref="RootTypeCloner"/>.
    /// </remarks>
    internal class NestedTypeCloner : ClonerBase<TypeDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="NestedTypeCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the parent type of the nested type being cloned.</param>
        /// <param name="source">Cloning source.</param>
        public NestedTypeCloner(ICloner<TypeDefinition> parent, TypeDefinition source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the cloner for the parent type.
        /// All types should have parent types.
        /// </summary>
        private ICloner<TypeDefinition> Parent { get; set; }

        /// <summary>
        /// Creates the new target.
        /// </summary>
        /// <returns>Newly created target.</returns>
        protected override TypeDefinition GetTarget()
        {
            var target = new TypeDefinition(this.Source.Namespace, this.Source.Name, 0);
            this.Parent.Target.NestedTypes.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the type.
        /// </summary>
        protected override void DoClone()
        {
            Contract.Assert(this.Target != this.ILCloningContext.RootTarget);
            Contract.Assert(this.Target.IsNested);
            Contract.Assert(this.Source.IsNested);

            if (this.Source.HasSecurityDeclarations)
            {
                // TODO Nested type security declarations
                throw new InvalidOperationException(string.Format(
                    "Cloning source type may not contain nested types annotated with security attributes: [{0}]",
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
        }
    }
}
