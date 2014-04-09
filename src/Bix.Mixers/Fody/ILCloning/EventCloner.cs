using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class EventCloner : MemberClonerBase<EventInfo, EventDefinition, EventWithRespectToModule>
    {
        public EventCloner(EventDefinition target, EventWithRespectToModule source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void Clone()
        {
            // TODO event cloning
            throw new NotImplementedException("Implement event cloning when needed");
        }
    }
}
