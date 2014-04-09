using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class MethodImporter : MemberImporterBase<MethodBase, MethodDefinition>
    {
        public MethodImporter() { }

        public MethodImporter(ModuleDefinition referencingModule)
            : base(referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
        }

        public override MethodDefinition GetMemberDefinition(MethodBase memberInfo)
        {
            return this.ReferencingModule.Import(memberInfo).Resolve();
        }
    }
}
