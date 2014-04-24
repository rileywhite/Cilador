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
    internal abstract class MemberSourceWithRootBase<TMemberDefinition> : IRootImportProvider
        where TMemberDefinition : MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public MemberSourceWithRootBase(
            RootContext rootContext,
            TMemberDefinition source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.RootContext != null);
            Contract.Ensures(this.Source != null);

            this.RootContext = rootContext;
            this.Source = source;
        }

        public TMemberDefinition Source { get; private set; }

        public RootContext RootContext { get; protected set; }

        TypeDefinition IRootImportProvider.RootSource
        {
            get { return this.RootContext.RootSource; }
        }

        TypeDefinition IRootImportProvider.RootTarget
        {
            get { return this.RootContext.RootTarget; }
        }

        public TItem DynamicRootImport<TItem>(TItem sourceItem)
        {
            return this.RootContext.DynamicRootImport<TItem>(sourceItem);
        }

        public TypeReference RootImport(TypeReference sourceType)
        {
            return this.RootContext.RootImport(sourceType);
        }

        public FieldReference RootImport(FieldReference sourceField)
        {
            return this.RootContext.RootImport(sourceField);
        }

        public MethodReference RootImport(MethodReference sourceMethod)
        {
            return this.RootContext.RootImport(sourceMethod);
        }
    }
}
