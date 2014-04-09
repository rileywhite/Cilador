using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class FieldCloner : MemberClonerBase<FieldInfo, FieldDefinition, FieldWithRespectToModule>
    {
        public FieldCloner(FieldDefinition target, FieldWithRespectToModule source)
            : base(target, source) { }

        public override void Clone()
        {
            Contract.Requires(this.Target.Name == this.Source.MemberDefinition.Name);

            this.Target.Attributes = this.Source.MemberDefinition.Attributes;
            this.Target.Constant = this.Source.MemberDefinition.Constant;
            this.Target.HasConstant = this.Source.MemberDefinition.HasConstant;
            this.Target.HasDefault = this.Source.MemberDefinition.HasDefault;
            this.Target.IsAssembly = this.Source.MemberDefinition.IsAssembly;
            this.Target.IsCompilerControlled = this.Source.MemberDefinition.IsCompilerControlled;
            this.Target.IsFamily = this.Source.MemberDefinition.IsFamily;
            this.Target.IsFamilyAndAssembly = this.Source.MemberDefinition.IsFamilyAndAssembly;
            this.Target.IsFamilyOrAssembly = this.Source.MemberDefinition.IsFamilyOrAssembly;
            this.Target.IsInitOnly = this.Source.MemberDefinition.IsInitOnly;
            this.Target.IsLiteral = this.Source.MemberDefinition.IsLiteral;
            this.Target.IsNotSerialized = this.Source.MemberDefinition.IsNotSerialized;
            this.Target.IsPInvokeImpl = this.Source.MemberDefinition.IsPInvokeImpl;
            this.Target.IsPrivate = this.Source.MemberDefinition.IsPrivate;
            this.Target.IsPublic = this.Source.MemberDefinition.IsPublic;
            this.Target.IsRuntimeSpecialName = this.Source.MemberDefinition.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.MemberDefinition.IsSpecialName;
            this.Target.IsStatic = this.Source.MemberDefinition.IsStatic;
            this.Target.Offset = this.Source.MemberDefinition.Offset;

            if (this.Source.MemberDefinition.MarshalInfo == null)
            {
                this.Target.MarshalInfo = null;
            }
            else
            {
                this.Target.MarshalInfo = new MarshalInfo(this.Source.MemberDefinition.MarshalInfo.NativeType);
            }

            if (this.Source.MemberDefinition.InitialValue != null)
            {
                var initialValue = new byte[this.Source.MemberDefinition.InitialValue.LongLength];
                this.Source.MemberDefinition.InitialValue.CopyTo(initialValue, 0);
                this.Target.InitialValue = initialValue;
            }

            this.Target.MetadataToken = new MetadataToken(
                this.Source.MemberDefinition.MetadataToken.TokenType,
                this.Source.MemberDefinition.MetadataToken.RID);

            this.Target.FieldType = this.Source.RootImport(this.Source.MemberDefinition.FieldType);

            // for some reason, I'm seeing duplicate custom attributes if I don't clear first
            // adding a console output line line like this makes it go away: Console.WriteLine(this.Target.CustomAttributes.Count);
            // but I opted for an explicit clear instead
            // (breaking anywhere before the RootImportAll call in the debugger keeps it from happening, too)
            this.Target.CustomAttributes.Clear();
            Contract.Assert(this.Target.CustomAttributes.Count == 0);
            this.Target.RootImportAllCustomAttributes(this.Source, this.Source.MemberDefinition.CustomAttributes);

            this.IsCloned = true;
        }
    }
}
