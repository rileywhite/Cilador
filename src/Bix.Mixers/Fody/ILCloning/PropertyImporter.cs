using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class PropertyImporter : MemberImporterBase<PropertyInfo, PropertyDefinition>
    {
        public PropertyImporter() { }

        public PropertyImporter(ModuleDefinition referencingModule)
            : base(referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
        }

        public override PropertyDefinition GetMemberDefinition(PropertyInfo memberInfo)
        {
            var sourceType = this.ReferencingModule.Import(memberInfo.DeclaringType).Resolve();
            return sourceType.Properties.First(property => property.Name == memberInfo.Name);
        }
    }
}
