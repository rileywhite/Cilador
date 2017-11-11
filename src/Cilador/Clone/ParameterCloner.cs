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
    /// Clones a parameter
    /// </summary>
    internal class ParameterCloner : ClonerBase<ParameterDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="ParameterCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the signature associated with the method body being cloned.</param>
        /// <param name="previous">Cloner for the previous instruction, if any.</param>
        /// <param name="source">Cloning source.</param>
        public ParameterCloner(MethodSignatureCloner parent, ParameterCloner previous, ParameterDefinition source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
            this.Parent.ParameterCloners.Add(this);
            this.Previous = previous;
        }

        /// <summary>
        /// Gets or sets the method signature cloner assiciated with the parameter cloners
        /// </summary>
        public MethodSignatureCloner Parent { get; }

        /// <summary>
        /// Gets or sets the cloner for the previous instruction, if any.
        /// </summary>
        public ParameterCloner Previous { get; set; }

        /// <summary>
        /// Creates the target parameter definition.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override ParameterDefinition GetTarget()
        {
            // order matters for parameters, so make sure the previous has already been created before creating this one
            if (this.Previous != null) { this.Previous.EnsureTargetIsSet(); }

            // now create the target
            var voidReference = this.CloningContext.RootTarget.Module.Import(typeof(void));
            var target = new ParameterDefinition(
                this.Source.Name,
                this.Source.Attributes,
                voidReference);
            this.Parent.Target.Parameters.Add(target);

            return target;
        }

        /// <summary>
        /// Clones the parameter
        /// </summary>
        protected override void DoClone()
        {
            Contract.Assert(this.Source.Name == this.Target.Name);
            Contract.Assert(this.Source.Attributes == this.Target.Attributes);

            this.Target.ParameterType = this.CloningContext.RootImport(this.Source.ParameterType);
            this.Target.Constant = this.Source.Constant;
            this.Target.HasConstant = this.Source.HasConstant;
            this.Target.HasDefault = this.Source.HasDefault;
            this.Target.HasFieldMarshal = this.Source.HasFieldMarshal;
            this.Target.IsIn = this.Source.IsIn;
            this.Target.IsLcid = this.Source.IsLcid;
            this.Target.IsOptional = this.Source.IsOptional;
            this.Target.IsOut = this.Source.IsOut;
            this.Target.IsReturnValue = this.Source.IsReturnValue;

            // TODO research correct usage
            if (this.Source.MarshalInfo != null)
            {
                this.Target.MarshalInfo = new MarshalInfo(this.Source.MarshalInfo.NativeType);
            }
        }
    }
}
