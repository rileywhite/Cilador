using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class MethodWithRespectToModule
        : MemberWithRespectToModuleBase<MethodBase, MethodDefinition>
    {
        public MethodWithRespectToModule(RootContext rootContext, MethodBase method, ModuleDefinition referencingModule)
            : base(rootContext, method, referencingModule)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }

        private MethodImporter memberImporter = new MethodImporter();
        public override IMemberImporter<MethodBase, MethodDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
