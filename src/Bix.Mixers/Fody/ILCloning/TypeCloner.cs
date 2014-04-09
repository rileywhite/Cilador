using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class TypeCloner : MemberClonerBase<Type, TypeDefinition, TypeWithRespectToModule>
    {
        public TypeCloner(TypeDefinition target, TypeWithRespectToModule source)
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
                this.Target.Attributes = this.Source.MemberDefinition.Attributes;
                this.Target.HasSecurity = this.Source.MemberDefinition.HasSecurity;
                this.Target.IsAbstract = this.Source.MemberDefinition.IsAbstract;
                this.Target.IsAnsiClass = this.Source.MemberDefinition.IsAnsiClass;
                this.Target.IsAutoClass = this.Source.MemberDefinition.IsAutoClass;
                this.Target.IsAutoLayout = this.Source.MemberDefinition.IsAutoLayout;
                this.Target.IsBeforeFieldInit = this.Source.MemberDefinition.IsBeforeFieldInit;
                this.Target.IsClass = this.Source.MemberDefinition.IsClass;
                this.Target.IsExplicitLayout = this.Source.MemberDefinition.IsExplicitLayout;
                this.Target.IsImport = this.Source.MemberDefinition.IsImport;
                this.Target.IsInterface = this.Source.MemberDefinition.IsInterface;
                this.Target.IsNestedAssembly = this.Source.MemberDefinition.IsNestedAssembly;
                this.Target.IsNestedFamily = this.Source.MemberDefinition.IsNestedFamily;
                this.Target.IsNestedFamilyAndAssembly = this.Source.MemberDefinition.IsNestedFamilyAndAssembly;
                this.Target.IsNestedFamilyOrAssembly = this.Source.MemberDefinition.IsNestedFamilyOrAssembly;
                this.Target.IsNestedPrivate = this.Source.MemberDefinition.IsNestedPrivate;
                this.Target.IsNestedPublic = this.Source.MemberDefinition.IsNestedPublic;
                this.Target.IsNotPublic = this.Source.MemberDefinition.IsNotPublic;
                this.Target.IsPublic = this.Source.MemberDefinition.IsPublic;
                this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
                this.Target.IsSealed = this.Source.MemberDefinition.IsSealed;
                this.Target.IsSequentialLayout = this.Source.MemberDefinition.IsSequentialLayout;
                this.Target.IsSerializable = this.Source.MemberDefinition.IsSerializable;
                this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;
                this.Target.IsUnicodeClass = this.Source.MemberDefinition.IsUnicodeClass;
                this.Target.IsValueType = this.Source.MemberDefinition.IsValueType;
                this.Target.IsWindowsRuntime = this.Source.MemberDefinition.IsWindowsRuntime;

                // TODO look more closely at type packing size
                this.Target.PackingSize = this.Source.MemberDefinition.PackingSize;

                // TODO look more closely at type class size
                this.Target.ClassSize = this.Source.MemberDefinition.ClassSize;

                // TODO look more closely at type scope
                this.Target.Scope = this.Source.MemberDefinition.Scope;

                if (this.Source.MemberDefinition.IsNested)
                {
                    this.Target.DeclaringType = this.Source.RootImport(this.Source.MemberDefinition.DeclaringType).Resolve();
                }

                this.Target.BaseType = this.Source.RootImport(this.Source.MemberDefinition.BaseType);

                // TODO look more closely at type metadata token
                this.Target.MetadataToken = this.Source.MemberDefinition.MetadataToken;
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around, but only for non-root type
            if (this.Target != this.Source.RootContext.RootTarget) { this.Target.CustomAttributes.Clear(); }
            this.Target.RootImportAllCustomAttributes(this.Source, this.Source.MemberDefinition.CustomAttributes);

            if (this.Source.MemberDefinition.HasGenericParameters)
            {
                // TODO type generic parameters
                throw new NotImplementedException("Implement type generic parameters when needed");
            }

            if (this.Source.MemberDefinition.HasSecurityDeclarations)
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

                foreach (var source in from type in this.Source.MemberInfo.GetNestedTypes()
                                       where !type.IsSkipped()
                                       select new TypeWithRespectToModule(this.Source.RootContext, type, this.Target.Module))
                {
                    var target = new TypeDefinition(
                        source.MemberDefinition.Namespace,
                        source.MemberDefinition.Name,
                        0);
                    target.Module.Types.Add(target);
                    this.Target.NestedTypes.Add(target);

                    var typeCloner = new TypeCloner(target, source);
                    typeCloner.PopulateClonersWithScaffolding();
                    this.Cloners.Add(typeCloner);
                }

                foreach (var source in from field in this.Source.MemberInfo.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !field.IsSkipped()
                                       select new FieldWithRespectToModule(this.Source.RootContext, field, this.Target.Module))
                {
                    var target = new FieldDefinition(source.MemberInfo.Name, 0, voidReference);
                    this.Target.Fields.Add(target);
                    this.Cloners.Add(new FieldCloner(target, source));
                }

                foreach (var source in (from method in this.Source.MemberInfo.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                        where !method.IsSkipped()
                                        select new MethodWithRespectToModule(this.Source.RootContext, method, this.Target.Module))
                                       .Concat
                                       (from method in this.Source.MemberInfo.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )
                                        where !method.IsSkipped()
                                        select new MethodWithRespectToModule(this.Source.RootContext, method, this.Target.Module)))
                {
                    var target = new MethodDefinition(source.MemberDefinition.Name, 0, voidReference);
                    this.Target.Methods.Add(target);
                    this.Cloners.Add(new MethodCloner(target, source));
                }

                foreach (var source in from property in this.Source.MemberInfo.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !property.IsSkipped()
                                       select new PropertyWithRespectToModule(this.Source.RootContext, property, this.Target.Module))
                {
                    var target = new PropertyDefinition(source.MemberDefinition.Name, 0, voidReference);
                    this.Target.Properties.Add(target);
                    this.Cloners.Add(new PropertyCloner(target, source));
                }

                foreach (var source in from @event in this.Source.MemberInfo.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                       where !@event.IsSkipped()
                                       select new EventWithRespectToModule(this.Source.RootContext, @event, this.Target.Module))
                {
                    var target = new EventDefinition(source.MemberDefinition.Name, 0, voidReference);
                    this.Target.Events.Add(target);
                    this.Cloners.Add(new EventCloner(target, source));
                }
            }
            this.IsScaffoldingCompleted = true;
        }
    }
}
