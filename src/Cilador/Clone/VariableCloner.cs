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

using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Clone
{
    /// <summary>
    /// Clones a source variable to a target variable
    /// </summary>
    internal class VariableCloner : ClonerBase<VariableDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="VariableCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner for the method body that contains the variable being cloned.</param>
        /// <param name="previous">Cloner for the variable, if any, for the variable that comes before the variable being cloned.</param>
        /// <param name="source">Cloning source.</param>
        public VariableCloner(ICloneToMethodBody<object> parent, VariableCloner previous, VariableDefinition source)
            : base(parent.CloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.CloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
            this.Previous = previous;
        }

        /// <summary>
        /// Gets or sets the cloner for the method body that contains the variable being cloned.
        /// </summary>
        private ICloneToMethodBody<object> Parent { get; set; }

        /// <summary>
        /// Gets or sets the cloner for the variable, if any, for the variable that comes before the variable being cloned.
        /// </summary>
        private VariableCloner Previous { get; set; }

        /// <summary>
        /// Creates the target variable.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override VariableDefinition GetTarget()
        {
            // order matters for variables, so ensure the previous target has been created
            if (this.Previous != null) { this.Previous.EnsureTargetIsSet(); }

            // now create the new variable
            var voidTypeReference = this.CloningContext.RootTarget.Module.Import(typeof(void));
            var target = new VariableDefinition(this.Source.Name, voidTypeReference);
            this.Parent.Target.Variables.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the variable.
        /// </summary>
        protected override void DoClone()
        {
            this.Target.VariableType = this.CloningContext.RootImport(this.Source.VariableType);
        }
    }
}
