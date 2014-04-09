using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class TypeWithRespectToModule : MemberWithRespectToModuleBase<Type, TypeDefinition>
    {
        public TypeWithRespectToModule(Type type, TypeDefinition targetType)
            : base(null, type, targetType.Module)
        {
            Contract.Ensures(this.RootContext != null);

            this.RootContext = new RootContext(this, targetType);
        }

        public TypeWithRespectToModule(RootContext rootContext, Type type, ModuleDefinition referencingModule)
            : base(rootContext, type, referencingModule)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }

        private TypeImporter memberImporter = new TypeImporter();
        public override IMemberImporter<Type, TypeDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
