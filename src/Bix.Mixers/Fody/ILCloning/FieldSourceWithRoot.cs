using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class FieldSourceWithRoot
        : MemberSourceWithRootBase<FieldDefinition>
    {
        public FieldSourceWithRoot(RootContext rootContext, FieldDefinition source)
            : base(rootContext, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }
    }
}
