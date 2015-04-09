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
    /// Clones <see cref="FieldDefinition"/> contents from a source to a target.
    /// </summary>
    internal class EventCloner : OldClonerBase<EventDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="EventCloner"/>
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public EventCloner(ILCloningContext ilCloningContext, EventDefinition source, EventDefinition target)
            : base(ilCloningContext, source, target)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
        }

        /// <summary>
        /// Clones the event in its entirety
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Name);

            this.Target.Attributes = this.Source.Attributes;
            this.Target.EventType = this.ILCloningContext.RootImport(this.Source.EventType);

            if (this.Source.AddMethod != null)
            {
                var targetAddMethod = this.ILCloningContext.RootImport(this.Source.AddMethod) as MethodDefinition;
                Contract.Assert(targetAddMethod != null); // this should always be the case because the method is internal to the target
                this.Target.AddMethod = targetAddMethod;
            }

            if (this.Source.RemoveMethod != null)
            {
                var targetRemoveMethod = this.ILCloningContext.RootImport(this.Source.RemoveMethod) as MethodDefinition;
                Contract.Assert(targetRemoveMethod != null); // this should always be the case because the method is internal to the target
                this.Target.RemoveMethod = targetRemoveMethod;
            }

            if (this.Source.InvokeMethod != null)
            {
                var targetInvokeMethod = this.ILCloningContext.RootImport(this.Source.InvokeMethod) as MethodDefinition;
                Contract.Assert(targetInvokeMethod != null); // this should always be the case because the method is internal to the target
                this.Target.InvokeMethod = targetInvokeMethod;
            }

            foreach (var sourceOtherMethod in this.Source.OtherMethods)
            {
                var targetOtherMethod = this.ILCloningContext.RootImport(sourceOtherMethod) as MethodDefinition;
                Contract.Assert(targetOtherMethod != null); // this should always be the case because the method is internal to the target
                this.Target.OtherMethods.Add(targetOtherMethod);
            }

            this.Target.CloneAllCustomAttributes(this.Source, this.ILCloningContext);

            this.IsCloned = true;

            Contract.Assert((this.Target.AddMethod == null) == (this.Source.AddMethod == null));
            Contract.Assert((this.Target.RemoveMethod == null) == (this.Source.RemoveMethod == null));
            Contract.Assert((this.Target.InvokeMethod == null) == (this.Source.InvokeMethod == null));
            for (int i = 0; i < this.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.Source.OtherMethods[i] == null));
            }
        }
    }
}
