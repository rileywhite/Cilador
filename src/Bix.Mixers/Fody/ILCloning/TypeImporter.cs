using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class TypeImporter : MemberImporterBase<Type, TypeDefinition>
    {
        public TypeImporter() { }

        public TypeImporter(ModuleDefinition referencingModule)
            : base(referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
        }

        public override TypeDefinition GetMemberDefinition(Type memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
