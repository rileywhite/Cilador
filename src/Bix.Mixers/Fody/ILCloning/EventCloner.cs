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
    internal class EventCloner : MemberClonerBase<EventDefinition, EventSourceWithRoot>
    {
        public EventCloner(EventDefinition target, EventSourceWithRoot source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void CloneStructure()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.SourceWithRoot.Source.Name);

            this.Target.Attributes = this.SourceWithRoot.Source.Attributes;
            this.Target.EventType = this.SourceWithRoot.RootImport(this.SourceWithRoot.Source.EventType);

            // TODO reseach correct usage of event MetadataToken
            this.Target.MetadataToken = new MetadataToken(
                this.SourceWithRoot.Source.MetadataToken.TokenType,
                this.SourceWithRoot.Source.MetadataToken.RID);


            foreach (var method in this.Target.DeclaringType.Methods)
            {
                if (this.SourceWithRoot.Source.AddMethod != null &&
                    this.Target.AddMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.AddMethod, this.SourceWithRoot.RootContext))
                {
                    this.Target.AddMethod = method;
                }

                if (this.SourceWithRoot.Source.RemoveMethod != null &&
                    this.Target.RemoveMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.RemoveMethod, this.SourceWithRoot.RootContext))
                {
                    this.Target.RemoveMethod = method;
                }

                if (this.SourceWithRoot.Source.InvokeMethod != null &&
                    this.Target.InvokeMethod == null &&
                    method.SignatureEquals(this.SourceWithRoot.Source.InvokeMethod, this.SourceWithRoot.RootContext))
                {
                    this.Target.InvokeMethod = method;
                }

                for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
                {
                    if (this.Target.OtherMethods[i] != null &&
                        this.Target.OtherMethods[i] == null &&
                        method.SignatureEquals(this.SourceWithRoot.Source.OtherMethods[i], this.SourceWithRoot.RootContext))
                    {
                        this.Target.OtherMethods[i] = method;
                    }
                }
            }

            // I did not check for a similar issue here as with the duplication in the FieldCloner...adding a clear line just to be safe
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.SourceWithRoot.Source, this.SourceWithRoot.RootContext);

            this.IsStructureCloned = true;

            Contract.Assert((this.Target.AddMethod == null) == (this.SourceWithRoot.Source.AddMethod == null));
            Contract.Assert((this.Target.RemoveMethod == null) == (this.SourceWithRoot.Source.RemoveMethod == null));
            Contract.Assert((this.Target.InvokeMethod == null) == (this.SourceWithRoot.Source.InvokeMethod == null));
            for (int i = 0; i < this.SourceWithRoot.Source.OtherMethods.Count; i++)
            {
                Contract.Assert((this.Target.OtherMethods[i] == null) == (this.SourceWithRoot.Source.OtherMethods[i] == null));
            }
        }
    }
}
