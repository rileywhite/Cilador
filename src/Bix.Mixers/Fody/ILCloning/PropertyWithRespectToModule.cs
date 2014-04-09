using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class PropertyWithRespectToModule
        : MemberWithRespectToModuleBase<PropertyInfo, PropertyDefinition>
    {
        public PropertyWithRespectToModule(RootContext rootContext, PropertyInfo property, ModuleDefinition referencingModule)
            : base(rootContext, property, referencingModule)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }

        private PropertyImporter memberImporter = new PropertyImporter();
        public override IMemberImporter<PropertyInfo, PropertyDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
