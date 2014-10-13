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
        /// Creates a new <see cref="InstructionCloner"/>.
        /// </summary>
        /// <param name="methodBodyCloner">Method body cloner associated with new instruction cloner.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public InstructionCloner(MethodBodyCloner methodBodyCloner, Instruction target, Instruction source)
            : this(new MethodContext(methodBodyCloner), target, source)
        {
            Contract.Requires(methodBodyCloner != null);
            Contract.Requires(methodBodyCloner.ILCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.MethodContext != null);
        }

        /// <summary>
        /// Creates a new <see cref="InstructionCloner"/>.
        /// </summary>
        /// <param name="MethodContext">Method context for the new instruction cloner.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public InstructionCloner(MethodContext methodContext, Instruction target, Instruction source)
            : base(methodContext.ILCloningContext, target, source)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(methodContext.ILCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.MethodContext != null);

            this.MethodContext = methodContext;
        }

        /// <summary>
        /// Gets or sets the context for the method associated with this cloner.
        /// </summary>
        public MethodContext MethodContext { get; private set; }

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

        #region Operand cloning

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
        /// Imports the operand
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
        /// Imports the operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private double ImportOperand(double value)
        {
            return value;
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="field">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private FieldReference ImportOperand(FieldReference field)
        {
            Contract.Requires(field != null);
            return this.ILCloningContext.RootImport(field);
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private float ImportOperand(float value)
        {
            return value;
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private Instruction ImportOperand(Instruction instruction)
        {
            Contract.Requires(instruction != null);

            return this.MethodContext.RootImport(instruction);
        }

        /// <summary>
        /// Imports the operand
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
        /// Imports the operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private int ImportOperand(int value)
        {
            return value;
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private long ImportOperand(long value)
        {
            return value;
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="method">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private MethodReference ImportOperand(MethodReference method)
        {
            Contract.Requires(method != null);
            return this.ILCloningContext.RootImport(method);
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private ParameterDefinition ImportOperand(ParameterDefinition parameter)
        {
            Contract.Requires(parameter != null);

            return this.MethodContext.RootImport(parameter);
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private sbyte ImportOperand(sbyte value)
        {
            return value;
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="value">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private string ImportOperand(string value)
        {
            Contract.Requires(value != null);
            return value;
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="type">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private TypeReference ImportOperand(TypeReference type)
        {
            Contract.Requires(type != null);
            return this.ILCloningContext.RootImport(type);
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private VariableDefinition ImportOperand(VariableDefinition variable)
        {
            Contract.Requires(variable != null);

            return this.MethodContext.RootImport(variable);
        }

        #endregion

        #region Unknown operand instruction factory methods

        /// <summary>
        /// Creates a wireframe instruction (i.e. operand not set) to be used as a cloning target for the given source instruction.
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the target method body.</param>
        /// <param name="sourceInstruction">Source instruction.</param>
        /// <returns>Wireframe target instruction. The operand will be invalid so that it can be set by instruction cloning later.</returns>
        public static Instruction CreateCloningTargetFor(ILCloningContext ilCloningContext, ILProcessor ilProcessor, Instruction sourceInstruction)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);
            Contract.Requires(sourceInstruction != null);

            return sourceInstruction.Operand == null ?
                ilProcessor.Create(sourceInstruction.OpCode) :
                CreateInstructionWithOperand(ilCloningContext, ilProcessor, sourceInstruction.OpCode, (dynamic)sourceInstruction.Operand);
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
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, object unsupportedOperand)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

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
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, byte value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

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
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, CallSite callSite)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

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
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, double value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="field">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, FieldReference field)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new FieldReference("", ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, float value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, Instruction instruction)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, Instruction.Create(OpCodes.Nop));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instructions">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, Instruction[] instructions)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

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
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, int value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, long value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="method">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, MethodReference method)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(
                opCode,
                new MethodReference("", ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// Clones the method body from the source to the target.
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new ParameterDefinition(ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, sbyte value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, string value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="type">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, TypeReference type)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, ilCloningContext.RootTarget.Module.Import(typeof(void)));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(ILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, VariableDefinition variable)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new VariableDefinition(ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        #endregion
    }
}
