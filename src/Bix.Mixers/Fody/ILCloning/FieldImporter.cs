using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class FieldImporter : MemberImporterBase<FieldInfo, FieldDefinition>
    {
        public FieldImporter() { }

        public FieldImporter(ModuleDefinition referencingModule)
            : base(referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
        }

        public override FieldDefinition GetMemberDefinition(FieldInfo memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
