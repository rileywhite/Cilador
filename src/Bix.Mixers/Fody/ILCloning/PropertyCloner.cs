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
    internal class PropertyCloner : MemberClonerBase<PropertyDefinition, PropertySourceWithRoot>
    {
        public PropertyCloner(PropertyDefinition target, PropertySourceWithRoot source)
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
            this.Target.Constant = this.SourceWithRoot.Source.Constant;
            this.Target.HasConstant = this.SourceWithRoot.Source.HasConstant;
            this.Target.HasDefault = this.SourceWithRoot.Source.HasDefault;
            this.Target.HasThis = this.SourceWithRoot.Source.HasThis;
            this.Target.IsRuntimeSpecialName = this.SourceWithRoot.Source.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.SourceWithRoot.Source.IsSpecialName;

            this.Target.PropertyType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.PropertyType);

            // TODO research correct usage of property MetadataToken
            this.Target.MetadataToken = new MetadataToken(
                this.SourceWithRoot.Source.MetadataToken.TokenType,
                this.SourceWithRoot.Source.MetadataToken.RID);

            if (this.SourceWithRoot.Source.HasParameters)
            {
                this.Target.Parameters.CloneAllParameters(this.SourceWithRoot.Source.Parameters, this.SourceWithRoot.RootContext);
            }

            for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
            {
                this.Target.OtherMethods.Add(null);
            }

            foreach(var method in this.Target.DeclaringType.Methods)
            {
                if (this.SourceWithRoot.Source.GetMethod != null &&
                    this.Target.GetMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.GetMethod, this.SourceWithRoot.RootContext))
                {
                    this.Target.GetMethod = method;
                }

                if (this.SourceWithRoot.Source.SetMethod != null &&
                    this.Target.SetMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.SetMethod, this.SourceWithRoot.RootContext))
                {
                    this.Target.SetMethod = method;
                }

                for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.SourceWithRoot.Source.OtherMethods[i], this.SourceWithRoot.RootContext))
                    {
                        this.Target.OtherMethods[i] = method;
                    }
                }
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.SourceWithRoot.Source, this.SourceWithRoot.RootContext);

            this.IsStructureCloned = true;

            Contract.Assert(this.Target.SignatureEquals(this.SourceWithRoot.Source));
            Contract.Assert((this.Target.GetMethod == null) == (this.SourceWithRoot.Source.GetMethod == null));
            Contract.Assert((this.Target.SetMethod == null) == (this.SourceWithRoot.Source.SetMethod == null));
            for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.SourceWithRoot.Source.OtherMethods[i] == null));
            }
        }
    }
}
