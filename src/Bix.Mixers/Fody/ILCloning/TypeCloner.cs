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
            Contract.Ensures(this.Cloners != null);
            this.Cloners = new List<IMemberCloner>();
        }

        private List<IMemberCloner> Cloners { get; set; }

        private bool IsScaffoldingCompleted { get; set; }

        public override void Clone()
        {
            this.PopulateClonersWithScaffolding();
            this.CopyTypeData();
            this.Cloners.Clone();
            this.IsCloned = true;
        }

        private void CopyTypeData()
        {
            if (this.Target != this.Source.RootContext.RootTarget)
            {
                this.Target.Attributes = this.Source.Source.Attributes;
                this.Target.HasSecurity = this.Source.Source.HasSecurity;
                this.Target.IsAbstract = this.Source.Source.IsAbstract;
                this.Target.IsAnsiClass = this.Source.Source.IsAnsiClass;
                this.Target.IsAutoClass = this.Source.Source.IsAutoClass;
                this.Target.IsAutoLayout = this.Source.Source.IsAutoLayout;
                this.Target.IsBeforeFieldInit = this.Source.Source.IsBeforeFieldInit;
                this.Target.IsClass = this.Source.Source.IsClass;
                this.Target.IsExplicitLayout = this.Source.Source.IsExplicitLayout;
                this.Target.IsImport = this.Source.Source.IsImport;
                this.Target.IsInterface = this.Source.Source.IsInterface;
                this.Target.IsNestedAssembly = this.Source.Source.IsNestedAssembly;
                this.Target.IsNestedFamily = this.Source.Source.IsNestedFamily;
                this.Target.IsNestedFamilyAndAssembly = this.Source.Source.IsNestedFamilyAndAssembly;
                this.Target.IsNestedFamilyOrAssembly = this.Source.Source.IsNestedFamilyOrAssembly;
                this.Target.IsNestedPrivate = this.Source.Source.IsNestedPrivate;
                this.Target.IsNestedPublic = this.Source.Source.IsNestedPublic;
                this.Target.IsNotPublic = this.Source.Source.IsNotPublic;
                this.Target.IsPublic = this.Source.Source.IsPublic;
                this.Target.IsRuntimeSpecialName = this.Source.Source.IsRuntimeSpecialName;
                this.Target.IsSealed = this.Source.Source.IsSealed;
                this.Target.IsSequentialLayout = this.Source.Source.IsSequentialLayout;
                this.Target.IsSerializable = this.Source.Source.IsSerializable;
                this.Target.IsSpecialName = this.Source.Source.IsSpecialName;
                this.Target.IsUnicodeClass = this.Source.Source.IsUnicodeClass;
                this.Target.IsValueType = this.Source.Source.IsValueType;
                this.Target.IsWindowsRuntime = this.Source.Source.IsWindowsRuntime;

                // TODO look more closely at type packing size
                this.Target.PackingSize = this.Source.Source.PackingSize;

                // TODO look more closely at type class size
                this.Target.ClassSize = this.Source.Source.ClassSize;

                // TODO look more closely at type scope
                this.Target.Scope = this.Source.Source.Scope;

                if (this.Source.Source.IsNested)
                {
                    this.Target.DeclaringType = this.Source.RootImport(this.Source.Source.DeclaringType).Resolve();
                }

                this.Target.BaseType = this.Source.RootImport(this.Source.Source.BaseType);

                // TODO look more closely at type metadata token
                this.Target.MetadataToken = this.Source.Source.MetadataToken;
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around, but only for non-root type
            if (this.Target != this.Source.RootContext.RootTarget) { this.Target.CustomAttributes.Clear(); }
            this.Target.RootImportAllCustomAttributes(this.Source, this.Source.Source.CustomAttributes);

            if (this.Source.Source.HasGenericParameters)
            {
                // TODO type generic parameters
                throw new NotImplementedException("Implement type generic parameters when needed");
            }

            if (this.Source.Source.HasSecurityDeclarations)
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

                foreach (var source in from type in this.Source.Source.NestedTypes
                                       where !type.IsSkipped()
                                       select new TypeSourceWithRoot(this.Source.RootContext, type))
                {
                    var target = new TypeDefinition(
                        source.Source.Namespace,
                        source.Source.Name,
                        0);
                    target.Module.Types.Add(target);
                    this.Target.NestedTypes.Add(target);

                    var typeCloner = new TypeCloner(target, source);
                    typeCloner.PopulateClonersWithScaffolding();
                    this.Cloners.Add(typeCloner);
                }

                foreach (var source in from field in this.Source.Source.Fields
                                       where !field.IsSkipped()
                                       select new FieldSourceWithRoot(this.Source.RootContext, field))
                {
                    var target = new FieldDefinition(source.Source.Name, 0, voidReference);
                    this.Target.Fields.Add(target);
                    this.Cloners.Add(new FieldCloner(target, source));
                }

                foreach (var source in from property in this.Source.Source.Properties
                                       where !property.IsSkipped()
                                       select new PropertySourceWithRoot(this.Source.RootContext, property))
                {
                    var target = new PropertyDefinition(source.Source.Name, 0, voidReference);
                    this.Target.Properties.Add(target);
                    this.Cloners.Add(new PropertyCloner(target, source));
                }

                foreach (var source in from @event in this.Source.Source.Events
                                       where !@event.IsSkipped()
                                       select new EventSourceWithRoot(this.Source.RootContext, @event))
                {
                    var target = new EventDefinition(source.Source.Name, 0, voidReference);
                    this.Target.Events.Add(target);
                    this.Cloners.Add(new EventCloner(target, source));
                }
            }
            this.IsScaffoldingCompleted = true;
        }
    }
}
