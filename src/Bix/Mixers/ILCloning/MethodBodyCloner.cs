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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones a method body from a source to a target.
    /// </summary>
    internal class MethodBodyCloner : ClonerBase<MethodBody>
    {
        /// <summary>
        /// Creates a new <see cref="MethodBodyCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the method signature to which this method body is attached.</param>
        /// <param name="source">Resolved cloning source.</param>
        public MethodBodyCloner(MethodSignatureCloner parent, MethodBody source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.VariableCloners != null);
            Contract.Ensures(this.InstructionCloners != null);
            Contract.Ensures(this.ExceptionHandlerCloners != null);

            this.Parent = parent;
            this.VariableCloners = new List<VariableCloner>();
            this.InstructionCloners = new List<InstructionCloner>();
            this.ExceptionHandlerCloners = new List<ExceptionHandlerCloner>();
        }

        /// <summary>
        /// Gets or sets the method signature cloner with which this method body cloner is associated
        /// </summary>
        public MethodSignatureCloner Parent { get; private set; }

        /// <summary>
        /// Gets or sets the collection of variable cloners for the method body.
        /// </summary>
        public List<VariableCloner> VariableCloners { get; private set; }

        /// <summary>
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        public List<InstructionCloner> InstructionCloners { get; private set; }

        /// <summary>
        /// Gets or sets the collection of exception handler cloners for the method body.
        /// </summary>
        public List<ExceptionHandlerCloner> ExceptionHandlerCloners { get; private set; }

        /// <summary>
        /// The method body already exists, attached to the parent cloner.
        /// </summary>
        /// <returns>Parent cloner's target body.</returns>
        protected override MethodBody CreateTarget()
        {
            return this.Parent.Target.Body;
        }

        /// <summary>
        /// Clones the method body from the source to the target.
        /// </summary>
        public override void Clone()
        {
            this.Target.InitLocals = this.Source.InitLocals;

            this.Target.MaxStackSize = this.Source.MaxStackSize;

            if (this.Source.Scope != null)
            {
                // TODO method body scope may be tough to get right
                // for now raise an exception
                throw new NotSupportedException("An unsupported configuration was detected. Please consider filing a bug report on the project's github page.");
            }

            this.IsCloned = true;
        }
    }
}
