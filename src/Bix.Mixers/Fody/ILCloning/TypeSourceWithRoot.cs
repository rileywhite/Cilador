using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class TypeSourceWithRoot : MemberSourceWithRootBase<TypeDefinition>
    {
        public static TypeSourceWithRoot CreateWithRootSourceAndTarget(TypeDefinition source, TypeDefinition target)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Requires(source != target);
            Contract.Ensures(Contract.Result<TypeSourceWithRoot>() != null);

            return new TypeSourceWithRoot(
                new RootContext(source, target),
                source);

        }

        public TypeSourceWithRoot(RootContext rootContext, TypeDefinition source)
            : base(rootContext, source) { }
    }
}
