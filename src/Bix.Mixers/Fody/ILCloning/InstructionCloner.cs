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
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Clones a source instruction to a target instruction
    /// </summary>
    internal class InstructionCloner : ClonerBase<Instruction>
    {
        /// <summary>
        /// Creates a new <see cref="VariableCloner"/>.
        /// </summary>
        /// <param name="methodBodyCloner">Method body cloner associated with new instruction cloner.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public InstructionCloner(MethodBodyCloner methodBodyCloner, Instruction target, Instruction source)
            : base(methodBodyCloner.ILCloningContext, target, source)
        {
            Contract.Requires(methodBodyCloner != null);
            Contract.Requires(methodBodyCloner.ILCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.MethodBodyCloner != null);

            this.MethodBodyCloner = methodBodyCloner;
        }

        /// <summary>
        /// Gets or sets the cloner for the method body associated with this variable cloner.
        /// </summary>
        public MethodBodyCloner MethodBodyCloner { get; private set; }

        /// <summary>
        /// Clones the instruction
        /// </summary>
        public override void Clone()
        {
            if (this.Source.Operand != null)
            {
                this.Target.Operand = this.ImportOperand((dynamic)this.Source.Operand);
            }

            this.Target.Offset = this.Source.Offset;

            this.IsCloned = true;
        }

        /// <summary>
        /// Catch-all method for dynamically dispatched instruction creation calls where the operand type is unrecognized
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="unsupportedOperand">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. This method is only invoked via dynamic dispatch when the operand is not recognized as a supported
        /// operand type. If the operand was recognized as a supported type, a different method would have been invoked.
        /// </exception>
        private object ImportOperand(object unsupportedOperand)
        {
            Contract.Requires(unsupportedOperand != null);

            throw new NotSupportedException(
                string.Format("Unsupported operand of type in instruction to be cloned: {0}", unsupportedOperand.GetType().FullName));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private byte ImportOperand(byte value)
        {
            return value;
        }

        /// <summary>
        /// Creates an instruction for invoking Calling <c>extern</c> methods using a <see cref="CallSite"/> operand.
        /// (Currently this is not supported.)
        /// </summary>
        /// <param name="callSite">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. Calling <c>extern</c> methods is not currently supported.
        /// </exception>
        private CallSite ImportOperand(CallSite callSite)
        {
            Contract.Requires(callSite != null);

            // TODO support extern methods
            throw new NotSupportedException(
                "Callsite instruction operands are used with the calli op code to make unmanaged method calls. This is not supported.");
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private double ImportOperand(double value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="field">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private FieldReference ImportOperand(FieldReference field)
        {
            Contract.Requires(field != null);
            return this.ILCloningContext.RootImport(field);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private float ImportOperand(float value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private Instruction ImportOperand(Instruction instruction)
        {
            Contract.Requires(instruction != null);

            var instructionCloner = this.MethodBodyCloner.InstructionCloners.FirstOrDefault(cloner => cloner.Source == instruction);
            if (instructionCloner == null)
            {
                throw new InvalidOperationException("Could not locate a variable for copying an instruction");
            }
            return instructionCloner.Target;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="instructions">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private Instruction[] ImportOperand(Instruction[] instructions)
        {
            Contract.Requires(instructions != null);

            var importedTargets = new Instruction[instructions.Length];
            for (int i = 0; i < instructions.Length; i++)
            {
                importedTargets[i] = this.ImportOperand(instructions[i]);
            }
            return importedTargets;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private int ImportOperand(int value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private long ImportOperand(long value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="method">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private MethodReference ImportOperand(MethodReference method)
        {
            Contract.Requires(method != null);
            return this.ILCloningContext.RootImport(method);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private ParameterDefinition ImportOperand(ParameterDefinition parameter)
        {
            Contract.Requires(parameter != null);

            if (parameter == this.MethodBodyCloner.Source.ThisParameter)
            {
                return this.MethodBodyCloner.Target.ThisParameter;
            }

            var parameterCloner =
                this.MethodBodyCloner.SignatureCloner.ParameterCloners.FirstOrDefault(cloner => cloner.Source == parameter);
            if (parameterCloner == null)
            {
                throw new InvalidOperationException("Failed to find a parameter cloner matching the operand in an instruction");
            }

            return parameterCloner.Target;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private sbyte ImportOperand(sbyte value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private string ImportOperand(string value)
        {
            Contract.Requires(value != null);
            return value;
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="type">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private TypeReference ImportOperand(TypeReference type)
        {
            Contract.Requires(type != null);
            return this.ILCloningContext.RootImport(type);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private VariableDefinition ImportOperand(VariableDefinition variable)
        {
            Contract.Requires(variable != null);

            var variableCloner = this.MethodBodyCloner.VariableCloners.FirstOrDefault(cloner => cloner.Source == variable);
            if (variableCloner == null)
            {
                throw new InvalidOperationException("Could not locate a variable for copying an instruction");
            }
            return variableCloner.Target;
        }
    }
}
