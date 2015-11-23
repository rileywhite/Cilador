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
    internal class MethodReturnTypeCloner : ClonerBase<MethodReturnType>
    {
        /// <summary>
        /// Clones a method return type.
        /// </summary>
        /// <param name="parent">Cloner for the method signature containing the method return type to be cloned.</param>
        /// <param name="source">Cloning source.</param>
        public MethodReturnTypeCloner(ICloner<object, MethodDefinition> parent, MethodReturnType source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the cloner for the method signature containing the method return type to be cloned.
        /// </summary>
        public ICloner<object, MethodDefinition> Parent { get; set; }

        /// <summary>
        /// Creates the target method return type.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override MethodReturnType GetTarget()
        {
            return this.Parent.Target.MethodReturnType;
        }

        /// <summary>
        /// Clones the source to the target.
        /// </summary>
        protected override void DoClone()
        {
            var source = this.Source;
            var target = this.Target;

            target.ReturnType = this.ILCloningContext.RootImport(source.ReturnType);
            target.Attributes = source.Attributes;
            target.Constant = source.Constant;
            target.HasConstant = source.HasConstant;

            // TODO research correct usage of MethodReturnType.MarshalInfo
            if (source.MarshalInfo != null)
            {
                target.MarshalInfo = new MarshalInfo(source.MarshalInfo.NativeType);
            }
        }
    }
}
