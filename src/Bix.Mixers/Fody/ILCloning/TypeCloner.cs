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
    internal class TypeCloner : MemberClonerBase<TypeDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="TypeCloner"/>.
        /// </summary>
        /// <param name="rootContext">Root context for cloning.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public TypeCloner(RootContext rootContext, TypeDefinition target, TypeDefinition source)
            : base(rootContext, target, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            this.TypeCloners = new List<TypeCloner>();
            this.FieldCloners = new List<FieldCloner>();
            this.MethodCloners = new List<MethodCloner>();
            this.PropertyCloners = new List<PropertyCloner>();
            this.EventCloners = new List<EventCloner>();
        }

        /// <summary>
        /// Gets or sets the collection of cloners for nested types
        /// </summary>
        private List<TypeCloner> TypeCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained fields
        /// </summary>
        private List<FieldCloner> FieldCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained methods
        /// </summary>
        private List<MethodCloner> MethodCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained properties
        /// </summary>
        private List<PropertyCloner> PropertyCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained events
        /// </summary>
        private List<EventCloner> EventCloners { get; set; }

        /// <summary>
        /// Gets or sets whether the initial creation of all members and their
        /// associated cloners has been created.
        /// </summary>
        private bool IsWireframeCompleted { get; set; }

        /// <summary>
        /// Clones the structure of this type but not the logic of method bodies
        /// </summary>
        public override void CloneStructure()
        {
            this.CreateWireframeAndCloners();
            this.CloneTypeData();
            this.TypeCloners.CloneStructure();
            this.FieldCloners.CloneStructure();
            this.MethodCloners.CloneStructure();
            this.PropertyCloners.CloneStructure();
            this.EventCloners.CloneStructure();
            this.IsStructureCloned = true;
        }

        /// <summary>
        /// Clones just the method body logic for this type's methods and for containing types methods
        /// </summary>
        public void CloneLogic()
        {
            foreach (var methodCloner in this.MethodCloners) { methodCloner.CloneLogic(); }
            foreach (var typeCloner in this.TypeCloners) { typeCloner.CloneLogic(); }
        }

        /// <summary>
        /// Clones just the properties of the type
        /// </summary>
        private void CloneTypeData()
        {
            if (this.Target == this.RootContext.RootTarget)
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

                if (this.Source.BaseType.Resolve() != this.Source.Module.Import(typeof(object)).Resolve())
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

                if(this.Source.HasGenericParameters)
                {
                    // TODO nested type generic parameters
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation may not include any open generic nested types: [{0}]",
                        this.RootContext.RootSource.FullName));
                }

                if (this.Source.HasSecurityDeclarations)
                {
                    // TODO Nested type security declarations
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation may not contain nested types annotated with security attributes: [{0}]",
                        this.RootContext.RootSource.FullName));
                }

                this.Target.Attributes = this.Source.Attributes;
                this.Target.DeclaringType = this.RootContext.RootImport(this.Source.DeclaringType).Resolve();
                this.Target.BaseType = this.RootContext.RootImport(this.Source.BaseType);

                foreach (var interfaceType in this.Source.Interfaces)
                {
                    this.Target.Interfaces.Add(this.RootContext.RootImport(interfaceType));
                }

                // TODO look more closely at type packing size
                this.Target.PackingSize = this.Source.PackingSize;

                // TODO look more closely at type class size
                this.Target.ClassSize = this.Source.ClassSize;

                // TODO research correct usage of type metadata token
                this.Target.MetadataToken = new MetadataToken(this.Source.MetadataToken.TokenType, this.Source.MetadataToken.RID);
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around, but only for non-root type
            if (this.Target != this.RootContext.RootTarget) { this.Target.CustomAttributes.Clear(); }
            this.Target.CloneAllCustomAttributes(this.Source, this.RootContext);
        }

        /// <summary>
        /// Creates all members of this and nested types and associated cloners with minimal properties set.
        /// </summary>
        private void CreateWireframeAndCloners()
        {
            if (!this.IsWireframeCompleted)
            {
                var voidReference = this.Target.Module.Import(typeof(void));

                foreach (var sourceType in this.Source.NestedTypes)
                {
                    var targetType = new TypeDefinition(sourceType.Namespace, sourceType.Name, 0);
                    this.Target.NestedTypes.Add(targetType);
                    var typeCloner = new TypeCloner(this.RootContext, targetType, sourceType);
                    typeCloner.CreateWireframeAndCloners();
                    this.TypeCloners.Add(typeCloner);
                }

                foreach (var sourceField in this.Source.Fields)
                {
                    var targetField = new FieldDefinition(sourceField.Name, 0, voidReference);
                    this.Target.Fields.Add(targetField);
                    this.FieldCloners.Add(new FieldCloner(this.RootContext, targetField, sourceField));
                }

                foreach (var sourceMethod in this.Source.Methods)
                {
                    if (sourceMethod.Name == ".cctor" &&
                        sourceMethod.IsStatic &&
                        sourceMethod.DeclaringType == RootContext.RootSource)
                    {
                        // TODO should static constructors be supported on the root type?
                        throw new WeavingException(string.Format(
                            "Configured mixin implementation cannot have a type initializer (i.e. static constructor): [{0}]",
                            this.RootContext.RootSource.FullName));
                    }

                    if (sourceMethod.IsConstructor &&
                        sourceMethod.DeclaringType == RootContext.RootSource)
                    {
                        // TODO support constructors for the root type in some meaningful way
                        if (sourceMethod.HasParameters) 
                        {
                            throw new WeavingException(string.Format(
                                "Configured mixin implementation cannot use constructors: [{0}]",
                                this.RootContext.RootSource.FullName));
                        }
                        continue;
                    }

                    var targetMethod = new MethodDefinition(sourceMethod.Name, 0, voidReference);
                    this.Target.Methods.Add(targetMethod);
                    this.MethodCloners.Add(new MethodCloner(this.RootContext, targetMethod, sourceMethod));
                }

                foreach (var sourceProperty in this.Source.Properties)
                {
                    var targetProperty = new PropertyDefinition(sourceProperty.Name, 0, voidReference);
                    this.Target.Properties.Add(targetProperty);
                    this.PropertyCloners.Add(new PropertyCloner(this.RootContext, targetProperty, sourceProperty));
                }

                foreach (var sourceEvent in this.Source.Events)
                {
                    var targetEvent = new EventDefinition(sourceEvent.Name, 0, voidReference);
                    this.Target.Events.Add(targetEvent);
                    this.EventCloners.Add(new EventCloner(this.RootContext, targetEvent, sourceEvent));
                }
            }
            this.IsWireframeCompleted = true;
        }
    }
}
