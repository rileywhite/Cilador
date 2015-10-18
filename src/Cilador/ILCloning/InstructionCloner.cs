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
            Contract.Ensures(this.TargetInstructionCreator != null);
            Contract.Ensures(this.OperandImporter != null);

            this.Parent = parent;
            this.Previous = previous;
            this.PossiblyReferencedVariables = possiblyReferencedVariables ?? new VariableDefinition[0];
            this.ReferencedVariableIndexTranslation = referencedVariableIndexTranslation;
            this.InstructionInsertAction = instructionInsertAction ?? InstructionCloner.DefaultInstructionInsertAction;
            this.TargetInstructionCreator = new InstructionCreateDispatcher(parent.ILCloningContext);
            this.OperandImporter = new OperandImportDispatcher(
                parent.ILCloningContext,
                this.Parent.SourceThisParameter,
                this.Parent.Target.ThisParameter);
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
            var target = this.TargetInstructionCreator.InvokeFor(source, ilProcessor);

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
                this.Target.Operand = this.OperandImporter.InvokeFor(this.Source);
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

        #region Creating instructions that may not have the correct operand

        /// <summary>
        /// Gets or sets the dispatcher for creating target instructions.
        /// </summary>
        private InstructionCreateDispatcher TargetInstructionCreator { get; set; }

        /// <summary>
        /// Used to create cloning target instructions with possibly dummy operands.
        /// </summary>
        private sealed class InstructionCreateDispatcher : InstructionOperandFunctionDispatcherBase<Instruction, ILProcessor>
        {
            /// <summary>
            /// Creates a new <see cref="InstructionCreateDispatcher"/>.
            /// </summary>
            /// <param name="ilCloningContext">Context for the cloning operation.</param>
            public InstructionCreateDispatcher(IILCloningContext ilCloningContext)
            {
                Contract.Requires(ilCloningContext != null);
                Contract.Ensures(this.ILCloningContext != null);

                this.ILCloningContext = ilCloningContext;
            }

            /// <summary>
            /// Context for the cloning operation.
            /// </summary>
            private IILCloningContext ILCloningContext { get; set; }

            /// <summary>
            /// Gets an instruction target for a source with a <c>null</c> operand.
            /// </summary>
            /// <param name="opCode">Op code of the instruction.</param>
            /// <param name="ilProcessor">IL processor for instruction creation.</param>
            /// <returns>New target instruction</returns>
            protected override Instruction GetReturnValueForNull(OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(byte operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(double operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(FieldReference operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, new FieldReference("", this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(float operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(Instruction operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, Instruction.Create(OpCodes.Nop));
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(Instruction[] operand, OpCode opCode, ILProcessor ilProcessor)
            {
                // TODO get coverage on this line with an inline switch
                // https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/OpCodes.cs
                return ilProcessor.Create(opCode, new Instruction[0]);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(int operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(long operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(MethodReference operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(
                    opCode,
                    new MethodReference("", this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(ParameterDefinition operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, new ParameterDefinition(this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(sbyte operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(string operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, operand);
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(TypeReference operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, this.ILCloningContext.RootTarget.Module.Import(typeof(void)));
            }

            /// <summary>
            /// Creates a new target instruction for the given source op code and operand.
            /// </summary>
            /// <param name="operand">Source instruction operand.</param>
            /// <param name="opCode">Source instruction op code.</param>
            /// <param name="ilProcessor">IL processor to use for instruction creation.</param>
            /// <returns>New target instruction.</returns>
            protected override Instruction InvokeForOperand(VariableDefinition operand, OpCode opCode, ILProcessor ilProcessor)
            {
                return ilProcessor.Create(opCode, new VariableDefinition(this.ILCloningContext.RootTarget.Module.Import(typeof(void))));
            }
        }

        #endregion

        #region Operand importing

        /// <summary>
        /// Gets or sets the dispatcher for importing instruction operands.
        /// </summary>
        private OperandImportDispatcher OperandImporter { get; set; }

        /// <summary>
        /// Type for performing root imports of instruction operands.
        /// </summary>
        private class OperandImportDispatcher : InstructionOperandFunctionDispatcherBase<object>
        {
            /// <summary>
            /// Creates a new <see cref="OperandImportDispatcher"/>.
            /// </summary>
            /// <param name="ilCloningContext">IL cloning context</param>
            /// <param name="sourceThisParameter">This parameter for the source method.</param>
            /// <param name="targetThisParameter">This parameter for the target method.</param>
            public OperandImportDispatcher(
                IILCloningContext ilCloningContext,
                ParameterDefinition sourceThisParameter,
                ParameterDefinition targetThisParameter)
            {
                Contract.Requires(ilCloningContext != null);
                Contract.Ensures(this.ILCloningContext != null);

                this.ILCloningContext = ilCloningContext;
                this.SourceThisParameter = sourceThisParameter;
                this.TargetThisParameter = targetThisParameter;
            }

            /// <summary>
            /// Cloning context for this object.
            /// </summary>
            private IILCloningContext ILCloningContext { get; set; }

            /// <summary>
            /// Source "this" parameter of the method body containing the instructions.
            /// </summary>
            private ParameterDefinition SourceThisParameter { get; set; }

            /// <summary>
            /// Target "this" parameter of the method body containing the instructions.
            /// </summary>
            private ParameterDefinition TargetThisParameter { get; set; }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(TypeReference operand)
            {
                return this.ILCloningContext.RootImport(operand);
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(FieldReference operand)
            {
                return this.ILCloningContext.RootImport(operand);
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(Instruction operand)
            {
                return this.ILCloningContext.RootImport(operand);
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(Instruction[] operand)
            {
                var importedTargets = new Instruction[operand.Length];
                for (int i = 0; i < operand.Length; i++)
                {
                    importedTargets[i] = (Instruction)this.InvokeForOperand(operand[i]);
                }
                return importedTargets;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(MethodReference operand)
            {
                return this.ILCloningContext.RootImport(operand);
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(ParameterDefinition operand)
            {
                if (operand == this.SourceThisParameter) { return this.TargetThisParameter; }
                return this.ILCloningContext.RootImport(operand);
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(VariableDefinition operand)
            {
                return this.ILCloningContext.RootImport(operand);
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(byte operand)
            {
                return operand;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(sbyte operand)
            {
                return operand;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(float operand)
            {
                return operand;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(double operand)
            {
                return operand;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(int operand)
            {
                return operand;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(long operand)
            {
                return operand;
            }

            /// <summary>
            /// Imports the operand.
            /// </summary>
            /// <param name="operand">Operand for instruction</param>
            /// <returns>Imported operand for target instruction.</returns>
            protected override object InvokeForOperand(string operand)
            {
                return operand;
            }
        }

        #endregion
    }
}
