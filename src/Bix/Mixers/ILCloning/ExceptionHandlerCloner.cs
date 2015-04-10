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
    /// Clones <see cref="ExceptionHandler"/> contents from a source to a target.
    /// </summary>
    internal class ExceptionHandlerCloner : OldClonerBase<ExceptionHandler>
    {
        /// <summary>
        /// Creates a new <see cref="ExceptionHandlerCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the method body that contains this exception handler.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public ExceptionHandlerCloner(MethodBodyCloner parent, ExceptionHandler source, ExceptionHandler target)
            : this(new MethodContext(parent), source, target)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);

            parent.ExceptionHandlerCloners.Add(this);
        }

        /// <summary>
        /// Creates a new <see cref="ExceptionHandlerCloner"/>
        /// </summary>
        /// <param name="methodContext">Method context for this cloner.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public ExceptionHandlerCloner(MethodContext methodContext, ExceptionHandler source, ExceptionHandler target)
            : base(methodContext.ILCloningContext, source, target)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(methodContext.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.MethodContext != null);

            this.MethodContext = methodContext;
        }

        /// <summary>
        /// Gets or sets the context for the method associated with this cloner.
        /// </summary>
        public MethodContext MethodContext { get; private set; }

        /// <summary>
        /// Clones the exception handler in its entirety.
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Target.HandlerType == this.Source.HandlerType);

            this.Target.CatchType = this.ILCloningContext.RootImport(this.Source.CatchType);
            this.Target.FilterStart = this.MethodContext.RootImport(this.Source.FilterStart);
            this.Target.HandlerEnd = this.MethodContext.RootImport(this.Source.HandlerEnd);
            this.Target.HandlerStart = this.MethodContext.RootImport(this.Source.HandlerStart);
            this.Target.TryEnd = this.MethodContext.RootImport(this.Source.TryEnd);
            this.Target.TryStart = this.MethodContext.RootImport(this.Source.TryStart);

            this.IsCloned = true;
        }
    }
}
