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
    internal class ExceptionHandlerCloner : ClonerBase<ExceptionHandler>
    {
        /// <summary>
        /// Creates a new <see cref="ExceptionHandlerCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the method body that contains this exception handler.</param>
        /// <param name="source">Cloning source.</param>
        public ExceptionHandlerCloner(MethodBodyCloner parent, ExceptionHandler source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);

            this.Parent = parent;
            this.Parent.ExceptionHandlerCloners.Add(this);
        }

        /// <summary>
        /// Gets or sets the cloner for the method body containing the exception handler being cloned.
        /// </summary>
        public MethodBodyCloner Parent { get; private set; }

        /// <summary>
        /// Creates the target exception handler.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override ExceptionHandler CreateTarget()
        {
            var target = new ExceptionHandler(this.Source.HandlerType);
            this.Parent.Target.ExceptionHandlers.Add(target);
            return target;
        }

        /// <summary>
        /// Clones the exception handler in its entirety.
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Target.HandlerType == this.Source.HandlerType);

            this.Target.CatchType = this.ILCloningContext.RootImport(this.Source.CatchType);

            var methodContext = new MethodContext(this.Parent);
            this.Target.FilterStart = methodContext.RootImport(this.Source.FilterStart);
            this.Target.HandlerEnd = methodContext.RootImport(this.Source.HandlerEnd);
            this.Target.HandlerStart = methodContext.RootImport(this.Source.HandlerStart);
            this.Target.TryEnd = methodContext.RootImport(this.Source.TryEnd);
            this.Target.TryStart = methodContext.RootImport(this.Source.TryStart);

            this.IsCloned = true;
        }
    }
}
