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
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.SourceWithRoot.Source.Name);

            this.Target.Attributes = this.SourceWithRoot.Source.Attributes;
            this.Target.EventType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.EventType);

            // TODO reseach correct usage of event MetadataToken
            this.Target.MetadataToken = new MetadataToken(
                this.SourceWithRoot.Source.MetadataToken.TokenType,
                this.SourceWithRoot.Source.MetadataToken.RID);


            foreach (var method in this.Target.DeclaringType.Methods)
            {
                if (this.SourceWithRoot.Source.AddMethod != null &&
                    this.Target.AddMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.AddMethod))
                {
                    this.Target.AddMethod = method;
                }

                if (this.SourceWithRoot.Source.RemoveMethod != null &&
                    this.Target.RemoveMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.RemoveMethod))
                {
                    this.Target.RemoveMethod = method;
                }

                if (this.SourceWithRoot.Source.InvokeMethod != null &&
                    this.Target.InvokeMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.InvokeMethod))
                {
                    this.Target.InvokeMethod = method;
                }

                for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.SourceWithRoot.Source.OtherMethods[i]))
                    {
                        this.Target.OtherMethods[i] = method;
                    }
                }
            }

            // I did not check for a similar issue here as with the duplication in the FieldCloner...adding a clear line just to be safe
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.SourceWithRoot.Source, this.SourceWithRoot.RootContext);

            this.IsStructureCloned = true;

            Contract.Assert((this.Target.AddMethod == null) == (this.SourceWithRoot.Source.AddMethod == null));
            Contract.Assert((this.Target.RemoveMethod == null) == (this.SourceWithRoot.Source.RemoveMethod == null));
            Contract.Assert((this.Target.InvokeMethod == null) == (this.SourceWithRoot.Source.InvokeMethod == null));
            for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.SourceWithRoot.Source.OtherMethods[i] == null));
            }
        }
    }
}
