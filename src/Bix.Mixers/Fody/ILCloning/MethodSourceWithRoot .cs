using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class MethodSourceWithRoot
        : MemberSourceWithRootBase<MethodDefinition>
    {
        public MethodSourceWithRoot(RootContext rootContext, MethodDefinition source)
            : base(rootContext, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }
    }
}
