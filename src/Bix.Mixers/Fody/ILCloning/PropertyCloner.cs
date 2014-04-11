using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class PropertyCloner : MemberClonerBase<PropertyDefinition, PropertySourceWithRoot>
    {
        public PropertyCloner(PropertyDefinition target, PropertySourceWithRoot source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void Clone()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Source.Name);

            this.Target.Attributes = this.Source.Source.Attributes;
            this.Target.Constant = this.Source.Source.Constant;
            this.Target.HasConstant = this.Source.Source.HasConstant;
            this.Target.HasDefault = this.Source.Source.HasDefault;
            this.Target.HasThis = this.Source.Source.HasThis;
            this.Target.IsRuntimeSpecialName = this.Source.Source.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.Source.IsSpecialName;

            this.Target.PropertyType = this.Source.RootImport(this.Source.Source.PropertyType);

            for (int i = 0; i < this.Source.Source.OtherMethods.Count; i++)
            {
                this.Target.OtherMethods.Add(null);
            }

            foreach(var method in this.Target.DeclaringType.Methods)
            {
                if (this.Source.Source.GetMethod != null &&
                    this.Target.GetMethod == null &&
                    method.SignatureEquals(this.Source.Source.GetMethod))
                {
                    this.Target.GetMethod = this.Source.RootImport(method).Resolve();
                }

                if (this.Source.Source.SetMethod != null &&
                    this.Target.SetMethod == null &&
                    method.SignatureEquals(this.Source.Source.SetMethod))
                {
                    this.Target.SetMethod = this.Source.RootImport(method).Resolve();
                }

                for (int i = 0; i < this.Source.Source.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.Source.Source.OtherMethods[i]))
                    {
                        this.Target.OtherMethods[i] = this.Source.RootImport(method).Resolve();
                    }
                }
            }
            this.Target.MetadataToken = this.Source.Source.MetadataToken;

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.RootImportAllCustomAttributes(this.Source, this.Source.Source.CustomAttributes);

            if (this.Source.Source.HasParameters)
            {
                // TODO property parameter cloning
                throw new NotImplementedException("Implement property parameters when needed");
            }

            this.IsCloned = true;

            Contract.Assert(this.Target.SignatureEquals(this.Source.Source));
            Contract.Assert((this.Target.GetMethod == null) == (this.Source.Source.GetMethod == null));
            Contract.Assert((this.Target.SetMethod == null) == (this.Source.Source.SetMethod == null));
            for (int i = 0; i < this.Source.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.Source.Source.OtherMethods[i] == null));
            }
        }
    }
}
