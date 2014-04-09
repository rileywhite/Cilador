using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class EventImporter : MemberImporterBase<EventInfo, EventDefinition>
    {
        public EventImporter() { }

        public EventImporter(ModuleDefinition referencingModule)
            : base(referencingModule)
        {
            Contract.Requires(referencingModule != null);
            Contract.Ensures(this.ReferencingModule != null);
        }

        public override EventDefinition GetMemberDefinition(EventInfo memberInfo)
        {
            throw new NotImplementedException("Implement this when needed");
        }
    }
}
