/***************************************************************************/
// Copyright 2013-2014 Riley White
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Clones a source variable to a target variable
    /// </summary>
    internal class VariableCloner : ClonerBase<VariableDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="VariableCloner"/>.
        /// </summary>
        /// <param name="methodBodyCloner">Method body cloner associated with new variable cloner.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public VariableCloner(MethodBodyCloner methodBodyCloner, VariableDefinition target, VariableDefinition source)
            : base(methodBodyCloner.ILCloningContext, target, source)
        {
            Contract.Requires(methodBodyCloner != null);
            Contract.Requires(methodBodyCloner.ILCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.MethodBodyCloner != null);

            this.MethodBodyCloner = methodBodyCloner;
        }

        /// <summary>
        /// Gets or sets the cloner for the method body associated with this variable cloner.
        /// </summary>
        public MethodBodyCloner MethodBodyCloner { get; private set; }

        /// <summary>
        /// Clones the variable
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Target.Name == this.Source.Name);
            this.Target.VariableType = this.ILCloningContext.RootImport(this.Source.VariableType);
            this.IsCloned = true;
        }
    }
}
