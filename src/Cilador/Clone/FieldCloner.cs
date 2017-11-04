/***************************************************************************/
// Copyright 2013-2017 Riley White
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

namespace Cilador.Clone
{
    /// <summary>
    /// Clones <see cref="FieldDefinition"/> contents from a source to a target.
    /// </summary>
    internal class FieldCloner : ClonerBase<FieldDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="FieldCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the type that owns the field being cloned.</param>
        /// <param name="source">Cloning source.</param>
        public FieldCloner(ICloner<TypeDefinition> parent, FieldDefinition source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the cloner for the type that owns the field being cloned.
        /// </summary>
        private ICloner<TypeDefinition> Parent { get; set; }

        /// <summary>
        /// Creates the target.
        /// </summary>
        /// <returns></returns>
        protected override FieldDefinition GetTarget()
        {
            var voidReference = this.CloningContext.RootTarget.Module.Import(typeof(void));
            var targetField = new FieldDefinition(this.Source.Name, 0, voidReference);
            this.Parent.Target.Fields.Add(targetField);
            return targetField;
        }

        /// <summary>
        /// Clones the field in its entirety
        /// </summary>
        protected override void DoClone()
        {
            this.Target.Attributes = this.Source.Attributes;
            this.Target.Constant = this.Source.Constant;
            this.Target.HasConstant = this.Source.HasConstant;
            this.Target.Offset = this.Source.Offset;

            // TODO research correct usage of field MarshalInfo
            this.Target.MarshalInfo = this.Source.MarshalInfo == null ? null : new MarshalInfo(this.Source.MarshalInfo.NativeType);

            // TODO research correct usage of field InitialValue
            if (this.Source.InitialValue != null)
            {
                var initialValue = new byte[this.Source.InitialValue.LongLength];
                this.Source.InitialValue.CopyTo(initialValue, 0);
                this.Target.InitialValue = initialValue;
            }

            this.Target.FieldType = this.CloningContext.RootImport(this.Source.FieldType);
        }
    }
}
