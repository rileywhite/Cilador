/***************************************************************************/
// Copyright 2013-2016 Riley White
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
    /// Defines the code contracts for <see cref="InstructionOperandFunctionDispatcherBase{TReturn, TState}"/>.
    /// </summary>
    [ContractClassFor(typeof(InstructionOperandFunctionDispatcherBase<,>))]
    internal abstract class InstructionOperandFunctionDispatcherContract<TReturn, TState>
        : InstructionOperandFunctionDispatcherBase<TReturn, TState>
    {
        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(TypeReference operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(FieldReference operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(Instruction operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(Instruction[] operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(MethodReference operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(ParameterDefinition operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(VariableDefinition operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(byte operand, OpCode opCode, TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(sbyte operand, OpCode opCode, TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(float operand, OpCode opCode, TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(double operand, OpCode opCode, TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(int operand, OpCode opCode, TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(long operand, OpCode opCode, TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contract for the overridden method.
        /// </summary>
        protected override TReturn InvokeForOperand(string operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);
            throw new NotImplementedException();
        }
    }
}
