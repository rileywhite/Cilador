using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class FieldCloner : MemberClonerBase<FieldDefinition, FieldSourceWithRoot>
    {
        public FieldCloner(FieldDefinition target, FieldSourceWithRoot source)
            : base(target, source) { }

        public override void Clone()
        {
            Contract.Requires(this.Target.Name == this.Source.Source.Name);

            this.Target.Attributes = this.Source.Source.Attributes;
            this.Target.Constant = this.Source.Source.Constant;
            this.Target.HasConstant = this.Source.Source.HasConstant;
            this.Target.HasDefault = this.Source.Source.HasDefault;
            this.Target.IsAssembly = this.Source.Source.IsAssembly;
            this.Target.IsCompilerControlled = this.Source.Source.IsCompilerControlled;
            this.Target.IsFamily = this.Source.Source.IsFamily;
            this.Target.IsFamilyAndAssembly = this.Source.Source.IsFamilyAndAssembly;
            this.Target.IsFamilyOrAssembly = this.Source.Source.IsFamilyOrAssembly;
            this.Target.IsInitOnly = this.Source.Source.IsInitOnly;
            this.Target.IsLiteral = this.Source.Source.IsLiteral;
            this.Target.IsNotSerialized = this.Source.Source.IsNotSerialized;
            this.Target.IsPInvokeImpl = this.Source.Source.IsPInvokeImpl;
            this.Target.IsPrivate = this.Source.Source.IsPrivate;
            this.Target.IsPublic = this.Source.Source.IsPublic;
            this.Target.IsRuntimeSpecialName = this.Source.Source.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.Source.IsSpecialName;
            this.Target.IsStatic = this.Source.Source.IsStatic;
            this.Target.Offset = this.Source.Source.Offset;

            if (this.Source.Source.MarshalInfo == null)
            {
                this.Target.MarshalInfo = null;
            }
            else
            {
                this.Target.MarshalInfo = new MarshalInfo(this.Source.Source.MarshalInfo.NativeType);
            }

            if (this.Source.Source.InitialValue != null)
            {
                var initialValue = new byte[this.Source.Source.InitialValue.LongLength];
                this.Source.Source.InitialValue.CopyTo(initialValue, 0);
                this.Target.InitialValue = initialValue;
            }

            this.Target.MetadataToken = new MetadataToken(
                this.Source.Source.MetadataToken.TokenType,
                this.Source.Source.MetadataToken.RID);

            this.Target.FieldType = this.Source.RootImport(this.Source.Source.FieldType);

            // for some reason, I'm seeing duplicate custom attributes if I don't clear first
            // adding a console output line line like this makes it go away: Console.WriteLine(this.Target.CustomAttributes.Count);
            // but I opted for an explicit clear instead
            // (breaking anywhere before the RootImportAll call in the debugger keeps it from happening, too)
            this.Target.CustomAttributes.Clear();
            Contract.Assert(this.Target.CustomAttributes.Count == 0);
            this.Target.RootImportAllCustomAttributes(this.Source, this.Source.Source.CustomAttributes);

            this.IsCloned = true;
        }
    }
}
