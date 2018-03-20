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

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Dispatch
{
    /// <summary>
    /// Base operand for invoking behavior for all possible types of operand operands
    /// with a breakout for important CIL types
    /// </summary>
    /// <typeparam name="TReturn">Type of the return value for each operation.</typeparam>
    /// <typeparam name="TState">Type of the ptional state to use within dispatching functionality.</typeparam>
    [ContractClass(typeof(InstructionOperandFunctionDispatcherContract<,>))]
    public abstract class InstructionOperandFunctionDispatcherBase<TReturn, TState>
    {
        /// <summary>
        /// Invokes the bahvior for an instruction's operand.
        /// </summary>
        /// <param name="instruction">Instruction whose operand should be looked at.</param>
        /// <param name="state">Optional state to pass through to invocations.</param>
        /// <returns>Result of the invocation.</returns>
        public TReturn InvokeFor(Instruction instruction, TState state)
        {
            Contract.Requires(instruction != null);

            return instruction.Operand == null ?
                this.GetReturnValueForNull(instruction.OpCode, state) :
                InvokeForOperand((dynamic)instruction.Operand, instruction.OpCode, state);
        }

        /// <summary>
        /// Gets the return value for a null operand. Unless overridden, this is
        /// <c>default(<typeparamref name="TState"/>)</c>.
        /// </summary>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected virtual TReturn GetReturnValueForNull(OpCode opCode, TState state)
        {
            return default(TReturn);
        }

        /// <summary>
        /// This is the catch-all behavior.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        private TReturn InvokeForOperand(object operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);

            throw new NotSupportedException(
                $"Unsupported operand type for instruction operand dispatching: {operand.GetType().FullName}");
        }

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Nothing. This operand always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. Calling <c>extern</c> methods is not currently supported.
        /// </exception>
        protected TReturn InvokeForOperand(CallSite operand, OpCode opCode, TState state)
        {
            Contract.Requires(operand != null);

            // TODO support extern methods
            throw new NotSupportedException(
                "Callsite instruction operands are used with the calli op code to make unmanaged operand calls. This is not supported.");
        }

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(TypeReference operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(FieldReference operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(Instruction operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(Instruction[] operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(MethodReference operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(ParameterDefinition operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(VariableDefinition operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(byte operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(sbyte operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(float operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(double operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(int operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(long operand, OpCode opCode, TState state);

        /// <summary>
        /// Invokes behavior based on an operand, op code, and state.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode"><see cref="OpCode"/> for the instruction.</param>
        /// <param name="state">Optional state to use within the invocation.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(string operand, OpCode opCode, TState state);
    }

    /// <summary>
    /// Simplified version of <see cref="InstructionOperandFunctionDispatcherBase{TReturn, TState}"/>
    /// for operations that care only about the operand itself.
    /// </summary>
    /// <typeparam name="TReturn"></typeparam>
    public abstract class InstructionOperandFunctionDispatcherBase<TReturn>
        : InstructionOperandFunctionDispatcherBase<TReturn, object>
    {
        /// <summary>
        /// Invokes the bahvior for an instruction's operand.
        /// </summary>
        /// <param name="instruction">Instruction whose operand should be looked at.</param>
        /// <returns>Result of the funct</returns>
        public TReturn InvokeFor(Instruction instruction)
        {
            Contract.Requires(instruction != null);

            if (instruction.Operand == null) { return this.ReturnValueForNull; }
            return InvokeForOperand((dynamic)instruction.Operand);
        }

        /// <summary>
        /// Gets the return value for a null operand. Unless overridden, this is
        /// <c>default(TReturn)</c>.
        /// </summary>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation for a null operand.</returns>
        protected sealed override TReturn GetReturnValueForNull(OpCode opCode, object state)
        {
            return this.ReturnValueForNull;
        }

        /// <summary>
        /// Gets the return value for a null operand. Unless overridden, this is
        /// <c>default(TReturn)</c>.
        /// </summary>
        protected virtual TReturn ReturnValueForNull => default(TReturn);

        /// <summary>
        /// Catch-all method to catch invalid operands.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        private TReturn InvokeForOperand(object operand)
        {
            Contract.Requires(operand != null);

            throw new NotSupportedException(
                $"Unsupported operand type for instruction operand dispatching: {operand.GetType().FullName}");
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(TypeReference operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(TypeReference operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(FieldReference operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(FieldReference operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(Instruction operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(Instruction operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(Instruction[] operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(Instruction[] operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(MethodReference operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(MethodReference operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(ParameterDefinition operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(ParameterDefinition operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(VariableDefinition operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(VariableDefinition operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(byte operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(byte operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(sbyte operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(sbyte operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(float operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(float operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(double operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(double operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(int operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(int operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(long operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(long operand);

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <param name="opCode">Ignored.</param>
        /// <param name="state">Ignored.</param>
        /// <returns>Result of the invocation.</returns>
        protected sealed override TReturn InvokeForOperand(string operand, OpCode opCode, object state)
        {
            return this.InvokeForOperand(operand);
        }

        /// <summary>
        /// Invokes behavior based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForOperand(string operand);
    }
}
