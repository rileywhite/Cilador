using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class PropertyCloner : MemberClonerBase<PropertyInfo, PropertyDefinition, PropertyWithRespectToModule>
    {
        public PropertyCloner(PropertyDefinition target, PropertyWithRespectToModule source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void Clone()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.MemberDefinition.Name);

            this.Target.Attributes = this.Source.MemberDefinition.Attributes;
            this.Target.Constant = this.Source.MemberDefinition.Constant;
            this.Target.HasConstant = this.Source.MemberDefinition.HasConstant;
            this.Target.HasDefault = this.Source.MemberDefinition.HasDefault;
            this.Target.HasThis = this.Source.MemberDefinition.HasThis;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;

            this.Target.PropertyType = this.Source.RootImport(this.Source.MemberDefinition.PropertyType);

            for (int i = 0; i < this.Source.MemberDefinition.OtherMethods.Count; i++)
            {
                this.Target.OtherMethods.Add(null);
            }

            foreach(var method in this.Target.DeclaringType.Methods)
            {
                if (this.Source.MemberDefinition.GetMethod != null &&
                    this.Target.GetMethod == null &&
                    method.SignatureEquals(this.Source.MemberDefinition.GetMethod))
                {
                    this.Target.GetMethod = this.Source.RootImport(method).Resolve();
                }

                if (this.Source.MemberDefinition.SetMethod != null &&
                    this.Target.SetMethod == null &&
                    method.SignatureEquals(this.Source.MemberDefinition.SetMethod))
                {
                    this.Target.SetMethod = this.Source.RootImport(method).Resolve();
                }

                for (int i = 0; i < this.Source.MemberDefinition.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.Source.MemberDefinition.OtherMethods[i]))
                    {
                        this.Target.OtherMethods[i] = this.Source.RootImport(method).Resolve();
                    }
                }
            }
            this.Target.MetadataToken = this.Source.MemberDefinition.MetadataToken;

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.RootImportAllCustomAttributes(this.Source, this.Source.MemberDefinition.CustomAttributes);

            if (this.Source.MemberDefinition.HasParameters)
            {
                throw new NotImplementedException("Implement property parameters when needed");
            }

            this.IsCloned = true;

            Contract.Assert(this.Target.SignatureEquals(this.Source.MemberDefinition));
            Contract.Assert((this.Target.GetMethod == null) == (this.Source.MemberDefinition.GetMethod == null));
            Contract.Assert((this.Target.SetMethod == null) == (this.Source.MemberDefinition.SetMethod == null));
            for (int i = 0; i < this.Source.MemberDefinition.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.Source.MemberDefinition.OtherMethods[i] == null));
            }
        }
    }
}
