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

using Cilador.Graph.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Clone
{
    /// <summary>
    /// Contracts for <see cref="ICloningContext"/>
    /// </summary>
    [ContractClassFor(typeof(ICloningContext))]
    internal abstract class CloningContextContract : ICloningContext
    {
        public ICilGraph CilGraph
        {
            get
            {
                Contract.Ensures(Contract.Result<ICilGraph>() != null);
                throw new NotSupportedException();
            }
        }


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
