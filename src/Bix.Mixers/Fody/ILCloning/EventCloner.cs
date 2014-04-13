using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class EventCloner : MemberClonerBase<EventDefinition, EventSourceWithRoot>
    {
        public EventCloner(EventDefinition target, EventSourceWithRoot source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void CloneStructure()
        {
            // TODO event cloning
            throw new NotImplementedException("Implement event cloning when needed");
        }
    }
}
