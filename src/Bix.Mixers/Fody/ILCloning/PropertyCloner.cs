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
    /// <summary>
    /// Clones <see cref="PropertyDefinition"/> contents from a source to a target.
    /// </summary>
    internal class PropertyCloner : MemberClonerBase<PropertyDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="PropertyCloner"/>
        /// </summary>
        /// <param name="rootContext">Root context for cloning.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public PropertyCloner(RootContext rootContext, PropertyDefinition target, PropertyDefinition source)
            : base(rootContext, target, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        /// <summary>
        /// Clones the property in its entirety
        /// </summary>
        public override void CloneStructure()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Name);

            this.Target.Attributes = this.Source.Attributes;
            this.Target.Constant = this.Source.Constant;
            this.Target.HasConstant = this.Source.HasConstant;
            this.Target.HasDefault = this.Source.HasDefault;
            this.Target.HasThis = this.Source.HasThis;
            this.Target.IsRuntimeSpecialName = this.Source.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.IsSpecialName;

            this.Target.PropertyType = this.RootContext.RootImport(this.Source.PropertyType);

            // TODO research correct usage of property MetadataToken
            //this.Target.MetadataToken = new MetadataToken(
            //    this.Source.MetadataToken.TokenType,
            //    this.Source.MetadataToken.RID);

            if (this.Source.HasParameters)
            {
                this.Target.Parameters.CloneAllParameters(this.Source.Parameters, this.RootContext);
            }

            for (int i = 0; i < this.Source.OtherMethods.Count; i++)
            {
                this.Target.OtherMethods.Add(null);
            }

            foreach(var method in this.Target.DeclaringType.Methods)
            {
                if (this.Source.GetMethod != null &&
                    this.Target.GetMethod == null &&
                    method.SignatureEquals(this.Source.GetMethod, this.RootContext))
                {
                    this.Target.GetMethod = method;
                }

                if (this.Source.SetMethod != null &&
                    this.Target.SetMethod == null &&
                    method.SignatureEquals(this.Source.SetMethod, this.RootContext))
                {
                    this.Target.SetMethod = method;
                }

                for (int i = 0; i < this.Source.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.Source.OtherMethods[i], this.RootContext))
                    {
                        this.Target.OtherMethods[i] = method;
                    }
                }
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.Source, this.RootContext);

            this.IsStructureCloned = true;

            Contract.Assert(this.Target.SignatureEquals(this.Source));
            Contract.Assert((this.Target.GetMethod == null) == (this.Source.GetMethod == null));
            Contract.Assert((this.Target.SetMethod == null) == (this.Source.SetMethod == null));
            for (int i = 0; i < this.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.Source.OtherMethods[i] == null));
            }
        }
    }
}
