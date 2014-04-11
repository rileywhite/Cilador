using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class TypeSourceWithRoot : MemberSourceWithRootBase<TypeDefinition>
    {
        public TypeSourceWithRoot(TypeDefinition source, TypeDefinition target)
            : base(null, source)
        {
            Contract.Ensures(this.RootContext != null);

            this.RootContext = new RootContext(source, target);
        }

        public TypeSourceWithRoot(RootContext rootContext, TypeDefinition source)
            : base(rootContext, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Ensures(this.RootContext != null);
        }
    }
}
