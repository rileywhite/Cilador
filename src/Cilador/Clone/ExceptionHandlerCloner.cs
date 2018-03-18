/***************************************************************************/
// Copyright 2013-2018 Riley White
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
    /// Clones <see cref="ExceptionHandler"/> contents from a source to a target.
    /// </summary>
    internal class ExceptionHandlerCloner : ClonerBase<ExceptionHandler>
    {
        /// <summary>
        /// Creates a new <see cref="ExceptionHandlerCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the method body that contains this exception handler.</param>
        /// <param name="previous">Cloner of the previous exception handler, if any.</param>
        /// <param name="source">Cloning source.</param>
        public ExceptionHandlerCloner(ICloneToMethodBody<object> parent, ExceptionHandlerCloner previous, ExceptionHandler source)
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
        /// Gets or sets the cloner for the method body containing the exception handler being cloned.
        /// </summary>
        public ICloneToMethodBody<object> Parent { get; }

        /// <summary>
        /// Gets or sets the cloner for the previous exception handler, if any.
        /// </summary>
        public ExceptionHandlerCloner Previous { get; }

        /// <summary>
        /// Creates the target exception handler.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override ExceptionHandler GetTarget()
        {
            // order of cration matters here, so ensure the previous target is created before continuing
            var previous = this.Previous;
            if (previous != null) { this.Previous.EnsureTargetIsSet(); }

            // now create the target
            var target = new ExceptionHandler(this.Source.HandlerType);
            this.Parent.Target.ExceptionHandlers.Add(target);

            return target;
        }

        /// <summary>
        /// Clones the exception handler in its entirety.
        /// </summary>
        protected override void DoClone()
        {
            Contract.Assert(this.Target.HandlerType == this.Source.HandlerType);

            this.Target.CatchType = this.CloningContext.RootImport(this.Source.CatchType);
            this.Target.FilterStart = this.CloningContext.RootImport(this.Source.FilterStart);
            this.Target.HandlerEnd = this.CloningContext.RootImport(this.Source.HandlerEnd);
            this.Target.HandlerStart = this.CloningContext.RootImport(this.Source.HandlerStart);
            this.Target.TryEnd = this.CloningContext.RootImport(this.Source.TryEnd);
            this.Target.TryStart = this.CloningContext.RootImport(this.Source.TryStart);
        }
    }
}
