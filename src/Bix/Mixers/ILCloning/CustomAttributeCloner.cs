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

using Bix.Mixers.Core;
using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones a custom attribute.
    /// </summary>
    internal class CustomAttributeCloner : ClonerBase<CustomAttribute>
    {
        /// <summary>
        /// Creates a new <see cref="ParameterCloner"/>.
        /// </summary>
        /// <param name="genericParameterProvider">Cloner for the member that the custom attribute is attached to.</param>
        /// <param name="source">Cloning source.</param>
        public CustomAttributeCloner(
            LazyClonerBase<ICustomAttributeProvider> parent,
            CustomAttribute source,
            Func<CustomAttribute> targetGetter,
            Action<CustomAttribute> targetSetter)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the cloner for the class/method/etc that the custom attribute is attached to.
        /// </summary>
        public LazyClonerBase<ICustomAttributeProvider> Parent { get; private set; }

        /// <summary>
        /// Creates the target custom attribute.
        /// </summary>
        /// <returns>Custom attribute to attach to the target provider.</returns>
        protected override CustomAttribute CreateTarget()
        {
            var target = new CustomAttribute(this.ILCloningContext.RootImport(this.Source.Constructor));
            this.Parent.Target.CustomAttributes.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the custom attribute
        /// </summary>
        public override void Clone()
        {
            var source = this.Source;
            var target = this.Target;

            // TODO what is the blob argument for custom attributes?
            if (source.HasConstructorArguments)
            {
                foreach (var sourceArgument in source.ConstructorArguments)
                {
                    target.ConstructorArguments.Add(
                        new CustomAttributeArgument(
                            this.ILCloningContext.RootImport(sourceArgument.Type),
                            this.ILCloningContext.DynamicRootImport(sourceArgument.Value)));
                }
            }

            if (source.HasProperties)
            {
                foreach (var sourceProperty in source.Properties)
                {
                    target.Properties.Add(
                        new CustomAttributeNamedArgument(
                            sourceProperty.Name,
                            new CustomAttributeArgument(
                                this.ILCloningContext.RootImport(sourceProperty.Argument.Type),
                                this.ILCloningContext.DynamicRootImport(sourceProperty.Argument.Value))));
                }
            }

            this.IsCloned = true;
        }
    }
}
