using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal abstract class MemberWithRespectToModuleBase<TMemberInfo, TMemberDefinition> : IRootImportProvider
        where TMemberInfo : MemberInfo
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public MemberWithRespectToModuleBase(
            RootContext rootContext,
            TMemberInfo member,
            ModuleDefinition referencingModule)
        {
            Contract.Requires(member != null);
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.MemberInfo != null);
            Contract.Ensures(this.ReferencingModule != null);
            Contract.Ensures(this.MemberImporter != null);
            Contract.Ensures(this.MemberDefinition != null);

            this.RootContext = rootContext;
            this.MemberInfo = member;
            this.ReferencingModule = referencingModule;
            this.MemberImporter.ReferencingModule = referencingModule;
        }

        public ModuleDefinition ReferencingModule { get; private set; }

        public abstract IMemberImporter<TMemberInfo, TMemberDefinition> MemberImporter { get; }

        public TMemberInfo MemberInfo { get; private set; }

        private TMemberDefinition memberDefinition;
        public TMemberDefinition MemberDefinition
        {
            get
            {
                if(this.memberDefinition == null)
                {
                    this.memberDefinition = this.MemberImporter.GetMemberDefinition(this.MemberInfo);
                }
                return this.memberDefinition;
            }
        }

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
