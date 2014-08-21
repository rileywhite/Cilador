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

using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Clones <see cref="TypeDefinition"/> contents from a source to a target.
    /// </summary>
    internal class TypeCloner : ClonerBase<TypeDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="TypeCloner"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public TypeCloner(ILCloningContext ilCloningContext, TypeDefinition target, TypeDefinition source)
            : base(ilCloningContext, target, source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        /// <summary>
        /// Clones the type.
        /// </summary>
        public override void Clone()
        {
            if (this.Target == this.ILCloningContext.RootTarget)
            {
                if (!(this.Source.IsClass && !this.Source.IsValueType))
                {
                    throw new WeavingException(string.Format("Configured mixin implementation type must be a reference type: [{0}]", this.Source.FullName));
                }

                if (this.Source.IsAbstract)
                {
                    throw new WeavingException(string.Format("Configured mixin implementation type cannot be abstract: [{0}]", this.Source.FullName));
                }

                if (this.Source.HasGenericParameters)
                {
                    throw new WeavingException(string.Format("Configured mixin implementation type cannot be an open generic type: [{0}]", this.Source.FullName));
                }

                if (this.Source.HasSecurityDeclarations)
                {
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation may not be annotated with security attributes: [{0}]",
                        this.Source.FullName));
                }

                if (this.Source.BaseType.Resolve().FullName != this.Source.Module.Import(typeof(object)).Resolve().FullName)
                {
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation cannot have a base type other than System.Object: [{0}]",
                        this.Source.FullName));
                }
            }
            else
            {
                Contract.Assert(this.Target.IsNested);
                Contract.Assert(this.Source.IsNested);

                if (this.Source.HasSecurityDeclarations)
                {
                    // TODO Nested type security declarations
                    throw new WeavingException(string.Format(
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
            }

            this.Target.CloneAllCustomAttributes(this.Source, this.ILCloningContext);

            this.IsCloned = true;
        }
    }
}
