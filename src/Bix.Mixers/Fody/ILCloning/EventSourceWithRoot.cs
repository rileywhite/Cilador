using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class EventSourceWithRoot
        : MemberSourceWithRootBase<EventDefinition>
    {
        public EventSourceWithRoot(RootContext rootContext, EventDefinition source)
            : base(rootContext, source) { }
    }
}
