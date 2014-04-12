using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal abstract class MemberSourceWithRootBase<TMemberDefinition> : IRootImportProvider
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public MemberSourceWithRootBase(
            RootContext rootContext,
            TMemberDefinition source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.RootContext != null);
            Contract.Ensures(this.Source != null);

            this.RootContext = rootContext;
            this.Source = source;
        }

        public TMemberDefinition Source { get; private set; }

        public RootContext RootContext { get; protected set; }

        TypeDefinition IRootImportProvider.RootTarget
        {
            get { return this.RootContext.RootTarget; }
        }

        public TItem DynamicRootImport<TItem>(TItem sourceItem)
        {
            return this.RootContext.DynamicRootImport<TItem>(sourceItem);
        }

        public TypeReference RootImport(TypeReference sourceType)
        {
            return this.RootContext.RootImport(sourceType);
        }

        public FieldReference RootImport(FieldReference sourceField)
        {
            return this.RootContext.RootImport(sourceField);
        }

        public PropertyReference RootImport(PropertyReference sourceProperty)
        {
            return this.RootContext.RootImport(sourceProperty);
        }

        public MethodReference RootImport(MethodReference sourceMethod)
        {
            return this.RootContext.RootImport(sourceMethod);
        }

        public EventReference RootImport(EventReference sourceEvent)
        {
            return this.RootContext.RootImport(sourceEvent);
        }
    }
}
