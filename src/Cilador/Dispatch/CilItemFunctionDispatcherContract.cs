/***************************************************************************/
// Copyright 2013-2017 Riley White
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
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Dispatch
{
    /// <summary>
    /// Contracts for <see cref="CilItemFunctionDispatcherBase{TReturn}"/>.
    /// </summary>
    [ContractClassFor(typeof(CilItemFunctionDispatcherBase<>))]
    internal abstract class CilItemFunctionDispatcherContract<TReturn> : CilItemFunctionDispatcherBase<TReturn>
    {
        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForNull()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForNonILItem(object item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(AssemblyDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(ModuleDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(TypeDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(FieldDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(MethodDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(MethodBody item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(PropertyDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(EventDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(GenericParameter item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(ParameterDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(MethodReturnType item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(CustomAttribute item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(VariableDefinition item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(Instruction item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contracts for method implementers.
        /// </summary>
        protected override TReturn InvokeForItem(ExceptionHandler item)
        {
            Contract.Requires(item != null);
            throw new NotSupportedException();
        }
    }
}
