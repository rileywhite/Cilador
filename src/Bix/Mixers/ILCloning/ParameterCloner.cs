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

using System;
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones a parameter
    /// </summary>
    internal class ParameterCloner : OldClonerBase<ParameterDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="ParameterCloner"/>.
        /// </summary>
        /// <param name="methodSignatureCloner">Cloner for the signature associated with the method body being cloned.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public ParameterCloner(MethodSignatureCloner methodSignatureCloner, ParameterDefinition source, ParameterDefinition target)
            : base(methodSignatureCloner.ILCloningContext, source, target)
        {
            Contract.Requires(methodSignatureCloner != null);
            Contract.Requires(methodSignatureCloner.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.MethodSignatureCloner != null);

            this.MethodSignatureCloner = methodSignatureCloner;
        }

        /// <summary>
        /// Gets or sets the method signature cloner assiciated with the parameter cloners
        /// </summary>
        public MethodSignatureCloner MethodSignatureCloner { get; private set; }

        /// <summary>
        /// Clones the parameter
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Source.Name == this.Target.Name);
            Contract.Assert(this.Source.Attributes == this.Target.Attributes);

            this.Target.ParameterType = this.ILCloningContext.RootImport(this.Source.ParameterType);
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

            this.Target.CloneAllCustomAttributes(this.Source, this.ILCloningContext);

            this.IsCloned = true;
        }
    }
}
