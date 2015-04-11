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

// ReSharper disable UnusedParameter.Local

namespace Bix.Mixers.ILCloning
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
        /// <param name="source">Cloning source.</param>
        public InstructionCloner(MethodBodyCloner parent, InstructionCloner previous, Instruction source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.MethodContext != null);

            this.Parent = parent;
            this.Parent.InstructionCloners.Add(this);
            this.MethodContext = new MethodContext(parent);
            this.Previous = previous;
        }

        /// <summary>
        /// Creates a new <see cref="InstructionCloner"/>.
        /// </summary>
        /// <param name="methodContext">Method context for the new instruction cloner.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="target">Cloning target.</param>
        public InstructionCloner(MethodContext methodContext, Instruction source, Instruction target)
            : base(methodContext.ILCloningContext, source)
        {
            // TODO remove this...it's only used by the constructor broadcaster
            Contract.Requires(methodContext != null);
            Contract.Requires(methodContext.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.MethodContext != null);

            this.MethodContext = methodContext;
            this.ExistingTarget = target;
        }

        /// <summary>
        /// Gets or sets the context for the method associated with this cloner.
        /// </summary>
        private MethodBodyCloner Parent { get; set; }

        /// <summary>
        /// Gets or sets the context for the method associated with this cloner.
        /// </summary>
        public MethodContext MethodContext { get; private set; }

        /// <summary>
        /// Gets or sets the existing target instruction.
        /// </summary>
        /// <remarks>
        /// TODO remove this...it's only used by the constructor broadcaster
        /// </remarks>
        private Instruction ExistingTarget { get; set; }

        /// <summary>
        /// Gets or sets the cloner for the previous instruction, if any.
        /// </summary>
        private InstructionCloner Previous { get; set; }

        /// <summary>
        /// Creates the target instruction.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override Instruction CreateTarget()
        {
            if (this.Previous != null) { this.Previous.EnsureTargetIsCreatedAndSet(); }

            // now create the target
            if (this.ExistingTarget != null) { return this.ExistingTarget; }

            Instruction target = InstructionCloner.CreateCloningTargetFor(
                this.MethodContext,
                this.Parent.TargetILProcessor,
                this.Source);
            this.Parent.TargetILProcessor.Append(target);
            return target;
        }

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
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the target method body.</param>
        /// <param name="sourceInstruction">Source instruction.</param>
        /// <returns>Wireframe target instruction. The operand will be invalid so that it can be set by instruction cloning later.</returns>
        public static Instruction CreateCloningTargetFor(MethodContext methodContext, ILProcessor ilProcessor, Instruction sourceInstruction)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);
            Contract.Requires(sourceInstruction != null);

            return sourceInstruction.Operand == null ?
                ilProcessor.Create(sourceInstruction.OpCode) :
                CreateInstructionWithOperand(methodContext, ilProcessor, sourceInstruction.OpCode, (dynamic)sourceInstruction.Operand);
        }

        /// <summary>
        /// Catch-all method for dynamically dispatched instruction creation calls where the operand type is unrecognized
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="unsupportedOperand">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. This method is only invoked via dynamic dispatch when the operand is not recognized as a supported
        /// operand type. If the operand was recognized as a supported type, a different method would have been invoked.
        /// </exception>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, object unsupportedOperand)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            if (unsupportedOperand == null) { return ilProcessor.Create(opCode); }

            throw new NotSupportedException(
                string.Format("Unsupported operand of type in instruction to be cloned: {0}", unsupportedOperand.GetType().FullName));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, byte value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates an instruction for invoking Calling <c>extern</c> methods using a <see cref="CallSite"/> operand.
        /// (Currently this is not supported.)
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="callSite">Operand for instruction</param>
        /// <returns>Nothing. This method always raises an exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always raised. Calling <c>extern</c> methods is not currently supported.
        /// </exception>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, CallSite callSite)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            // TODO support extern methods and get coverage for this
            // see https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/OpCodes.cs for opcodes
            throw new NotSupportedException(
                "Callsite instruction operands are used with the calli op code to make unmanaged method calls. This is not supported.");
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, double value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="field">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, FieldReference field)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new FieldReference("", methodContext.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, float value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instruction">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, Instruction instruction)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, Instruction.Create(OpCodes.Nop));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="instructions">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, Instruction[] instructions)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            // TODO get coverage on this line with an inline switch
            // https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/OpCodes.cs
            return ilProcessor.Create(opCode, new Instruction[0]);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, int value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, long value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="method">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, MethodReference method)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(
                opCode,
                new MethodReference("", methodContext.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// Clones the method body from the source to the target.
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new ParameterDefinition(methodContext.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, sbyte value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="value">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, string value)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, value);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="type">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, TypeReference type)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, methodContext.ILCloningContext.RootTarget.Module.Import(typeof(void)));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="methodContext">Cloning context.</param>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="variable">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private static Instruction CreateInstructionWithOperand(MethodContext methodContext, ILProcessor ilProcessor, OpCode opCode, VariableDefinition variable)
        {
            Contract.Requires(methodContext != null);
            Contract.Requires(ilProcessor != null);

            return ilProcessor.Create(opCode, new VariableDefinition(methodContext.ILCloningContext.RootTarget.Module.Import(typeof(void))));
        }

        #endregion
    }
}
