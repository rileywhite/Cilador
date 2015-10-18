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

using Cilador.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cilador.Graph
{
    /// <summary>
    /// Gets dependencies from instruction operands.
    /// </summary>
    internal class OperandDependenciesGetDispatcher : InstructionOperandFunctionDispatcherBase<IEnumerable<object>>
    {
        /// <summary>
        /// Gets an empty array for null operands.
        /// </summary>
        protected override IEnumerable<object> ReturnValueForNull
        {
            get { return new object[0]; }
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(TypeReference operand)
        {
            Contract.Requires(operand != null);
            return operand.GetDependencies();
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(FieldReference operand)
        {
            Contract.Requires(operand != null);
            return operand.GetDependencies();
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(Instruction operand)
        {
            Contract.Requires(operand != null);
            return new object[] { operand };
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(Instruction[] operand)
        {
            Contract.Requires(operand != null);
            return operand;
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(MethodReference operand)
        {
            Contract.Requires(operand != null);
            return operand.GetDependencies();
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(ParameterDefinition operand)
        {
            Contract.Requires(operand != null);
            return new object[] { operand };
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(VariableDefinition operand)
        {
            Contract.Requires(operand != null);
            return new object[] { operand };
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(byte operand)
        {
            return new object[0];
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(sbyte operand)
        {
            return new object[0];
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(float operand)
        {
            return new object[0];
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(double operand)
        {
            return new object[0];
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(int operand)
        {
            return new object[0];
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(long operand)
        {
            return new object[0];
        }

        /// <summary>
        /// Finds dependencies based on an operand.
        /// </summary>
        /// <param name="operand">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        protected override IEnumerable<object> InvokeForOperand(string operand)
        {
            return new object[0];
        }
    }
}
