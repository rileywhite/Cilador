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

namespace Bix.Mixers.ILCloning
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
        /// <param name="source">Resolved cloning source.</param>
        /// <param name="target">Resolved cloning target.</param>
        public MethodBodyCloner(MethodSignatureCloner signatureCloner, MethodBody source, MethodBody target)
            : base(signatureCloner.ILCloningContext, source, target)
        {
            Contract.Requires(signatureCloner != null);
            Contract.Requires(signatureCloner.ILCloningContext != null);
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Ensures(this.SignatureCloner != null);
            Contract.Ensures(this.VariableCloners != null);

            this.SignatureCloner = signatureCloner;
            this.PopulateVariableCloners();
            this.PopulateInstructionCloners();
            this.PopulateExceptionHandlerCloners();
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
                this.VariableCloners.Add(new VariableCloner(this.ILCloningContext, sourceVariable, targetVariable));
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
                // but at this stage root resolving is not yet allowed because wireframes of all items do not yet exist
                // so, where needed, dummy operands are used which will be replaced in the clone step of each instruction cloner
                Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(new MethodContext(this), ilProcessor, sourceInstruction);
                ilProcessor.Append(targetInstruction);
                this.InstructionCloners.Add(new InstructionCloner(this, sourceInstruction, targetInstruction));
            }
        }

        /// <summary>
        /// Gets or sets the collection of instruction cloners for the method body.
        /// </summary>
        public List<InstructionCloner> InstructionCloners { get; private set; }

        /// <summary>
        /// Populates <see cref="ExceptionHandlerCloners"/>.
        /// </summary>
        private void PopulateExceptionHandlerCloners()
        {
            Contract.Ensures(this.ExceptionHandlerCloners != null);

            this.ExceptionHandlerCloners = new List<ExceptionHandlerCloner>();

            foreach (var sourceExceptionHandler in this.Source.ExceptionHandlers)
            {
                var targetExceptionHandler = new ExceptionHandler(sourceExceptionHandler.HandlerType);
                this.Target.ExceptionHandlers.Add(targetExceptionHandler);
                this.ExceptionHandlerCloners.Add(new ExceptionHandlerCloner(new MethodContext(this), sourceExceptionHandler, targetExceptionHandler));
            }
        }

        /// <summary>
        /// Gets or sets the collection of exception handler cloners for the method body.
        /// </summary>
        public List<ExceptionHandlerCloner> ExceptionHandlerCloners { get; private set; }

        /// <summary>
        /// Clones the method body from the source to the target.
        /// </summary>
        public override void Clone()
        {
            this.Target.InitLocals = this.Source.InitLocals;

            this.Target.MaxStackSize = this.Source.MaxStackSize;

            if (this.Source.Scope != null)
            {
                // TODO method body scope may be tough to get right
                // for now raise an exception
                throw new NotSupportedException("An unsupported configuration was detected. Please consider filing a bug report on the project's github page.");
            }

            this.IsCloned = true;
        }
    }
}
