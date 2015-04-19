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
using Mono.Cecil.Cil;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Contracts for <see cref="IILCloningContext"/>
    /// </summary>
    [ContractClassFor(typeof(IILCloningContext))]
    internal abstract class ILCloningContextContract : IILCloningContext
    {
        public TypeDefinition RootSource
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeDefinition>() != null);
                throw new NotSupportedException();
            }
        }

        public TypeDefinition RootTarget
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeDefinition>() != null);
                throw new NotSupportedException();
            }
        }

        public TItem DynamicRootImport<TItem>(TItem item)
        {
            throw new NotSupportedException();
        }

        public TypeReference RootImport(TypeReference type)
        {
            throw new NotSupportedException();
        }

        public MethodReference RootImport(MethodReference method)
        {
            throw new NotSupportedException();
        }

        public FieldReference RootImport(FieldReference field)
        {
            throw new NotSupportedException();
        }

        public ParameterDefinition RootImport(ParameterDefinition parameter)
        {
            throw new NotSupportedException();
        }

        public VariableDefinition RootImport(VariableDefinition variable)
        {
            throw new NotSupportedException();
        }

        public Instruction RootImport(Instruction instruction)
        {
            throw new NotSupportedException();
        }
    }
}
