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

        public override void CloneStructure()
        {
            Contract.Requires(this.Target.Name == this.SourceWithRoot.Source.Name);

            this.Target.Attributes = this.SourceWithRoot.Source.Attributes;
            this.Target.Constant = this.SourceWithRoot.Source.Constant;
            this.Target.HasConstant = this.SourceWithRoot.Source.HasConstant;
            this.Target.HasDefault = this.SourceWithRoot.Source.HasDefault;
            this.Target.IsAssembly = this.SourceWithRoot.Source.IsAssembly;
            this.Target.IsCompilerControlled = this.SourceWithRoot.Source.IsCompilerControlled;
            this.Target.IsFamily = this.SourceWithRoot.Source.IsFamily;
            this.Target.IsFamilyAndAssembly = this.SourceWithRoot.Source.IsFamilyAndAssembly;
            this.Target.IsFamilyOrAssembly = this.SourceWithRoot.Source.IsFamilyOrAssembly;
            this.Target.IsInitOnly = this.SourceWithRoot.Source.IsInitOnly;
            this.Target.IsLiteral = this.SourceWithRoot.Source.IsLiteral;
            this.Target.IsNotSerialized = this.SourceWithRoot.Source.IsNotSerialized;
            this.Target.IsPInvokeImpl = this.SourceWithRoot.Source.IsPInvokeImpl;
            this.Target.IsPrivate = this.SourceWithRoot.Source.IsPrivate;
            this.Target.IsPublic = this.SourceWithRoot.Source.IsPublic;
            this.Target.IsRuntimeSpecialName = this.SourceWithRoot.Source.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.SourceWithRoot.Source.IsSpecialName;
            this.Target.IsStatic = this.SourceWithRoot.Source.IsStatic;
            this.Target.Offset = this.SourceWithRoot.Source.Offset;

            if (this.SourceWithRoot.Source.MarshalInfo == null)
            {
                this.Target.MarshalInfo = null;
            }
            else
            {
                this.Target.MarshalInfo = new MarshalInfo(this.SourceWithRoot.Source.MarshalInfo.NativeType);
            }

            if (this.SourceWithRoot.Source.InitialValue != null)
            {
                var initialValue = new byte[this.SourceWithRoot.Source.InitialValue.LongLength];
                this.SourceWithRoot.Source.InitialValue.CopyTo(initialValue, 0);
                this.Target.InitialValue = initialValue;
            }

            this.Target.MetadataToken = new MetadataToken(
                this.SourceWithRoot.Source.MetadataToken.TokenType,
                this.SourceWithRoot.Source.MetadataToken.RID);

            this.Target.FieldType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.FieldType);

            // for some reason, I'm seeing duplicate custom attributes if I don't clear first
            // adding a console output line line like this makes it go away: Console.WriteLine(this.Target.CustomAttributes.Count);
            // but I opted for an explicit clear instead
            // (breaking anywhere before the RootImportAll call in the debugger keeps it from happening, too)
            this.Target.CustomAttributes.Clear();
            Contract.Assert(this.Target.CustomAttributes.Count == 0);
            this.Target.RootImportAllCustomAttributes(this.SourceWithRoot, this.SourceWithRoot.Source.CustomAttributes);

            this.IsStructureCloned = true;
        }
    }
}
