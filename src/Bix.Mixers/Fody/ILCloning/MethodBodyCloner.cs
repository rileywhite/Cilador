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
    /// Clones a method body from a source to a target.
    /// </summary>
    internal class MethodBodyCloner : ClonerBase<MethodBody>
    {
        /// <summary>
        /// Creates a new <see cref="MethodBodyCloner"/>
        /// </summary>
        /// <param name="signatureCloner">Cloner for the method signature to which this method body is attached.</param>
        /// <param name="target">Resolved cloning target.</param>
        /// <param name="source">Resolved cloning source.</param>
        public MethodBodyCloner(MethodSignatureCloner signatureCloner, MethodBody target, MethodBody source)
            : base(signatureCloner.ILCloningContext, target, source)
        {
            Contract.Requires(signatureCloner != null);
            Contract.Requires(signatureCloner.ILCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.SignatureCloner != null);
            Contract.Ensures(this.VariableCloners != null);

            this.SignatureCloner = signatureCloner;
            this.PopulateVariableCloners();
            this.PopulateInstructionCloners();
        }

        /// <summary>
        /// Gets or sets the method signature cloner with which this method body cloner is associated
        /// </summary>
        public MethodSignatureCloner SignatureCloner { get; private set; }

        /// <summary>
        /// Populates <see cref="VariableCloners"/>.
        /// </summary>
        private void PopulateVariableCloners()
        {
            Contract.Ensures(this.VariableCloners != null);

            this.VariableCloners = new List<VariableCloner>();

            var voidTypeReference = this.Target.Method.Module.Import(typeof(void));
            foreach (var sourceVariable in this.Source.Variables)
            {
                var targetVariable = new VariableDefinition(sourceVariable.Name, voidTypeReference);
                this.Target.Variables.Add(targetVariable);
                this.VariableCloners.Add(new VariableCloner(this, targetVariable, sourceVariable));
            }
        }

        /// <summary>
        /// Gets or sets the collection of variable cloners for the method body.
        /// </summary>
        public List<VariableCloner> VariableCloners { get; private set; }

        /// <summary>
        /// Populates <see cref="InstructionCloners"/>.
        /// </summary>
        private void PopulateInstructionCloners()
        {
            Contract.Ensures(this.InstructionCloners != null);

            var ilProcessor = this.Target.GetILProcessor();

            this.InstructionCloners = new List<InstructionCloner>();

            foreach (var sourceInstruction in this.Source.Instructions)
            {
                // the operand is required to create the instruction
                // but at this stage, it root resolving is not yet allowed because wireframes of all items do not yet exist
                // so, where needed, dummy operands are used which will be replaced in the clone step of each instruction cloner
                Instruction targetInstruction =
                    sourceInstruction.Operand == null ?
                    ilProcessor.Create(sourceInstruction.OpCode) :
                    this.CreateInstructionWithOperand(ilProcessor, sourceInstruction.OpCode, (dynamic)sourceInstruction.Operand);
                ilProcessor.Append(targetInstruction);
                this.InstructionCloners.Add(new InstructionCloner(this, targetInstruction, sourceInstruction));
            }
        }

        /// <summary>
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        public List<InstructionCloner> InstructionCloners { get; private set; }

        /// <summary>
        /// Clones the method body from the source to the target.
        /// </summary>
        public override void Clone()
        {
            this.Target.InitLocals = this.Source.InitLocals;

            // TODO research correct usage of LocalVarToken
            //targetBody.LocalVarToken = new MetadataToken(
            //    sourceBody.LocalVarToken.TokenType,
            //    sourceBody.LocalVarToken.RID);

            this.Target.MaxStackSize = this.Source.MaxStackSize;

            // TODO method body scope may be tough to get right
            this.Target.Scope = this.Source.Scope;

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
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, object unsupportedOperand)
        {
            if (unsupportedOperand == null) { return ilProcessor.Create(opCode); }

            throw new NotSupportedException(
                string.Format("Unsupported operand of type in instruction to be cloned: {0}", unsupportedOperand.GetType().FullName));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, byte value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates an instruction for invoking Calling <c>extern</c> methods using a <see cref="CallSite"/> operand.
        /// (Currently this is not supported.)
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="callSite">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. Calling <c>extern</c> methods is not currently supported.
        /// </exception>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, CallSite callSite)
        {
            // TODO support extern methods and get coverage for this
            // see https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/OpCodes.cs for opcodes
            throw new NotSupportedException(
                "Callsite instruction operands are used with the calli op code to make unmanaged method calls. This is not supported.");
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, double value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="field">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, FieldReference field)
        {
            return ilProcessor.Create(opCode, new FieldReference("", this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }
        
        /// <summary>
        /// Creates a new method with the given operand
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, float value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction instruction)
        {
            return ilProcessor.Create(opCode, Instruction.Create(OpCodes.Nop));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instructions">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction[] instructions)
        {
            // TODO get coverage on this line with an inline switch
            // https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/OpCodes.cs
            return ilProcessor.Create(opCode, new Instruction[0]);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, int value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, long value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="method">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, MethodReference method)
        {
            return ilProcessor.Create(
                opCode,
                new MethodReference("", this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// Clones the method body from the source to the target.
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            return ilProcessor.Create(opCode, new ParameterDefinition(this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, sbyte value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, string value)
        {
            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="type">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, TypeReference type)
        {
            return ilProcessor.Create(opCode, this.ILCloningContext.RootTarget.Module.Import(typeof(void)));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, VariableDefinition variable)
        {
            return ilProcessor.Create(opCode, new VariableDefinition(this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }
    }
}
