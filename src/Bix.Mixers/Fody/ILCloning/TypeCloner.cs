using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class TypeCloner : MemberClonerBase<TypeDefinition, TypeSourceWithRoot>
    {
        public TypeCloner(TypeDefinition target, TypeSourceWithRoot source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            this.TypeCloners = new List<TypeCloner>();
            this.FieldCloners = new List<FieldCloner>();
            this.MethodCloners = new List<MethodCloner>();
            this.PropertyCloners = new List<PropertyCloner>();
            this.EventCloners = new List<EventCloner>();
        }

        private List<TypeCloner> TypeCloners { get; set; }
        private List<FieldCloner> FieldCloners { get; set; }
        private List<MethodCloner> MethodCloners { get; set; }
        private List<PropertyCloner> PropertyCloners { get; set; }
        private List<EventCloner> EventCloners { get; set; }

        private bool IsWireframeCompleted { get; set; }

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

        public void CloneLogic()
        {
            foreach (var methodCloner in this.MethodCloners) { methodCloner.CloneLogic(); }
            foreach (var typeCloner in this.TypeCloners) { typeCloner.CloneLogic(); }
        }

        private void CloneTypeData()
        {
            if (this.Target == this.SourceWithRoot.RootContext.RootTarget)
            {
                if (this.SourceWithRoot.Source.IsAbstract)
                {
                    throw new WeavingException(string.Format("Mixin source type cannot be abstract: [{0}]", this.SourceWithRoot.Source.FullName));
                }

                if (this.SourceWithRoot.Source.HasGenericParameters)
                {
                    throw new WeavingException(string.Format("Mixin source type cannot be an open generic type: [{0}]", this.SourceWithRoot.Source.FullName));
                }

                if (this.SourceWithRoot.Source.HasSecurityDeclarations)
                {
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation may not be annotated with security attributes: [{0}]",
                        this.SourceWithRoot.Source.FullName));
                }

                if (!(this.SourceWithRoot.Source.IsClass && !this.SourceWithRoot.Source.IsValueType))
                {
                    throw new WeavingException(string.Format("Mixin source type must be a reference type: [{0}]", this.SourceWithRoot.Source.FullName));
                }

                if (this.SourceWithRoot.Source.BaseType.Resolve() != this.SourceWithRoot.Source.Module.Import(typeof(object)).Resolve())
                {
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation cannot have a base type other than System.Object: [{0}]",
                        this.SourceWithRoot.Source.FullName));
                }
            }
            else
            {
                Contract.Assert(this.Target.IsNested);
                Contract.Assert(this.SourceWithRoot.Source.IsNested);

                if(this.SourceWithRoot.Source.HasGenericParameters)
                {
                    // TODO nested type generic parameters
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation may not include any open generic nested types: [{0}]",
                        this.SourceWithRoot.Source.FullName));
                }

                if (this.SourceWithRoot.Source.HasSecurityDeclarations)
                {
                    // TODO Nested type security declarations
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation may not contain nested types annotated with security attributes: [{0}]",
                        this.SourceWithRoot.RootContext.RootSource.FullName));
                }

                this.Target.Attributes = this.SourceWithRoot.Source.Attributes;
                this.Target.DeclaringType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.DeclaringType).Resolve();
                this.Target.BaseType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.BaseType);

                foreach (var interfaceType in this.SourceWithRoot.Source.Interfaces)
                {
                    this.Target.Interfaces.Add(this.SourceWithRoot.RootImport(interfaceType));
                }

                // TODO look more closely at type packing size
                this.Target.PackingSize = this.SourceWithRoot.Source.PackingSize;

                // TODO look more closely at type class size
                this.Target.ClassSize = this.SourceWithRoot.Source.ClassSize;

                // TODO research correct usage of type metadata token
                this.Target.MetadataToken = new MetadataToken(this.SourceWithRoot.Source.MetadataToken.TokenType, this.SourceWithRoot.Source.MetadataToken.RID);
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around, but only for non-root type
            if (this.Target != this.SourceWithRoot.RootContext.RootTarget) { this.Target.CustomAttributes.Clear(); }
            this.Target.CloneAllCustomAttributes(this.SourceWithRoot.Source, this.SourceWithRoot.RootContext);
        }

        private void CreateWireframeAndCloners()
        {
            if (!this.IsWireframeCompleted)
            {
                var voidReference = this.Target.Module.Import(typeof(void));

                foreach (var sourceWithRoot in from type in this.SourceWithRoot.Source.NestedTypes
                                       where !type.IsSkipped()
                                       select new TypeSourceWithRoot(this.SourceWithRoot.RootContext, type))
                {
                    var target = new TypeDefinition(sourceWithRoot.Source.Namespace, sourceWithRoot.Source.Name, 0);
                    this.Target.NestedTypes.Add(target);
                    var typeCloner = new TypeCloner(target, sourceWithRoot);
                    typeCloner.CreateWireframeAndCloners();
                    this.TypeCloners.Add(typeCloner);
                }

                foreach (var sourceWithRoot in from field in this.SourceWithRoot.Source.Fields
                                       where !field.IsSkipped()
                                       select new FieldSourceWithRoot(this.SourceWithRoot.RootContext, field))
                {
                    var target = new FieldDefinition(sourceWithRoot.Source.Name, 0, voidReference);
                    this.Target.Fields.Add(target);
                    this.FieldCloners.Add(new FieldCloner(target, sourceWithRoot));
                }

                foreach (var sourceWithRoot in from method in this.SourceWithRoot.Source.Methods
                                       where !method.IsSkipped()
                                       select new MethodSourceWithRoot(this.SourceWithRoot.RootContext, method))
                {
                    if (sourceWithRoot.Source.Name == ".cctor" && sourceWithRoot.Source.IsStatic)
                    {
                        throw new WeavingException(string.Format(
                            "Configured mixin implementation cannot have a type initializer (i.e. static constructor): [{0}]",
                            sourceWithRoot.Source.FullName));
                    }

                    var target = new MethodDefinition(sourceWithRoot.Source.Name, 0, voidReference);
                    this.Target.Methods.Add(target);
                    this.MethodCloners.Add(new MethodCloner(target, sourceWithRoot));
                }

                foreach (var sourceWithRoot in from property in this.SourceWithRoot.Source.Properties
                                       where !property.IsSkipped()
                                       select new PropertySourceWithRoot(this.SourceWithRoot.RootContext, property))
                {
                    var target = new PropertyDefinition(sourceWithRoot.Source.Name, 0, voidReference);
                    this.Target.Properties.Add(target);
                    this.PropertyCloners.Add(new PropertyCloner(target, sourceWithRoot));
                }

                foreach (var sourceWithRoot in from @event in this.SourceWithRoot.Source.Events
                                       where !@event.IsSkipped()
                                       select new EventSourceWithRoot(this.SourceWithRoot.RootContext, @event))
                {
                    var target = new EventDefinition(sourceWithRoot.Source.Name, 0, voidReference);
                    this.Target.Events.Add(target);
                    this.EventCloners.Add(new EventCloner(target, sourceWithRoot));
                }
            }
            this.IsWireframeCompleted = true;
        }
    }
}
