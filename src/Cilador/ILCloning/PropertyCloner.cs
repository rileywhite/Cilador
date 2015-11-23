/***************************************************************************/
// Copyright 2013-2015 Riley White
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

using Cilador.Core;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Clones <see cref="PropertyDefinition"/> contents from a source to a target.
    /// </summary>
    internal class PropertyCloner : ClonerBase<PropertyDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="PropertyCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for type that contains the property being cloned.</param>
        /// <param name="source">Cloning source.</param>
        public PropertyCloner(ICloner<TypeDefinition> parent, PropertyDefinition source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the cloner for the type that contains the property being cloned.
        /// </summary>
        private ICloner<TypeDefinition> Parent { get; set; }

        /// <summary>
        /// Creates the target property.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override PropertyDefinition GetTarget()
        {
            var voidReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
            var targetProperty = new PropertyDefinition(this.Source.Name, 0, voidReference);
            this.Parent.Target.Properties.Add(targetProperty);
            return targetProperty;
        }

        /// <summary>
        /// Clones the property in its entirety
        /// </summary>
        protected override void DoClone()
        {
            Contract.Ensures(this.Target.Parameters.Count == this.Source.Parameters.Count);

            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Name);

            this.Target.Attributes = this.Source.Attributes;
            this.Target.Constant = this.Source.Constant;
            this.Target.HasConstant = this.Source.HasConstant;
            this.Target.HasDefault = this.Source.HasDefault;
            this.Target.HasThis = this.Source.HasThis;
            this.Target.IsRuntimeSpecialName = this.Source.IsRuntimeSpecialName;
            this.Target.IsSpecialName = this.Source.IsSpecialName;

            this.Target.PropertyType = this.ILCloningContext.RootImport(this.Source.PropertyType);

            // seting the getter and setter methods also sets the parameters for the property
            if (this.Source.GetMethod != null)
            {
                var targetGetMethod = this.ILCloningContext.RootImport(this.Source.GetMethod) as MethodDefinition;
                Contract.Assert(targetGetMethod != null); // this should always be the case because the method is internal to the target
                this.Target.GetMethod = targetGetMethod;
            }

            if (this.Source.SetMethod != null)
            {
                var targetSetMethod = this.ILCloningContext.RootImport(this.Source.SetMethod) as MethodDefinition;
                Contract.Assert(targetSetMethod != null); // this should always be the case because the method is internal to the target
                this.Target.SetMethod = targetSetMethod;
            }

            foreach (var sourceOtherMethod in this.Source.OtherMethods)
            {
                var targetOtherMethod = this.ILCloningContext.RootImport(sourceOtherMethod) as MethodDefinition;
                Contract.Assert(targetOtherMethod != null); // this should always be the case because the method is internal to the target
                this.Target.OtherMethods.Add(targetOtherMethod);
            }

            Contract.Assert((this.Target.GetMethod == null) == (this.Source.GetMethod == null));
            Contract.Assert((this.Target.SetMethod == null) == (this.Source.SetMethod == null));
            for (int i = 0; i < this.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.Source.OtherMethods[i] == null));
            }
        }
    }
}
