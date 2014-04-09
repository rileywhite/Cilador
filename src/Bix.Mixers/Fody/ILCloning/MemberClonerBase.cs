using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal abstract class MemberClonerBase<TMemberInfo, TMemberDefinition, TMemberWithRespectToModule> : IMemberCloner
        where TMemberInfo : MemberInfo
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
        where TMemberWithRespectToModule : MemberWithRespectToModuleBase<TMemberInfo, TMemberDefinition>
    {
        public MemberClonerBase(TMemberDefinition target, TMemberWithRespectToModule source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Source != null);

            if (source.MemberInfo.IsSkipped())
            {
                throw new InvalidOperationException("Cannot clone a skipped member");
            }

            this.Target = target;
            this.Source = source;
        }

        public TMemberDefinition Target { get; private set; }

        public TMemberWithRespectToModule Source { get; private set; }

        public bool IsCloned { get; protected set; }

        public abstract void Clone();
    }
}
