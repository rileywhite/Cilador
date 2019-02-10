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
    /// Clones <see cref="TypeDefinition"/> contents from a source to a target.
    /// </summary>
    /// <remarks>
    /// This is used by nested types. The root cloned type should use
    /// a <see cref="MergedRootTypeCloner"/>.
    /// </remarks>
    internal class NestedTypeCloner : ClonerBase<TypeDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="NestedTypeCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the parent type of the nested type being cloned.</param>
        /// <param name="source">Cloning source.</param>
        public NestedTypeCloner(ICloner<TypeDefinition> parent, TypeDefinition source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
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
            Contract.Assert(this.Target != this.CloningContext.RootTarget);
            Contract.Assert(this.Target.IsNested);
            Contract.Assert(this.Source.IsNested);

            if (this.Source.HasSecurityDeclarations)
            {
                // TODO Nested type security declarations
                throw new InvalidOperationException(string.Format(
                    "Cloning source type may not contain nested types annotated with security attributes: [{0}]",
                    this.CloningContext.RootSource.FullName));
            }

            this.Target.Attributes = this.Source.Attributes;
            this.Target.DeclaringType = this.CloningContext.RootImport(this.Source.DeclaringType).Resolve();
            this.Target.BaseType = this.CloningContext.RootImport(this.Source.BaseType);

            foreach (var interfaceImplementation in this.Source.Interfaces)
            {
                var iiClone = new InterfaceImplementation(this.CloningContext.RootImport(interfaceImplementation.InterfaceType));
                if (iiClone.HasCustomAttributes)
                {
                    // TODO support custom attributes on an interface implementation (new in Mono.Cecil 0.10.0)
                    // (should fit nicely with cloning operations, but it will take a bit of time)
                    throw new NotSupportedException("Cannont clone an interface implementation with custom attributes");
                }
                this.Target.Interfaces.Add(iiClone);
            }

            // TODO look more closely at type packing size
            this.Target.PackingSize = this.Source.PackingSize;

            // TODO look more closely at type class size
            this.Target.ClassSize = this.Source.ClassSize;
        }
    }
}
