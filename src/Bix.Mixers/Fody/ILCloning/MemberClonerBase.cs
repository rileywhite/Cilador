using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal abstract class MemberClonerBase<TMemberDefinition, TMemberWithRoot> : IMemberCloner
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
        where TMemberWithRoot : MemberSourceWithRootBase<TMemberDefinition>
    {
        public MemberClonerBase(TMemberDefinition target, TMemberWithRoot sourceWithRoot)
        {
            Contract.Requires(target != null);
            Contract.Requires(sourceWithRoot != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.SourceWithRoot != null);

            if (sourceWithRoot.Source.IsSkipped())
            {
                throw new InvalidOperationException("Cannot clone a skipped member");
            }

            this.Target = target;
            this.SourceWithRoot = sourceWithRoot;
        }

        public TMemberDefinition Target { get; private set; }

        public TMemberWithRoot SourceWithRoot { get; private set; }

        public bool IsStructureCloned { get; protected set; }

        public abstract void CloneStructure();
    }
}
