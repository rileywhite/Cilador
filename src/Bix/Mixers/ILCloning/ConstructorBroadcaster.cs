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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Bix.Mixers.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones a mixin implementation's default constructor into target constructors.
    /// </summary>
    /// <remarks>
    /// There are two classifications of constructors: those that call into a "base"
    /// constructor and those that call into a different "this" constructor. During
    /// object initialization, exactly one constructor that calls into a base constructor
    /// is executed, so these are the constructors that we focus on. Each of these
    /// "initializing" constructors will get code from the implementation's default
    /// constructor. They will get both initialization code, i.e. compler generated code
    /// that runs before the implementation's base constructor call such as field initialization,
    /// and constructor code, which is code compiled from programmer-written constructor
    /// code.
    /// </remarks>
    internal class ConstructorBroadcaster
    {
        /// <summary>
        /// Creates a new <see cref="ConstructorBroadcaster"/>.
        /// </summary>
        /// <param name="ilCloningContext"></param>
        /// <param name="sourceConstructor"></param>
        /// <param name="targetType"></param>
        public ConstructorBroadcaster(
            ILCloningContext ilCloningContext,
            MethodDefinition sourceConstructor,
            TypeDefinition targetType)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(sourceConstructor != null);
            Contract.Requires(sourceConstructor.DeclaringType == ilCloningContext.RootSource);
            Contract.Requires(!sourceConstructor.Parameters.Any());
            Contract.Requires(sourceConstructor.HasBody);
            Contract.Requires(targetType != null);
            Contract.Requires(targetType.Methods != null);

            this.ILCloningContext = ilCloningContext;
            this.SourceConstructor = sourceConstructor;
            this.TargetType = targetType;
            this.VariableCloners = new List<VariableCloner>();
            this.InstructionCloners = new List<InstructionCloner>();

            Contract.Assert(this.SourceConstructor.Body != null);
            Contract.Assert(this.SourceConstructor.Body.Instructions != null);
            Contract.Assert(this.SourceConstructor.Body.Instructions.Any());
        }

        /// <summary>
        /// Gets or sets the IL cloning context.
        /// </summary>
        private ILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Source default constructor that will be broadcast into target constructors.
        /// </summary>
        private MethodDefinition SourceConstructor { get; set; }

        /// <summary>
        /// Target type for the constructor broadcast.
        /// </summary>
        private TypeDefinition TargetType { get; set; }

        /// <summary>
        /// Variable cloners that will be added to the collection of all cloners.
        /// </summary>
        public List<VariableCloner> VariableCloners { get; private set; }

        /// <summary>
        /// Instruction cloners that will be added to the collection of all cloners.
        /// </summary>
        public List<InstructionCloner> InstructionCloners { get; private set; }

        /// <summary>
        /// Performs the actual work of cloning the source constructor to target constructors.
        /// </summary>
        public void BroadcastConstructor()
        {
            var sourceMultiplexedConstructor = ConstructorMultiplexer.Get(this.ILCloningContext, this.SourceConstructor);
            if (sourceMultiplexedConstructor.HasInitializationItems)
            {
                this.InjectInitializationItems(sourceMultiplexedConstructor);
            }

            if (sourceMultiplexedConstructor.HasConstructionItems)
            {
                this.InjectConstructionItems(sourceMultiplexedConstructor);
            }
        }

        /// <summary>
        /// Adds initialization items to all initializing target constructors.
        /// </summary>
        /// <param name="sourceMultiplexedConstructor">Multiplexed source constructor.</param>
        private void InjectInitializationItems(ConstructorMultiplexer sourceMultiplexedConstructor)
        {
            Contract.Requires(sourceMultiplexedConstructor != null);
            Contract.Requires(sourceMultiplexedConstructor.HasInitializationItems);

            foreach (var targetConstructor in this.TargetType.Methods.Where(method => method.IsConstructor && !method.IsStatic))
            {
                var targetMultiplexedConstructor = ConstructorMultiplexer.Get(this.ILCloningContext, this.SourceConstructor);
                if (!targetMultiplexedConstructor.IsInitializingConstructor) { continue; }  // skip non-initializing constructors

                this.InjectInitializationItems(sourceMultiplexedConstructor, targetConstructor);
            }
        }

        /// <summary>
        /// Adds initialization variables and/or instructions into a target's constructor.
        /// </summary>
        /// <param name="sourceMultiplexedConstructor">Multiplexed source constructor.</param>
        /// <param name="targetConstructor">Target constructor to inject into.</param>
        private void InjectInitializationItems(ConstructorMultiplexer sourceMultiplexedConstructor, MethodDefinition targetConstructor)
        {
            Contract.Requires(sourceMultiplexedConstructor != null);
            Contract.Requires(targetConstructor != null);
            Contract.Requires(sourceMultiplexedConstructor.HasInitializationItems);

            targetConstructor.Body.InitLocals = targetConstructor.Body.InitLocals || sourceMultiplexedConstructor.InitializationVariables.Any();

            var variableCloners = new List<VariableCloner>();
            var voidTypeReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
            var targetVariableIndexBySourceVariableIndex = new Dictionary<int, int>();
            foreach (var sourceVariable in sourceMultiplexedConstructor.InitializationVariables)
            {
                var targetVariable = new VariableDefinition(sourceVariable.Name, voidTypeReference);
                targetConstructor.Body.Variables.Add(targetVariable);
                targetVariableIndexBySourceVariableIndex.Add(sourceVariable.Index, targetVariable.Index);
                variableCloners.Add(new VariableCloner(this.ILCloningContext, sourceVariable, targetVariable));
            }
            this.VariableCloners.AddRange(variableCloners);

            var ilProcessor = targetConstructor.Body.GetILProcessor();
            var firstInstructionInTargetConstructor = targetConstructor.Body.Instructions[0];

            var instructionCloners = new List<InstructionCloner>(sourceMultiplexedConstructor.InitializationInstructions.Count);
            MethodContext methodContext = new MethodContext(
                this.ILCloningContext,
                Tuple.Create(this.SourceConstructor.Body.ThisParameter, targetConstructor.Body.ThisParameter),
                new List<Tuple<ParameterDefinition, LazyAccessor<ParameterDefinition>>>(),
                variableCloners,
                instructionCloners);
            foreach (var sourceInstruction in sourceMultiplexedConstructor.InitializationInstructions)
            {
                int translation;
                int? sourceVariableIndex;
                if (!sourceInstruction.TryGetVariableIndex(out sourceVariableIndex)) { translation = 0; }
                else
                {
                    Contract.Assert(sourceVariableIndex.HasValue);

                    int targetVariableIndex;
                    if (!targetVariableIndexBySourceVariableIndex.TryGetValue(sourceVariableIndex.Value, out targetVariableIndex))
                    {
                        throw new InvalidOperationException("No source entry to find target variable index for a construction instruction.");
                    }
                    translation = targetVariableIndex - sourceVariableIndex.Value;
                }
                var translatedSourceInstruction = sourceInstruction.ApplyLocalVariableTranslation(translation);
                Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(methodContext, ilProcessor, translatedSourceInstruction);
                ilProcessor.InsertBefore(firstInstructionInTargetConstructor, targetInstruction);
                instructionCloners.Add(new InstructionCloner(methodContext, translatedSourceInstruction, targetInstruction));
            }
            this.InstructionCloners.AddRange(instructionCloners);
        }

        /// <summary>
        /// Adds construction variables and/or instructions into all initializing target constructors.
        /// </summary>
        /// <param name="sourceMultiplexedConstructor">Multiplexed source constructor.</param>
        private void InjectConstructionItems(ConstructorMultiplexer sourceMultiplexedConstructor)
        {
            Contract.Requires(sourceMultiplexedConstructor != null);
            Contract.Requires(sourceMultiplexedConstructor.HasConstructionItems);

            // because the source constructor may have multiple exit points, it's not enough to simply copy instructions
            // from the source into target constructors
            // the safe way to handle this is to enclose construction items into their own method and invoke that from
            // every target constructor

            var constructionMethod = new MethodDefinition(
                string.Format("ctor_{0:N}", Guid.NewGuid()),
                MethodAttributes.Private | MethodAttributes.HideBySig,
                this.ILCloningContext.RootTarget.Module.Import(typeof(void)));
            this.TargetType.Methods.Add(constructionMethod);

            constructionMethod.Body = new MethodBody(constructionMethod)
            {
                InitLocals = sourceMultiplexedConstructor.ConstructionVariables.Any()
            };

            var variableCloners = new List<VariableCloner>();
            var voidTypeReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
            var targetVariableIndexBySourceVariableIndex = new Dictionary<int, int>();
            foreach (var sourceVariable in sourceMultiplexedConstructor.InitializationVariables)
            {
                var targetVariable = new VariableDefinition(sourceVariable.Name, voidTypeReference);
                constructionMethod.Body.Variables.Add(targetVariable);
                targetVariableIndexBySourceVariableIndex.Add(sourceVariable.Index, targetVariable.Index);
                variableCloners.Add(new VariableCloner(this.ILCloningContext, sourceVariable, targetVariable));
            }
            this.VariableCloners.AddRange(variableCloners);

            var ilProcessor = constructionMethod.Body.GetILProcessor();
            var instructionCloners = new List<InstructionCloner>(sourceMultiplexedConstructor.InitializationInstructions.Count);
            MethodContext methodContext = new MethodContext(
                this.ILCloningContext,
                Tuple.Create(this.SourceConstructor.Body.ThisParameter, constructionMethod.Body.ThisParameter),
                new List<Tuple<ParameterDefinition, LazyAccessor<ParameterDefinition>>>(),
                variableCloners,
                instructionCloners);
            foreach (var sourceInstruction in sourceMultiplexedConstructor.ConstructionInstructions)
            {
                int translation;
                int? sourceVariableIndex;
                if (!sourceInstruction.TryGetVariableIndex(out sourceVariableIndex)) { translation = 0; }
                else
                {
                    Contract.Assert(sourceVariableIndex.HasValue);

                    int targetVariableIndex;
                    if (!targetVariableIndexBySourceVariableIndex.TryGetValue(sourceVariableIndex.Value, out targetVariableIndex))
                    {
                        throw new InvalidOperationException("No source entry to find target variable index for a construction instruction.");
                    }
                    translation = targetVariableIndex - sourceVariableIndex.Value;
                }
                var translatedSourceInstruction = sourceInstruction.ApplyLocalVariableTranslation(translation);
                Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(methodContext, ilProcessor, translatedSourceInstruction);
                ilProcessor.Append(targetInstruction);
                instructionCloners.Add(new InstructionCloner(methodContext, translatedSourceInstruction, targetInstruction));
            }
            this.InstructionCloners.AddRange(instructionCloners);

            foreach (var targetConstructor in this.TargetType.Methods.Where(method => method.IsConstructor && !method.IsStatic))
            {
                // we can't re-use multiplexed target constructors from initialization because they may have changed
                var targetMultiplexedConstructor = ConstructorMultiplexer.Get(this.ILCloningContext, targetConstructor);
                if (!targetMultiplexedConstructor.IsInitializingConstructor) { continue; }  // skip non-initializing constructors

                var boundaryInstruction = targetConstructor.Body.Instructions[targetMultiplexedConstructor.BoundaryLastInstructionIndex];
                var targetILProcessor = targetConstructor.Body.GetILProcessor();

                // insert in reverse order
                targetILProcessor.InsertAfter(boundaryInstruction, targetILProcessor.Create(OpCodes.Call, constructionMethod));
                targetILProcessor.InsertAfter(boundaryInstruction, targetILProcessor.Create(OpCodes.Ldarg_0));
            }
        }
    }
}
