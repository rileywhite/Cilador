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
using System.Collections.Generic;

// ReSharper disable UnusedParameter.Local

namespace Cilador.ILCloning
{
    /// <summary>
    /// Clones a source instruction to a target instruction
    /// </summary>
    internal class InstructionCloner : ClonerBase<Instruction>
    {
        /// <summary>
        /// Creates a new <see cref="InstructionCloner"/>.
        /// </summary>
        /// <param name="parent">Method body cloner associated with new instruction cloner.</param>
        /// <param name="previous">Cloner for the previous instruction, if any.</param>
        /// <param name="possiblyReferencedVariables">Collection of variables that may be referenced by the source instruction.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="referencedVariableIndexTranslation">Optional translation that should be applied to the possible variable reference due to added or removed variables.</param>
        /// <param name="instructionInsertAction">Override default instruction add behavior by sending a custom action.</param>
        /// <remarks>
        /// By default, an instruction is inserted after the previous instruction, and an instruction with no previous instruction, as
        /// specified by <paramref name="previous"/>, will be appended to the end of the target method. If a delegate is supplied in
        /// in <paramref name="instructionInsertAction"/>, then the delegate will be run instead.
        /// </remarks>
        public InstructionCloner(
            ICloneToMethodBody<object> parent,
            InstructionCloner previous,
            Instruction source,
            IEnumerable<VariableDefinition> possiblyReferencedVariables = null,
            int referencedVariableIndexTranslation = 0,
            Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction> instructionInsertAction = null)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.PossiblyReferencedVariables != null);
            Contract.Ensures(this.InstructionInsertAction != null);

            this.Parent = parent;
            this.Previous = previous;
            this.PossiblyReferencedVariables = possiblyReferencedVariables ?? new VariableDefinition[0];
            this.ReferencedVariableIndexTranslation = referencedVariableIndexTranslation;
            this.InstructionInsertAction = instructionInsertAction ?? InstructionCloner.DefaultInstructionInsertAction;
        }

        /// <summary>
        /// Gets or sets the action that will add the target instruction.
        /// </summary>
        /// <remarks>
        /// An action is not required to insert the instruction. In general, the instruction to
        /// be inserted is the <see cref="InstructionCloner.Target"/>.
        /// </remarks>
        private Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction> InstructionInsertAction { get; set; }

        /// <summary>
        /// Gets or sets the context for the method associated with this cloner.
        /// </summary>
        public ICloneToMethodBody<object> Parent { get; private set; }

        /// <summary>
        /// Gets or sets the cloner for the previous instruction, if any.
        /// </summary>
        public InstructionCloner Previous { get; private set; }

        /// <summary>
        /// Gets or sets the translation, if any, that should be applied to the variable reference
        /// due to added or removed variables.
        /// </summary>
        /// <remarks>
        /// A negative number translates left, and a positive number translates right.
        /// </remarks>
        private int ReferencedVariableIndexTranslation { get; set; }

        /// <summary>
        /// Gets or sets the collection of variables that may be referenced by the source variable.
        /// </summary>
        private IEnumerable<VariableDefinition> PossiblyReferencedVariables { get; set; }

        /// <summary>
        /// Creates the target instruction.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override Instruction GetTarget()
        {
            var previous = this.Previous;
            if (previous != null) { this.Previous.EnsureTargetIsSet(); }

            // apply a translation to the source's referenced variable, if applicable
            var source = this.ReferencedVariableIndexTranslation == 0
                ? this.Source
                : this.Source.ApplyLocalVariableTranslation(this.ReferencedVariableIndexTranslation, this.PossiblyReferencedVariables);

            // now create the target
            var ilProcessor = this.Parent.Target.GetILProcessor();
            Instruction target = CreateCloningTargetFor(
                this.ILCloningContext,
                ilProcessor,
                source);

            // run the insert action
            this.InstructionInsertAction(ilProcessor, this.Parent, this.Previous, this.Source, target);

            return target;
        }

        /// <summary>
        /// Clones the instruction
        /// </summary>
        protected override void DoClone()
        {
            if (this.Source.Operand != null)
            {
                this.Target.Operand = this.ImportOperand((dynamic)this.Source.Operand);
            }

            this.Target.Offset = this.Source.Offset;
        }

        /// <summary>
        /// Default action for inserting the target instruction.
        /// </summary>
        /// <param name="ilProcessor">IL processor to use in inserting the instruction.</param>
        /// <param name="parent">Cloner for the method body containing the instruction being cloned.</param>
        /// <param name="previous">Cloner for the previous instruction being cloned, if any.</param>
        /// <param name="source">Source instruction.</param>
        /// <param name="target">Target instruction</param>
        public static void DefaultInstructionInsertAction(
            ILProcessor ilProcessor,
            ICloneToMethodBody<object> parent,
            InstructionCloner previous,
            Instruction source,
            Instruction target)
        {
            if (previous == null) { ilProcessor.Append(target); }
            else { ilProcessor.InsertAfter(previous.Target, target); }
        }

        #region Methods for creating instructions that may not have the correct operand

        /// <summary>
        /// Creates a wireframe instruction (i.e. operand not set) to be used as a cloning target for the given source instruction.
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the target method body.</param>
        /// <param name="sourceInstruction">Source instruction.</param>
        /// <returns>Wireframe target instruction. The operand will be invalid so that it can be set by instruction cloning later.</returns>
        public static Instruction CreateCloningTargetFor(
            IILCloningContext ilCloningContext,
            ILProcessor ilProcessor,
            Instruction sourceInstruction)
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
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="unsupportedOperand">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. This method is only invoked via dynamic dispatch when the operand is not recognized as a supported
        /// operand type. If the operand was recognized as a supported type, a different method would have been invoked.
        /// </exception>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, object unsupportedOperand)
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
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, byte value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates an instruction for invoking Calling <c>extern</c> methods using a <see cref="CallSite"/> operand.
        /// (Currently this is not supported.)
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="callSite">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. Calling <c>extern</c> methods is not currently supported.
        /// </exception>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, CallSite callSite)
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
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, double value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="field">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, FieldReference field)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new FieldReference("", ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, float value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, Instruction instruction)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, Instruction.Create(OpCodes.Nop));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instructions">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, Instruction[] instructions)
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
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, int value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, long value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="method">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, MethodReference method)
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
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new ParameterDefinition(ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, sbyte value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, string value)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="type">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, TypeReference type)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, ilCloningContext.RootTarget.Module.Import(typeof(void)));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilCloningContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(IILCloningContext ilCloningContext, ILProcessor ilProcessor, OpCode opCode, VariableDefinition variable)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new VariableDefinition(ilCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        #endregion

        #region Operand cloning

        /// <summary>
        /// Catch-all method for dynamically dispatched instruction creation calls where the operand type does not need
        /// special handling.
        /// </summary>
        /// <param name="otherOperand">Operand for instruction</param>
        /// <returns>Returns the operand that was passed in.</returns>
        private object ImportOperand(object otherOperand)
        {
            return otherOperand;
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
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private Instruction ImportOperand(Instruction instruction)
        {
            Contract.Requires(instruction != null);

            return this.ILCloningContext.RootImport(instruction);
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

            if (parameter == this.Parent.SourceThisParameter) { return this.Parent.Target.ThisParameter; }
            return this.ILCloningContext.RootImport(parameter);
        }

        /// <summary>
        /// Imports the operand
        /// </summary>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>Imported operand for target instruction.</returns>
        private VariableDefinition ImportOperand(VariableDefinition variable)
        {
            Contract.Requires(variable != null);

            return this.ILCloningContext.RootImport(variable);
        }

        #endregion
    }
}
