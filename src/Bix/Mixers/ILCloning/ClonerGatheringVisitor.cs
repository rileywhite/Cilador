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
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// A visitor that traverses a cloner target hierarchy and creates
    /// cloners for each item that should be cloned.
    /// </summary>
    /// <remarks>
    /// This "visits" each item in the hierarchy, similar to the visitor pattern,
    /// but the classic visitor pattern doesn't quite make sense here. This
    /// is a variation.
    /// </remarks>
    internal class ClonerGatheringVisitor
    {
        /// <summary>
        /// Create a new <see cref="ClonerGatheringVisitor"/>
        /// </summary>
        public ClonerGatheringVisitor(ILCloningContext ilCloningContext)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Cloners != null);

            this.ILCloningContext = ilCloningContext;
            this.Cloners = new Cloners();
        }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        private ILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Gets or sets the cloners created during visit operations.
        /// </summary>
        public Cloners Cloners { get; private set; }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="rootTypeCloner">Cloner to gather child cloners for.</param>
        public void Visit(RootTypeCloner rootTypeCloner)
        {
            Contract.Requires(rootTypeCloner != null);
            this.Cloners.AddCloner(rootTypeCloner);

            if (this.ILCloningContext.RootSource.Methods.Any(
                sourceMethod => sourceMethod.IsConstructor && !sourceMethod.IsStatic && sourceMethod.HasParameters))
            {
                // at some point in the future if it becomes clear that it would be useful,
                // it may be possible to create all combinations of source and target constructor
                // arguments and put them into the final mixed target
                // but that's a complex and time-consuming task with unknown payoff
                // so for now we don't support mixin implementations that have constructors with parameters
                throw new InvalidOperationException(
                    string.Format(
                        "Configured mixin implementation cannot have constructors with parameters: [{0}]",
                        this.ILCloningContext.RootSource.FullName));
            }

            var sourceConstructor = this.ILCloningContext.RootSource.Methods.SingleOrDefault(
                sourceMethod => sourceMethod.IsConstructor && !sourceMethod.IsStatic && !sourceMethod.HasParameters);

            var sourceMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, sourceConstructor);

            ConstructorLogicSignatureCloner constructorLogicSignatureCloner = null;
            if (sourceMultiplexedConstructor.HasConstructionItems)
            {
                constructorLogicSignatureCloner =
                    new ConstructorLogicSignatureCloner(rootTypeCloner, sourceMultiplexedConstructor);
                this.Cloners.AddCloner(constructorLogicSignatureCloner);
                this.Visit(constructorLogicSignatureCloner);
            }

            // the initialization cloning includes calling redirected construction methods
            // so we want this if we have either initialization or if we have create a logic cloner
            if (sourceMultiplexedConstructor.HasInitializationItems || constructorLogicSignatureCloner != null)
            {
                foreach (var targetConstructor in
                    this.ILCloningContext.RootTarget.Methods.Where(
                        targetMethod => targetMethod.IsConstructor && !targetMethod.IsStatic))
                {
                    var targetMultiplexedConstructor = MultiplexedConstructor.Get(
                        this.ILCloningContext,
                        targetConstructor);
                    if (!targetMultiplexedConstructor.IsInitializingConstructor)
                    {
                        // skip non-initializing constructors because they will eventually call into an initializing constructor
                        continue;
                    }

                    Contract.Assert(targetConstructor.HasBody);
                    var constructorCloner = new ConstructorInitializationCloner(
                        rootTypeCloner,
                        constructorLogicSignatureCloner,
                        sourceMultiplexedConstructor,
                        targetConstructor.Body);

                    this.Cloners.AddCloner(constructorCloner);
                    this.Visit(constructorCloner);
                }
            }

            // continue visiting the items common to root and nested types
            this.Visit((ClonerBase<TypeDefinition>)rootTypeCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="constructorLogicSignatureCloner">Cloner to gather child cloners for.</param>
        private void Visit(ConstructorLogicSignatureCloner constructorLogicSignatureCloner)
        {
            var constructorLogicBodyCloner =
                new ConstructorLogicBodyCloner(constructorLogicSignatureCloner, constructorLogicSignatureCloner.Source);
            this.Cloners.AddCloner(constructorLogicBodyCloner);
            this.Visit(constructorLogicBodyCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="constructorLogicBodyCloner">Cloner to gather child cloners for.</param>
        private void Visit(ConstructorLogicBodyCloner constructorLogicBodyCloner)
        {
            var targetVariableIndexBySourceVariableIndex = new Dictionary<int, int>();
            VariableCloner previousVariableCloner = null;
            var nextVariableIndex = 0;
            foreach (var sourceVariable in constructorLogicBodyCloner.Source.ConstructionVariables)
            {
                var variableCloner = new VariableCloner(constructorLogicBodyCloner, previousVariableCloner, sourceVariable);
                targetVariableIndexBySourceVariableIndex.Add(sourceVariable.Index, nextVariableIndex);
                ++nextVariableIndex;
                this.Cloners.AddCloner(variableCloner);
                this.Visit(variableCloner);
                previousVariableCloner = variableCloner;
            }

            InstructionCloner previousInstructionCloner = null;
            foreach (var sourceInstruction in constructorLogicBodyCloner.Source.ConstructionInstructions)
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
                        throw new InvalidOperationException(
                            "No source entry to find target variable index for a construction logic instruction.");
                    }
                    translation = targetVariableIndex - sourceVariableIndex.Value;
                }

                var instructionCloner = new InstructionCloner(
                    constructorLogicBodyCloner,
                    previousInstructionCloner,
                    sourceInstruction,
                    constructorLogicBodyCloner.Source.Constructor.Body.Variables,
                    translation);
                this.Cloners.AddCloner(instructionCloner);
                this.Visit(instructionCloner);
                previousInstructionCloner = instructionCloner;
            }

            // all exception handlers will be in the constructor logic code rather than initialization code
            foreach (var sourceExceptionHandler in constructorLogicBodyCloner.Source.Constructor.Body.ExceptionHandlers)
            {
                var exceptionHandlerCloner = new ExceptionHandlerCloner(constructorLogicBodyCloner, sourceExceptionHandler);
                this.Cloners.AddCloner(exceptionHandlerCloner);
                this.Visit(exceptionHandlerCloner);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="constructorInitializationCloner">Cloner to gather child cloners for.</param>
        public void Visit(ConstructorInitializationCloner constructorInitializationCloner)
        {
            var targetVariableIndexBySourceVariableIndex = new Dictionary<int, int>();
            VariableCloner previousVariableCloner = null;
            var nextVariableIndex = constructorInitializationCloner.CountOfTargetVariablesBeforeCloning;
            foreach (var sourceVariable in constructorInitializationCloner.Source.InitializationVariables)
            {
                var variableCloner = new VariableCloner(constructorInitializationCloner, previousVariableCloner, sourceVariable);
                targetVariableIndexBySourceVariableIndex.Add(sourceVariable.Index, nextVariableIndex);
                ++nextVariableIndex;
                this.Cloners.AddCloner(variableCloner);
                this.Visit(variableCloner);
                previousVariableCloner = variableCloner;
            }

            Action<ILProcessor, ICloneToMethodBody<object>, InstructionCloner, Instruction, Instruction> instructionInsertAction =
                (ilProcessor, parent, previous, source, target) =>
            {
                if (previous != null) { ilProcessor.InsertAfter(previous.Target, target); }
                else
                {
                    var firstInstructionInTargetConstructor = parent.Target.Instructions[0];
                    ilProcessor.InsertBefore(firstInstructionInTargetConstructor, target);
                }
            };

            InstructionCloner previousInstructionCloner = null;
            foreach (var sourceInstruction in constructorInitializationCloner.Source.InitializationInstructions)
            {
                // determine translation of variable operand, if any
                int translation;
                int? sourceVariableIndex;
                if (!sourceInstruction.TryGetVariableIndex(out sourceVariableIndex)) { translation = 0; }
                else
                {
                    Contract.Assert(sourceVariableIndex.HasValue);

                    int targetVariableIndex;
                    if (!targetVariableIndexBySourceVariableIndex.TryGetValue(sourceVariableIndex.Value, out targetVariableIndex))
                    {
                        throw new InvalidOperationException(
                            "No source entry to find target variable index for a constructor initialization instruction.");
                    }
                    translation = targetVariableIndex - sourceVariableIndex.Value;
                }

                // create the cloner
                var instructionCloner = new InstructionCloner(
                    constructorInitializationCloner,
                    previousInstructionCloner,
                    sourceInstruction,
                    constructorInitializationCloner.Source.Constructor.Body.Variables,
                    translation,
                    instructionInsertAction);
                this.Cloners.AddCloner(instructionCloner);
                previousInstructionCloner = instructionCloner;
            }

            // no exception handlers in the initialization section of code
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="nestedTypeCloner">Cloner to gather child cloners for.</param>
        private void Visit(NestedTypeCloner nestedTypeCloner)
        {
            Contract.Requires(nestedTypeCloner != null);

            this.GatherAndVisitGenericParameterClonersFrom(nestedTypeCloner);
            this.GatherAndVisitCustomAttributeClonersFrom(nestedTypeCloner);

            // continue visiting the items common to root and nested types
            this.Visit((ClonerBase<TypeDefinition>)nestedTypeCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="typeCloner">Cloner to gather child cloners for.</param>
        private void Visit(ICloner<TypeDefinition> typeCloner)
        {
            Contract.Requires(typeCloner != null);

            var sourceType = typeCloner.Source;

            foreach (var sourceNestedType in sourceType.NestedTypes)
            {
                var nestedTypeCloner = new NestedTypeCloner(typeCloner, sourceNestedType);
                this.Cloners.AddCloner(nestedTypeCloner);
                this.Visit(nestedTypeCloner);
            }

            foreach (var sourceField in sourceType.Fields)
            {
                var fieldCloner = new FieldCloner(typeCloner, sourceField);
                this.Cloners.AddCloner(fieldCloner);
                this.Visit(fieldCloner);
            }

            foreach (var sourceMethod in sourceType.Methods)
            {
                if (sourceMethod.IsConstructor &&
                    !sourceMethod.IsStatic &&
                    sourceMethod.DeclaringType == this.ILCloningContext.RootSource)
                {
                    // this case is handled by the root type visitor
                    continue;
                }

                var methodSignatureCloner = new MethodSignatureCloner(typeCloner, sourceMethod);
                this.Cloners.AddCloner(methodSignatureCloner);
                this.Visit(methodSignatureCloner);
            }

            foreach (var sourceProperty in sourceType.Properties)
            {
                var propertyCloner = new PropertyCloner(typeCloner, sourceProperty);
                this.Cloners.AddCloner(propertyCloner);
                this.Visit(propertyCloner);
            }

            foreach (var sourceEvent in sourceType.Events)
            {
                var eventCloner = new EventCloner(typeCloner, sourceEvent);
                this.Cloners.AddCloner(eventCloner);
                this.Visit(eventCloner);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="fieldCloner">Cloner for a field being cloned.</param>
        private void Visit(FieldCloner fieldCloner)
        {
            Contract.Requires(fieldCloner != null);

            this.GatherAndVisitCustomAttributeClonersFrom(fieldCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="methodSignatureCloner">Cloner for the method</param>
        private void Visit(MethodSignatureCloner methodSignatureCloner)
        {
            Contract.Requires(methodSignatureCloner != null);

            ParameterCloner previousParameterCloner = null;
            foreach (var sourceParameter in methodSignatureCloner.Source.Parameters)
            {
                var parameterCloner = new ParameterCloner(methodSignatureCloner, previousParameterCloner, sourceParameter);
                this.Cloners.AddCloner(parameterCloner);
                this.Visit(parameterCloner);
                previousParameterCloner = parameterCloner;
            }

            this.GatherAndVisitGenericParameterClonersFrom(methodSignatureCloner);
            this.GatherAndVisitCustomAttributeClonersFrom(methodSignatureCloner);

            var methodReturnTypeCloner = new MethodReturnTypeCloner(
                methodSignatureCloner,
                methodSignatureCloner.Source.MethodReturnType);
            this.Cloners.AddCloner(methodReturnTypeCloner);
            this.Visit(methodReturnTypeCloner);

            if (methodSignatureCloner.Source.HasBody)
            {
                var methodBodyCloner = new MethodBodyCloner(methodSignatureCloner, methodSignatureCloner.Source.Body);
                this.Cloners.AddCloner(methodBodyCloner);
                this.Visit(methodBodyCloner);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="methodBodyCloner">Cloner for the method body.</param>
        private void Visit(MethodBodyCloner methodBodyCloner)
        {
            Contract.Requires(methodBodyCloner!= null);

            VariableCloner previousVariableCloner = null;
            foreach (var sourceVariable in methodBodyCloner.Source.Variables)
            {
                var variableCloner = new VariableCloner(methodBodyCloner, previousVariableCloner, sourceVariable);
                this.Cloners.AddCloner(variableCloner);
                this.Visit(variableCloner);
                previousVariableCloner = variableCloner;
            }

            InstructionCloner previousInstructionCloner = null;
            foreach (var sourceInstruction in methodBodyCloner.Source.Instructions)
            {
                // the operand is required to create the instruction
                // but at this stage root resolving is not yet allowed because the tree of cloners is not yet completed
                // so, where needed, dummy operands are used which will be replaced in the clone step of each instruction cloner
                var instructionCloner = new InstructionCloner(methodBodyCloner, previousInstructionCloner, sourceInstruction);
                this.Cloners.AddCloner(instructionCloner);
                this.Visit(instructionCloner);
                previousInstructionCloner = instructionCloner;
            }

            foreach (var sourceExceptionHandler in methodBodyCloner.Source.ExceptionHandlers)
            {
                var exceptionHandlerCloner = new ExceptionHandlerCloner(methodBodyCloner, sourceExceptionHandler);
                this.Cloners.AddCloner(exceptionHandlerCloner);
                this.Visit(exceptionHandlerCloner);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="propertyCloner">Cloner for the property.</param>
        private void Visit(PropertyCloner propertyCloner)
        {
            Contract.Requires(propertyCloner != null);

            this.GatherAndVisitCustomAttributeClonersFrom(propertyCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="eventCloner">Cloner for event.</param>
        private void Visit(EventCloner eventCloner)
        {
            Contract.Requires(eventCloner != null);

            this.GatherAndVisitCustomAttributeClonersFrom(eventCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="genericParameterCloner">Cloner for generic parameter.</param>
        private void Visit(GenericParameterCloner genericParameterCloner)
        {
            Contract.Requires(genericParameterCloner != null);

            this.GatherAndVisitCustomAttributeClonersFrom(genericParameterCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="parameterCloner">Cloner for the parameter.</param>
        private void Visit(ParameterCloner parameterCloner)
        {
            Contract.Requires(parameterCloner != null);

            this.GatherAndVisitCustomAttributeClonersFrom(parameterCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="methodReturnTypeCloner">Cloner for the method return type.</param>
        private void Visit(MethodReturnTypeCloner methodReturnTypeCloner)
        {
            Contract.Requires(methodReturnTypeCloner != null);

            this.GatherAndVisitCustomAttributeClonersFrom(methodReturnTypeCloner);
        }

        /// <summary>
        /// A catch-all visit method that handles visits to cloners that need
        /// no special handling. This allows us to keep the pattern of always
        /// calling a visit method on every cloner without having to maintain
        /// several empty visit methods. If logic needs to be added for visiting
        /// any particular type of cloner, the method can be added, and the
        /// existing call will bind to the new method on recompile.
        /// </summary>
        /// <param name="cloner">Cloner that requires no special visit logic.</param>
        // ReSharper disable once UnusedParameter.Local
        private void Visit(ICloner<object, object> cloner)
        {
            Contract.Requires(cloner != null);
        }

        /// <summary>
        /// Gathers and visits all generic parameter cloners from the given
        /// generic parameter provider cloner.
        /// </summary>
        /// <param name="genericParameterProviderCloner">Cloner to look at.</param>
        private void GatherAndVisitGenericParameterClonersFrom(ICloner<IGenericParameterProvider> genericParameterProviderCloner)
        {
            Contract.Requires(genericParameterProviderCloner != null);

            GenericParameterCloner previousGenericParameterCloner = null;
            foreach (var sourceGenericParameter in genericParameterProviderCloner.Source.GenericParameters)
            {
                var genericParameterCloner = new GenericParameterCloner(
                    genericParameterProviderCloner,
                    previousGenericParameterCloner,
                    sourceGenericParameter);
                this.Cloners.AddCloner(genericParameterCloner);
                this.Visit(genericParameterCloner);
                previousGenericParameterCloner = genericParameterCloner;
            }
        }

        /// <summary>
        /// Gathers and visits all custom attribute cloners from the given
        /// custom attribute provider cloner.
        /// </summary>
        /// <param name="customAttributeProviderCloner">Cloner to look at.</param>
        private void GatherAndVisitCustomAttributeClonersFrom(ICloner<ICustomAttributeProvider> customAttributeProviderCloner)
        {
            Contract.Requires(customAttributeProviderCloner != null);

            foreach (var sourceCustomAttribute in customAttributeProviderCloner.Source.CustomAttributes)
            {
                var genericParameterCloner = new CustomAttributeCloner(
                    customAttributeProviderCloner,
                    sourceCustomAttribute);
                this.Cloners.AddCloner(genericParameterCloner);
                this.Visit(genericParameterCloner);
            }
        }
    }
}
