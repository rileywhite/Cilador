using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal abstract class MemberImporterBase<TMemberInfo, TMemberDefinition> : IMemberImporter<TMemberInfo, TMemberDefinition>
        where TMemberInfo : MemberInfo
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public MemberImporterBase() { }

        public MemberImporterBase(ModuleDefinition referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
            this.ReferencingModule = referencingModule;
        }

        public abstract TMemberDefinition GetMemberDefinition(TMemberInfo memberInfo);

        ModuleDefinition IMemberImporter<TMemberInfo, TMemberDefinition>.ReferencingModule
        {
            get { return this.ReferencingModule; }
            set { this.ReferencingModule = value; }
        }

        private ModuleDefinition referencingModule;
        public ModuleDefinition ReferencingModule
        {
            get { return this.referencingModule; }
            private set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.ReferencingModule != null);
                this.referencingModule = value;
            }
        }
    }
}
