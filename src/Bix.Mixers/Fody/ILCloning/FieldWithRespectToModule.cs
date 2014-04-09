using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class FieldWithRespectToModule
        : MemberWithRespectToModuleBase<FieldInfo, FieldDefinition>
    {
        public FieldWithRespectToModule(RootContext rootContext, FieldInfo field, ModuleDefinition referencingModule)
            : base(rootContext, field, referencingModule)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }

        private FieldImporter memberImporter = new FieldImporter();
        public override IMemberImporter<FieldInfo, FieldDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
