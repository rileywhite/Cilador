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
    internal class MethodCloner : MemberClonerBase<MethodDefinition, MethodSourceWithRoot>
    {
        public MethodCloner(MethodDefinition target, MethodSourceWithRoot source)
            : base(target, source)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
        }

        public override void CloneStructure()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.SourceWithRoot.Source.Name);

            this.Target.Attributes = this.SourceWithRoot.Source.Attributes;
            this.Target.CallingConvention = this.SourceWithRoot.Source.CallingConvention;
            this.Target.ExplicitThis = this.SourceWithRoot.Source.ExplicitThis;
            this.Target.HasThis = this.SourceWithRoot.Source.HasThis;
            this.Target.ImplAttributes = this.SourceWithRoot.Source.ImplAttributes;
            this.Target.IsAddOn = this.SourceWithRoot.Source.IsAddOn;
            this.Target.IsCheckAccessOnOverride = this.SourceWithRoot.Source.IsCheckAccessOnOverride;
            this.Target.IsFire = this.SourceWithRoot.Source.IsFire;
            this.Target.IsForwardRef = this.SourceWithRoot.Source.IsForwardRef;
            this.Target.IsGetter = this.SourceWithRoot.Source.IsGetter;
            this.Target.IsIL = this.SourceWithRoot.Source.IsIL;
            this.Target.IsInternalCall = this.SourceWithRoot.Source.IsInternalCall;
            this.Target.IsManaged = this.SourceWithRoot.Source.IsManaged;
            this.Target.IsNative = this.SourceWithRoot.Source.IsNative;
            this.Target.IsOther = this.SourceWithRoot.Source.IsOther;
            this.Target.IsPreserveSig = this.SourceWithRoot.Source.IsPreserveSig;
            this.Target.IsRemoveOn = this.SourceWithRoot.Source.IsRemoveOn;
            this.Target.IsRuntime = this.SourceWithRoot.Source.IsRuntime;
            this.Target.IsSetter = this.SourceWithRoot.Source.IsSetter;
            this.Target.IsSynchronized = this.SourceWithRoot.Source.IsSynchronized;
            this.Target.IsUnmanaged = this.SourceWithRoot.Source.IsUnmanaged;
            this.Target.NoInlining = this.SourceWithRoot.Source.NoInlining;
            this.Target.NoOptimization = this.SourceWithRoot.Source.NoOptimization;
            this.Target.SemanticsAttributes = this.SourceWithRoot.Source.SemanticsAttributes;

            // TODO research correct usage of method MetadataToken
            this.Target.MetadataToken = new MetadataToken(this.SourceWithRoot.Source.MetadataToken.TokenType, this.SourceWithRoot.Source.MetadataToken.RID);

            if(this.SourceWithRoot.Source.IsPInvokeImpl)
            {
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not contain extern methods: [{0}]",
                    this.SourceWithRoot.RootContext.RootSource.FullName));
            }
            Contract.Assert(this.SourceWithRoot.Source.PInvokeInfo == null);

            if (this.SourceWithRoot.Source.HasOverrides)
            {
                foreach (var sourceOverride in this.SourceWithRoot.Source.Overrides)
                {
                    this.Target.Overrides.Add(this.SourceWithRoot.RootImport(sourceOverride));
                }
            }

            this.ParameterOperandReplacementMap = new Dictionary<ParameterDefinition, ParameterDefinition>(this.SourceWithRoot.Source.Parameters.Count);
            if (this.SourceWithRoot.Source.HasParameters)
            {
                this.Target.Parameters.CloneAllParameters(
                    this.SourceWithRoot.Source.Parameters,
                    this.SourceWithRoot.RootContext,
                    this.ParameterOperandReplacementMap);
            }
            Contract.Assert(this.Target.Parameters.Count == this.SourceWithRoot.Source.Parameters.Count);

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.SourceWithRoot.Source, this.SourceWithRoot.RootContext);

            if (this.SourceWithRoot.Source.HasGenericParameters)
            {
                // TODO method generic parameters
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not include any generic methods: [{0}]",
                    this.SourceWithRoot.RootContext.RootSource.FullName));
            }

            if (this.SourceWithRoot.Source.HasSecurityDeclarations)
            {
                // TODO method security declarations
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not contain methods annotated with security attributes: [{0}]",
                    this.SourceWithRoot.RootContext.RootSource.FullName));
            }

            var sourceMethodReturnType = this.SourceWithRoot.Source.MethodReturnType;
            Contract.Assert(sourceMethodReturnType != null);
            this.Target.MethodReturnType = new MethodReturnType(this.Target);
            this.Target.MethodReturnType.ReturnType = this.SourceWithRoot.RootImport(sourceMethodReturnType.ReturnType);
            this.Target.MethodReturnType.Attributes = sourceMethodReturnType.Attributes;
            this.Target.MethodReturnType.Constant = sourceMethodReturnType.Constant;
            this.Target.MethodReturnType.HasConstant = sourceMethodReturnType.HasConstant;

            // TODO research correct usage of MethodReturnType.MarshalInfo
            if (sourceMethodReturnType.MarshalInfo != null)
            {
                this.Target.MethodReturnType.MarshalInfo = new MarshalInfo(sourceMethodReturnType.MarshalInfo.NativeType);
            }

            // TODO research correct usage of MethodReturnType.MetadataToken
            this.Target.MethodReturnType.MetadataToken =
                new MetadataToken(sourceMethodReturnType.MetadataToken.TokenType, sourceMethodReturnType.MetadataToken.RID);

            this.Target.MethodReturnType.CloneAllCustomAttributes(sourceMethodReturnType, this.SourceWithRoot.RootContext);

            this.IsStructureCloned = true;
        }

        Dictionary<ParameterDefinition, ParameterDefinition> ParameterOperandReplacementMap { get; set; }

        public bool IsBodyCloned { get; private set; }

        public void CloneLogic()
        {
            Contract.Requires(this.IsStructureCloned);
            Contract.Requires(!this.IsBodyCloned);
            Contract.Ensures(this.IsBodyCloned);

            Contract.Assert(this.ParameterOperandReplacementMap != null);

            var sourceBody = this.SourceWithRoot.Source.Body;

            if(sourceBody == null)
            {
                this.Target.Body = null;
                this.IsBodyCloned = true;
                return;
            }

            var targetBody = this.Target.Body;

            targetBody.InitLocals = sourceBody.InitLocals;

            // TODO not sure about this
            targetBody.LocalVarToken = new MetadataToken(
                sourceBody.LocalVarToken.TokenType,
                sourceBody.LocalVarToken.RID);

            targetBody.MaxStackSize = sourceBody.MaxStackSize;

            // TODO method body scope may be tough to get right
            targetBody.Scope = sourceBody.Scope;

            var variableOperandReplacementMap = new Dictionary<VariableDefinition, VariableDefinition>(sourceBody.Variables.Count);
            foreach (var sourceVariable in sourceBody.Variables)
            {
                var targetVariable = new VariableDefinition(
                    sourceVariable.Name,
                    this.SourceWithRoot.RootImport(sourceVariable.VariableType));

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

        private bool TryReplaceParameterOperand(Dictionary<ParameterDefinition, ParameterDefinition> parameterOperandReplacementMap, Instruction targetInstruction)
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

        private bool TryReplaceVariableOperand(Dictionary<VariableDefinition, VariableDefinition> variableOperandReplacementMap, Instruction targetInstruction)
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

        private bool TryReplaceInstructionOperand(Dictionary<Instruction, Instruction> instructionOperandReplacementMap, Instruction targetInstruction)
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

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, object unsupportedOperand)
        {
            if (unsupportedOperand == null) { return ilProcessor.Create(opCode); }

            throw new NotSupportedException(
                string.Format("Unsupported operand of type in instruction to be cloned: {0}", unsupportedOperand.GetType().FullName));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, byte value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, CallSite callSite)
        {
            throw new NotSupportedException(
                "Callsite instruction operands are used with the calli op code to make unmanaged method calls. This is not supported.");
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, double value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, FieldReference field)
        {
            return ilProcessor.Create(opCode, this.SourceWithRoot.RootImport(field));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, float value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction target)
        {
            return ilProcessor.Create(opCode, target);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, Instruction[] targets)
        {
            return ilProcessor.Create(opCode, targets);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, int value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, long value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, MethodReference method)
        {
            return ilProcessor.Create(opCode, this.SourceWithRoot.RootImport(method));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, ParameterDefinition parameter)
        {
            return ilProcessor.Create(opCode, parameter);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, sbyte value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, string value)
        {
            return ilProcessor.Create(opCode, value);
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, TypeReference type)
        {
            return ilProcessor.Create(opCode, this.SourceWithRoot.RootImport(type));
        }

        private Instruction CreateInstructionWithOperand(ILProcessor ilProcessor, OpCode opCode, VariableDefinition variable)
        {
            return ilProcessor.Create(opCode, variable);
        }
    }
}
