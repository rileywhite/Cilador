using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class PropertySourceWithRoot
        : MemberSourceWithRootBase<PropertyDefinition>
    {
        public PropertySourceWithRoot(RootContext rootContext, PropertyDefinition source)
            : base(rootContext, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }
    }
}
