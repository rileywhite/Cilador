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
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones a source variable to a target variable
    /// </summary>
    internal class VariableCloner : OldClonerBase<VariableDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="VariableCloner"/>.
        /// </summary>
        /// <param name="ilCloningContext">Cloner for the method body that contains the variable being cloned</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public VariableCloner(MethodBodyCloner parent, VariableDefinition source, VariableDefinition target)
            : this(parent.ILCloningContext, source, target)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
            this.Parent.VariableCloners.Add(this);
        }

        /// <summary>
        /// Creates a new <see cref="VariableCloner"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public VariableCloner(IILCloningContext ilCloningContext, VariableDefinition source, VariableDefinition target)
            : base(ilCloningContext, source, target)
        {
            // TODO get rid of this constructor...somehow...? it's used in constructor broadcasting
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
        }

        /// <summary>
        /// Gets or sets the cloner for the method body that contains the variable being cloned.
        /// </summary>
        private MethodBodyCloner Parent { get; set; }

        /// <summary>
        /// Clones the variable
        /// </summary>
        public override void Clone()
        {
            this.Target.VariableType = this.ILCloningContext.RootImport(this.Source.VariableType);
            this.IsCloned = true;
        }
    }
}
