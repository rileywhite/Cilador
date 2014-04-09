using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class EventWithRespectToModule
        : MemberWithRespectToModuleBase<EventInfo, EventDefinition>
    {
        public EventWithRespectToModule(RootContext rootContext, EventInfo @event, ModuleDefinition referencingModule)
            : base(rootContext, @event, referencingModule)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }

        public EventImporter memberImporter = new EventImporter();
        public override IMemberImporter<EventInfo, EventDefinition> MemberImporter
        {
            get { return this.memberImporter; }
        }
    }
}
