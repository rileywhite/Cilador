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

using Mono.Cecil;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Clones <see cref="FieldDefinition"/> contents from a source to a target.
    /// </summary>
    internal class EventCloner : MemberClonerBase<EventDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="EventCloner"/>
        /// </summary>
        /// <param name="rootContext">Root context for cloning.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public EventCloner(RootContext rootContext, EventDefinition target, EventDefinition source)
            : base(rootContext, target, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        /// <summary>
        /// Clones the event in its entirety
        /// </summary>
        public override void CloneStructure()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Name);

            this.Target.Attributes = this.Source.Attributes;
            this.Target.EventType = this.RootContext.RootImport(this.Source.EventType);

            // TODO reseach correct usage of event MetadataToken
            //this.Target.MetadataToken = new MetadataToken(
            //    this.Source.MetadataToken.TokenType,
            //    this.Source.MetadataToken.RID);


            foreach (var method in this.Target.DeclaringType.Methods)
            {
                if (this.Source.AddMethod != null &&
                    this.Target.AddMethod == null &&
                    method.SignatureEquals(this.Source.AddMethod, this.RootContext))
                {
                    this.Target.AddMethod = method;
                }

                if (this.Source.RemoveMethod != null &&
                    this.Target.RemoveMethod == null &&
                    method.SignatureEquals(this.Source.RemoveMethod, this.RootContext))
                {
                    this.Target.RemoveMethod = method;
                }

                if (this.Source.InvokeMethod != null &&
                    this.Target.InvokeMethod == null &&
                    method.SignatureEquals(this.Source.InvokeMethod, this.RootContext))
                {
                    this.Target.InvokeMethod = method;
                }

                for (int i = 0; i < this.Source.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.Source.OtherMethods[i], this.RootContext))
                    {
                        this.Target.OtherMethods[i] = method;
                    }
                }
            }

            // I did not check for a similar issue here as with the duplication in the FieldCloner...adding a clear line just to be safe
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.Source, this.RootContext);

            this.IsStructureCloned = true;

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
