/***************************************************************************/
// Copyright 2013-2014 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

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
            this.Target.Offset = this.SourceWithRoot.Source.Offset;

            // TODO research correct usage of field MarshalInfo
            if (this.SourceWithRoot.Source.MarshalInfo == null)
            {
                this.Target.MarshalInfo = null;
            }
            else
            {
                this.Target.MarshalInfo = new MarshalInfo(this.SourceWithRoot.Source.MarshalInfo.NativeType);
            }

            // TODO research correct usage of field InitialValue
            if (this.SourceWithRoot.Source.InitialValue != null)
            {
                var initialValue = new byte[this.SourceWithRoot.Source.InitialValue.LongLength];
                this.SourceWithRoot.Source.InitialValue.CopyTo(initialValue, 0);
                this.Target.InitialValue = initialValue;
            }

            // TODO research correct usage of field MetadataToken
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
            this.Target.CloneAllCustomAttributes(this.SourceWithRoot.Source, this.SourceWithRoot.RootContext);

            this.IsStructureCloned = true;
        }
    }
}
