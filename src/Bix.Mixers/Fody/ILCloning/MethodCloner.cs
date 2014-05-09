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

using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Clones <see cref="MethodDefinition"/> contents from a source to a target.
    /// </summary>
    internal class MethodCloner : MemberClonerBase<MethodDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="MethodCloner"/>
        /// </summary>
        /// <param name="rootContext">Root context for cloning.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public MethodCloner(RootContext rootContext, MethodDefinition target, MethodDefinition source)
            : base(rootContext, target, source)
        {
            Contract.Requires(rootContext != null);
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        /// <summary>
        /// Clones all method info except the actual method body.
        /// </summary>
        public override void CloneStructure()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Name);

            this.Target.Attributes = this.Source.Attributes;
            this.Target.CallingConvention = this.Source.CallingConvention;
            this.Target.ExplicitThis = this.Source.ExplicitThis;
            this.Target.HasThis = this.Source.HasThis;
            this.Target.ImplAttributes = this.Source.ImplAttributes;
            this.Target.IsAddOn = this.Source.IsAddOn;
            this.Target.IsCheckAccessOnOverride = this.Source.IsCheckAccessOnOverride;
            this.Target.IsFire = this.Source.IsFire;
            this.Target.IsForwardRef = this.Source.IsForwardRef;
            this.Target.IsGetter = this.Source.IsGetter;
            this.Target.IsIL = this.Source.IsIL;
            this.Target.IsInternalCall = this.Source.IsInternalCall;
            this.Target.IsManaged = this.Source.IsManaged;
            this.Target.IsNative = this.Source.IsNative;
            this.Target.IsOther = this.Source.IsOther;
            this.Target.IsPreserveSig = this.Source.IsPreserveSig;
            this.Target.IsRemoveOn = this.Source.IsRemoveOn;
            this.Target.IsRuntime = this.Source.IsRuntime;
            this.Target.IsSetter = this.Source.IsSetter;
            this.Target.IsSynchronized = this.Source.IsSynchronized;
            this.Target.IsUnmanaged = this.Source.IsUnmanaged;
            this.Target.NoInlining = this.Source.NoInlining;
            this.Target.NoOptimization = this.Source.NoOptimization;
            this.Target.SemanticsAttributes = this.Source.SemanticsAttributes;

            // TODO research correct usage of method MetadataToken
            //this.Target.MetadataToken = new MetadataToken(this.Source.MetadataToken.TokenType, this.Source.MetadataToken.RID);

            if(this.Source.IsPInvokeImpl)
            {
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not contain extern methods: [{0}]",
                    this.RootContext.RootSource.FullName));
            }
            Contract.Assert(this.Source.PInvokeInfo == null);

            if (this.Source.HasOverrides)
            {
                foreach (var sourceOverride in this.Source.Overrides)
                {
                    this.Target.Overrides.Add(this.RootContext.RootImport(sourceOverride));
                }
            }

            this.ParameterOperandReplacementMap = new Dictionary<ParameterDefinition, ParameterDefinition>(this.Source.Parameters.Count);
            if (this.Source.HasParameters)
            {
                this.Target.Parameters.CloneAllParameters(
                    this.Source.Parameters,
                    this.RootContext,
                    this.ParameterOperandReplacementMap);
            }
            Contract.Assert(this.Target.Parameters.Count == this.Source.Parameters.Count);

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.Source, this.RootContext);

            if (this.Source.HasGenericParameters)
            {
                // TODO method generic parameters
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not include any generic methods: [{0}]",
                    this.RootContext.RootSource.FullName));
            }

            if (this.Source.HasSecurityDeclarations)
            {
                // TODO method security declarations
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not contain methods annotated with security attributes: [{0}]",
                    this.RootContext.RootSource.FullName));
            }

            var sourceMethodReturnType = this.Source.MethodReturnType;
            Contract.Assert(sourceMethodReturnType != null);
            this.Target.MethodReturnType = new MethodReturnType(this.Target);
            this.Target.MethodReturnType.ReturnType = this.RootContext.RootImport(sourceMethodReturnType.ReturnType);
            this.Target.MethodReturnType.Attributes = sourceMethodReturnType.Attributes;
            this.Target.MethodReturnType.Constant = sourceMethodReturnType.Constant;
            this.Target.MethodReturnType.HasConstant = sourceMethodReturnType.HasConstant;

            // TODO research correct usage of MethodReturnType.MarshalInfo
            if (sourceMethodReturnType.MarshalInfo != null)
            {
                this.Target.MethodReturnType.MarshalInfo = new MarshalInfo(sourceMethodReturnType.MarshalInfo.NativeType);
            }

            // TODO research correct usage of MethodReturnType.MetadataToken
            //this.Target.MethodReturnType.MetadataToken =
            //    new MetadataToken(sourceMethodReturnType.MetadataToken.TokenType, sourceMethodReturnType.MetadataToken.RID);

            this.Target.MethodReturnType.CloneAllCustomAttributes(sourceMethodReturnType, this.RootContext);

            this.IsStructureCloned = true;
        }

        /// <summary>
        /// Gets or sets the collection of clone parameters keyed by the source parameters that they will replace
        /// in the cloned method bodies.
        /// </summary>
        Dictionary<ParameterDefinition, ParameterDefinition> ParameterOperandReplacementMap { get; set; }

        /// <summary>
        /// Gets or sets whether the method body logic has been cloned.
        /// </summary>
        public bool IsBodyCloned { get; private set; }

        /// <summary>
        /// Clones the method body logic from the source to the target.
        /// </summary>
        public void CloneLogic()
        {
            Contract.Requires(this.IsStructureCloned);
            Contract.Requires(!this.IsBodyCloned);
            Contract.Ensures(this.IsBodyCloned);

            Contract.Assert(this.ParameterOperandReplacementMap != null);

            var sourceBody = this.Source.Body;

            if(sourceBody == null)
            {
                this.Target.Body = null;
                this.IsBodyCloned = true;
                return;
            }

            var targetBody = this.Target.Body;

            targetBody.InitLocals = sourceBody.InitLocals;

            // TODO research correct usage of LocalVarToken
            //targetBody.LocalVarToken = new MetadataToken(
            //    sourceBody.LocalVarToken.TokenType,
            //    sourceBody.LocalVarToken.RID);

            targetBody.MaxStackSize = sourceBody.MaxStackSize;

            // TODO method body scope may be tough to get right
            targetBody.Scope = sourceBody.Scope;

            var variableOperandReplacementMap = new Dictionary<VariableDefinition, VariableDefinition>(sourceBody.Variables.Count);
            foreach (var sourceVariable in sourceBody.Variables)
            {
                var targetVariable = new VariableDefinition(
                    sourceVariable.Name,
                    this.RootContext.RootImport(sourceVariable.VariableType));

                variableOperandReplacementMap.Add(sourceVariable, targetVariable);

                targetBody.Variables.Add(targetVariable);
            }

            var instructionOperandReplacementMap = new Dictionary<Instruction, Instruction>(sourceBody.Instructions.Count);
            var ilProcessor = targetBody.GetILProcessor();
            foreach (var sourceInstruction in sourceBody.Instructions)
            {
                Instruction targetInstruction;
                if (sourceInstruction.Operand == null)
                {
                    targetInstruction = ilProcessor.Create(sourceInstruction.OpCode);
                }
                else
                {
                    targetInstruction = this.CreateInstructionWithOperand(ilProcessor, sourceInstruction.OpCode, (dynamic)sourceInstruction.Operand);
                }
                targetInstruction.Offset = sourceInstruction.Offset;

                ilProcessor.Append(targetInstruction);
                instructionOperandReplacementMap.Add(sourceInstruction, targetInstruction);
            }

            foreach (var targetInstruction in targetBody.Instructions.Where(instruction => instruction.Operand != null))
            {
                if (TryReplaceParameterOperand(this.ParameterOperandReplacementMap, targetInstruction)) { continue; }
                if (TryReplaceThisReferenceOperand(sourceBody.ThisParameter, targetBody.ThisParameter, targetInstruction)) { continue; }
                if (TryReplaceVariableOperand(variableOperandReplacementMap, targetInstruction)) { continue; }
                if (TryReplaceInstructionOperand(instructionOperandReplacementMap, targetInstruction)) { continue; }
                if (TryReplaceInstructionsOperand(instructionOperandReplacementMap, targetInstruction)) { continue; }
            }

            this.IsBodyCloned = true;
        }

        /// <summary>
        /// Replaces a source parameter instruction operand with the corresponding target parameter operand if applicable.
        /// </summary>
        /// <param name="parameterOperandReplacementMap">Mapping of source to target parameters.</param>
        /// <param name="targetInstruction">Instruction to look at.</param>
        /// <returns><c>true</c> if the operand for the instruction is a parameter operand, else <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Thrown if the instruction operand is a parameter that isn't in the replacement map.</exception>
        private bool TryReplaceParameterOperand(
            Dictionary<ParameterDefinition, ParameterDefinition> parameterOperandReplacementMap,
            Instruction targetInstruction)
        {
            Contract.Requires(parameterOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var parameterOperand = targetInstruction.Operand as ParameterDefinition;
            if (parameterOperand != null)
            {
                ParameterDefinition replacementParameterOperand;
                if (!parameterOperandReplacementMap.TryGetValue(parameterOperand, out replacementParameterOperand))
                {
                    throw new InvalidOperationException("Failed to update parameter operand in an instruction");
                }
                targetInstruction.Operand = replacementParameterOperand;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces a source <c>this</c> reference instruction operand with the target's <c>this</c> reference if applicable.
        /// </summary>
        /// <param name="sourceThis">Source method's <c>this</c> reference.</param>
        /// <param name="targetThis">Target method's <c>this</c> reference.</param>
        /// <param name="targetInstruction">Instruction to look at.</param>
        /// <returns><c>true</c> if the operand for the instruction is a <c>this</c> reference, else <c>false</c></returns>
        private bool TryReplaceThisReferenceOperand(ParameterDefinition sourceThis, ParameterDefinition targetThis, Instruction targetInstruction)
        {
            Contract.Requires(targetInstruction != null);
            if (targetInstruction.Operand == sourceThis)
            {
                targetInstruction.Operand = targetThis;
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Replaces a source local variable instruction operand with the corresponding target local variable operand if applicable.
        /// </summary>
        /// <param name="variableOperandReplacementMap">Mapping of source to target local variables.</param>
        /// <param name="targetInstruction">Instruction to look at.</param>
        /// <returns><c>true</c> if the operand for the instruction is a local variable operand, else <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Thrown if the instruction operand is a local variable that isn't in the replacement map.</exception>
        private bool TryReplaceVariableOperand(
            Dictionary<VariableDefinition, VariableDefinition> variableOperandReplacementMap,
            Instruction targetInstruction)
        {
            Contract.Requires(variableOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var variableOperand = targetInstruction.Operand as VariableDefinition;
            if (variableOperand != null)
            {
                VariableDefinition replacementVariableOperand;
                if (!variableOperandReplacementMap.TryGetValue(variableOperand, out replacementVariableOperand))
                {
                    throw new InvalidOperationException("Failed to update local variable operand in an instruction");
                }
                targetInstruction.Operand = replacementVariableOperand;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces a source instruction reference instruction operand with the corresponding target instruction operand if applicable.
        /// </summary>
        /// <param name="instructionOperandReplacementMap">Mapping of source to target instructions.</param>
        /// <param name="targetInstruction">Instruction to look at.</param>
        /// <returns><c>true</c> if the operand for the instruction is an instruction reference operand, else <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Thrown if the instruction operand is an instruction reference that isn't in the replacement map.</exception>
        private bool TryReplaceInstructionOperand(
            Dictionary<Instruction, Instruction> instructionOperandReplacementMap,
            Instruction targetInstruction)
        {
            Contract.Requires(instructionOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var instructionOperand = targetInstruction.Operand as Instruction;
            if (instructionOperand != null)
            {
                Instruction replacementInstructionOperand;
                if (!instructionOperandReplacementMap.TryGetValue(instructionOperand, out replacementInstructionOperand))
                {
                    throw new InvalidOperationException("Failed to update instruction operand in an instruction");
                }
                targetInstruction.Operand = replacementInstructionOperand;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces items within a source instruction reference instruction array operand with the corresponding target instruction references if applicable.
        /// </summary>
        /// <param name="instructionOperandReplacementMap">Mapping of source to target instructions.</param>
        /// <param name="targetInstruction">Instruction to look at.</param>
        /// <returns><c>true</c> if the operand for the instruction is an instruction reference array operand, else <c>false</c></returns>
        /// <exception cref="InvalidOperationException">Thrown if any item in the instruction array operand is an instruction reference that isn't in the replacement map.</exception>
        private bool TryReplaceInstructionsOperand(Dictionary<Instruction, Instruction> instructionOperandReplacementMap, Instruction targetInstruction)
        {
            Contract.Requires(instructionOperandReplacementMap != null);
            Contract.Requires(targetInstruction != null);

            var instructionsOperand = targetInstruction.Operand as Instruction[];
            if (instructionsOperand != null)
            {
                for (int i = 0; i < instructionsOperand.Length; i++)
                {
                    Instruction replacementInstructionOperand;
                    if (!instructionOperandReplacementMap.TryGetValue(instructionsOperand[i], out replacementInstructionOperand))
                    {
                        throw new InvalidOperationException(string.Format("Failed to update index [{0}] within an instructions operand in an instruction", i.ToString()));
                    }
                    instructionsOperand[i] = replacementInstructionOperand;
                }
                return true;
            }

            return false;
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
            // TODO support extern methods
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
            return ilProcessor.Create(opCode, this.RootContext.RootImport(field));
        }

        /// <summary>
        /// Creates a new method with the given operand
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
        /// <param name="target">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction target)
        {
            return ilProcessor.Create(opCode, target);
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="targets">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction[] targets)
        {
            return ilProcessor.Create(opCode, targets);
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
            return ilProcessor.Create(opCode, this.RootContext.RootImport(method));
        }

        /// <summary>
        /// Creates a new method with the given operand
        /// </summary>
        /// <param name="ilProcessor">IL processor for the method body being cloned</param>
        /// <param name="opCode">MSIL op code of an instruction that should be created.</param>
        /// <param name="parameter">Operand for instruction</param>
        /// <returns>New instruction.</returns>
        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            return ilProcessor.Create(opCode, parameter);
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
            return ilProcessor.Create(opCode, this.RootContext.RootImport(type));
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
            return ilProcessor.Create(opCode, variable);
        }
    }
}
