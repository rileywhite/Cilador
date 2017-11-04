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
    /// Clones a custom attribute.
    /// </summary>
    internal class CustomAttributeCloner : ClonerBase<CustomAttribute>
    {
        /// <summary>
        /// Creates a new <see cref="ParameterCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the member that the custom attribute is attached to.</param>
        /// <param name="source">Cloning source.</param>
        public CustomAttributeCloner(
            ICloner<ICustomAttributeProvider> parent,
            CustomAttribute source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the cloner for the class/method/etc that the custom attribute is attached to.
        /// </summary>
        public ICloner<ICustomAttributeProvider> Parent { get; private set; }

        /// <summary>
        /// Creates the target custom attribute.
        /// </summary>
        /// <returns>Custom attribute to attach to the target provider.</returns>
        protected override CustomAttribute GetTarget()
        {
            var target = new CustomAttribute(this.CloningContext.RootImport(this.Source.Constructor));
            this.Parent.Target.CustomAttributes.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the custom attribute
        /// </summary>
        protected override void DoClone()
        {
            var source = this.Source;
            var target = this.Target;

            // TODO what is the blob argument for custom attributes?
            if (source.HasConstructorArguments)
            {
                // Arguments are structs rather than classes which makes cloners 
                foreach (var sourceArgument in source.ConstructorArguments)
                {
                    target.ConstructorArguments.Add(
                        new CustomAttributeArgument(
                            this.CloningContext.RootImport(sourceArgument.Type),
                            this.CloningContext.DynamicRootImport(sourceArgument.Value)));
                }
            }

            if (source.HasProperties)
            {
                // Arguments are structs rather than classes which makes cloners 
                foreach (var sourceProperty in source.Properties)
                {
                    target.Properties.Add(
                        new CustomAttributeNamedArgument(
                            sourceProperty.Name,
                            new CustomAttributeArgument(
                                this.CloningContext.RootImport(sourceProperty.Argument.Type),
                                this.CloningContext.DynamicRootImport(sourceProperty.Argument.Value))));
                }
            }

            if (source.HasFields)
            {
                // Arguments are structs rather than classes which makes cloners 
                foreach (var sourceField in source.Fields)
                {
                    target.Fields.Add(
                        new CustomAttributeNamedArgument(
                            sourceField.Name,
                            new CustomAttributeArgument(
                                this.CloningContext.RootImport(sourceField.Argument.Type),
                                this.CloningContext.DynamicRootImport(sourceField.Argument.Value))));
                }
            }
        }
    }
}
