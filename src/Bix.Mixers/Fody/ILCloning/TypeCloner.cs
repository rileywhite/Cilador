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

        private bool IsScaffoldingCompleted { get; set; }

        public override void Clone()
        {
            this.PopulateClonersWithScaffolding();
            this.CopyTypeData();
            this.TypeCloners.Clone();
            this.FieldCloners.Clone();
            this.MethodCloners.Clone();
            this.PropertyCloners.Clone();
            this.EventCloners.Clone();
            this.IsCloned = true;
        }

        public void CloneMethodBodies()
        {
            foreach (var methodCloner in this.MethodCloners) { methodCloner.CloneBody(); }
            foreach (var typeCloner in this.TypeCloners) { typeCloner.CloneMethodBodies(); }
        }

        private void CopyTypeData()
        {
            if (this.Target != this.SourceWithRoot.RootContext.RootTarget)
            {
                this.Target.Attributes = this.SourceWithRoot.Source.Attributes;
                this.Target.HasSecurity = this.SourceWithRoot.Source.HasSecurity;
                this.Target.IsAbstract = this.SourceWithRoot.Source.IsAbstract;
                this.Target.IsAnsiClass = this.SourceWithRoot.Source.IsAnsiClass;
                this.Target.IsAutoClass = this.SourceWithRoot.Source.IsAutoClass;
                this.Target.IsAutoLayout = this.SourceWithRoot.Source.IsAutoLayout;
                this.Target.IsBeforeFieldInit = this.SourceWithRoot.Source.IsBeforeFieldInit;
                this.Target.IsClass = this.SourceWithRoot.Source.IsClass;
                this.Target.IsExplicitLayout = this.SourceWithRoot.Source.IsExplicitLayout;
                this.Target.IsImport = this.SourceWithRoot.Source.IsImport;
                this.Target.IsInterface = this.SourceWithRoot.Source.IsInterface;
                this.Target.IsNestedAssembly = this.SourceWithRoot.Source.IsNestedAssembly;
                this.Target.IsNestedFamily = this.SourceWithRoot.Source.IsNestedFamily;
                this.Target.IsNestedFamilyAndAssembly = this.SourceWithRoot.Source.IsNestedFamilyAndAssembly;
                this.Target.IsNestedFamilyOrAssembly = this.SourceWithRoot.Source.IsNestedFamilyOrAssembly;
                this.Target.IsNestedPrivate = this.SourceWithRoot.Source.IsNestedPrivate;
                this.Target.IsNestedPublic = this.SourceWithRoot.Source.IsNestedPublic;
                this.Target.IsNotPublic = this.SourceWithRoot.Source.IsNotPublic;
                this.Target.IsPublic = this.SourceWithRoot.Source.IsPublic;
                this.Target.IsRuntimeSpecialName = this.SourceWithRoot.Source.IsRuntimeSpecialName;
                this.Target.IsSealed = this.SourceWithRoot.Source.IsSealed;
                this.Target.IsSequentialLayout = this.SourceWithRoot.Source.IsSequentialLayout;
                this.Target.IsSerializable = this.SourceWithRoot.Source.IsSerializable;
                this.Target.IsSpecialName = this.SourceWithRoot.Source.IsSpecialName;
                this.Target.IsUnicodeClass = this.SourceWithRoot.Source.IsUnicodeClass;
                this.Target.IsValueType = this.SourceWithRoot.Source.IsValueType;
                this.Target.IsWindowsRuntime = this.SourceWithRoot.Source.IsWindowsRuntime;

                // TODO look more closely at type packing size
                this.Target.PackingSize = this.SourceWithRoot.Source.PackingSize;

                // TODO look more closely at type class size
                this.Target.ClassSize = this.SourceWithRoot.Source.ClassSize;

                // TODO look more closely at type scope
                this.Target.Scope = this.SourceWithRoot.Source.Scope;

                if (this.SourceWithRoot.Source.IsNested)
                {
                    this.Target.DeclaringType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.DeclaringType).Resolve();
                }

                this.Target.BaseType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.BaseType);

                // TODO look more closely at type metadata token
                this.Target.MetadataToken = this.SourceWithRoot.Source.MetadataToken;
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around, but only for non-root type
            if (this.Target != this.SourceWithRoot.RootContext.RootTarget) { this.Target.CustomAttributes.Clear(); }
            this.Target.RootImportAllCustomAttributes(this.SourceWithRoot, this.SourceWithRoot.Source.CustomAttributes);

            if (this.SourceWithRoot.Source.HasGenericParameters)
            {
                // TODO type generic parameters
                throw new NotImplementedException("Implement type generic parameters when needed");
            }

            if (this.SourceWithRoot.Source.HasSecurityDeclarations)
            {
                // TODO type security declarations
                throw new NotImplementedException("Implement type security declarations when needed");
            }
        }

        private void PopulateClonersWithScaffolding()
        {
            if (!this.IsScaffoldingCompleted)
            {
                var voidReference = this.Target.Module.Import(typeof(void));

                foreach (var sourceWithRoot in from type in this.SourceWithRoot.Source.NestedTypes
                                       where !type.IsSkipped()
                                       select new TypeSourceWithRoot(this.SourceWithRoot.RootContext, type))
                {
                    var target = new TypeDefinition(
                        sourceWithRoot.Source.Namespace,
                        sourceWithRoot.Source.Name,
                        0);
                    target.Module.Types.Add(target);
                    this.Target.NestedTypes.Add(target);

                    var typeCloner = new TypeCloner(target, sourceWithRoot);
                    typeCloner.PopulateClonersWithScaffolding();
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
            this.IsScaffoldingCompleted = true;
        }
    }
}
