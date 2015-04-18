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

using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones the logic part of a source constructor into a new method body.
    /// </summary>
    internal class ConstructorLogicBodyCloner :
        ClonerBase<MultiplexedConstructor, MethodBody>,
        ICloneToMethodBody<MultiplexedConstructor>
    {
        /// <summary>
        /// Creates a new <see cref="ConstructorLogicBodyCloner"/>.
        /// </summary>
        /// <param name="parent">Cloner that creates the method signature for the logic part of the source constructor.</param>
        /// <param name="source"></param>
        public ConstructorLogicBodyCloner(ConstructorLogicSignatureCloner parent, MultiplexedConstructor source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(parent.Source != null);
            Contract.Requires(parent.Source.Constructor != null);
            Contract.Requires(parent.Source.Constructor.Body != null);
            Contract.Requires(parent.Source.Constructor.Body.ThisParameter != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.SourceThisParameter != null);

            this.Parent = parent;
            this.SourceThisParameter = parent.Source.Constructor.Body.ThisParameter;
        }

        /// <summary>
        /// Gets or sets the cloner that creates the method signature for the logic part of the source constructor.
        /// </summary>
        private ConstructorLogicSignatureCloner Parent { get; set; }

        /// <summary>
        /// Gets or sets the This parameter of the source constructor.
        /// </summary>
        public ParameterDefinition SourceThisParameter { get; private set; }

        private ILProcessor ilProcessor;
        /// <summary>
        /// Gets or sets the <see cref="ILProcessor"/> for accesing IL instructions.
        /// </summary>
        internal ILProcessor TargetILProcessor
        {
            get
            {
                this.EnsureTargetIsSet();
                return this.ilProcessor;
            }
            private set { this.ilProcessor = value; }
        }

        /// <summary>
        /// Creates the target.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override MethodBody GetTarget()
        {
            Contract.Ensures(this.TargetILProcessor != null);

            //var target = new MethodBody(this.Parent.Target);
            //this.Parent.Target.Body = target;
            var target = this.Parent.Target.Body;
            this.TargetILProcessor = target.GetILProcessor();
            return target;
        }

        /// <summary>
        /// Clones the constructor logic into a new method body.
        /// </summary>
        protected override void DoClone()
        {
            this.Target.InitLocals = this.Parent.Source.ConstructionVariables.Any();
        }
    }
}
