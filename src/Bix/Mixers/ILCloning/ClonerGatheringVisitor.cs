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
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

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
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="rootTypeCloner">Cloner to gather child cloners for.</param>
        public void Visit(RootTypeCloner rootTypeCloner)
        {
            Contract.Requires(rootTypeCloner != null);
            this.Cloners.AddCloner(rootTypeCloner);
            this.Visit((ClonerBase<TypeDefinition>)rootTypeCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="nestedTypeCloner">Cloner to gather child cloners for.</param>
        private void Visit(NestedTypeCloner nestedTypeCloner)
        {
            Contract.Requires(nestedTypeCloner != null);

            this.Visit((ClonerBase<TypeDefinition>)nestedTypeCloner);

            GenericParameterCloner previousGenericParameterCloner = null;
            foreach (var sourceGenericParameter in nestedTypeCloner.Source.GenericParameters)
            {
                var genericParameterCloner = new GenericParameterCloner(
                    nestedTypeCloner,
                    previousGenericParameterCloner,
                    sourceGenericParameter);
                this.Cloners.AddCloner(genericParameterCloner);
                this.Visit(genericParameterCloner);
            }
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

            var targetType = typeCloner.Target;     // TODO Don't access Target

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
                    if (sourceMethod.HasParameters)
                    {
                        // at some point in the future if it becomes clear that it would be useful,
                        // it may be possible to create all combinations of source and target constructor
                        // arguments and put them into the final mixed target
                        // but that's a complex and time-consuming task with unknown payoff
                        // so for now we don't support mixin implementations that have constructors with parameters
                        throw new InvalidOperationException(string.Format(
                            "Configured mixin implementation cannot use constructors with parameters: [{0}]",
                            this.ILCloningContext.RootSource.FullName));
                    }

                    // for a parameterless constructor, we need to inject it into every target constructor
                    var constructorBroadcaster = new ConstructorBroadcaster(this.ILCloningContext, sourceMethod, targetType);
                    constructorBroadcaster.BroadcastConstructor();
                    this.Cloners.AddCloners(constructorBroadcaster.VariableCloners);
                    this.Cloners.AddCloners(constructorBroadcaster.InstructionCloners);

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

            GenericParameterCloner previousGenericParameterCloner = null;
            foreach (var sourceGenericParameter in methodSignatureCloner.Source.GenericParameters)
            {
                var genericParameterCloner = new GenericParameterCloner(
                    methodSignatureCloner,
                    previousGenericParameterCloner,
                    sourceGenericParameter);
                this.Cloners.AddCloner(genericParameterCloner);
                this.Visit(genericParameterCloner);
            }

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

            var ilProcessor = methodBodyCloner.Target.GetILProcessor();
            foreach (var sourceInstruction in methodBodyCloner.Source.Instructions)
            {
                // the operand is required to create the instruction
                // but at this stage root resolving is not yet allowed because the tree of cloners is not yet completed
                // so, where needed, dummy operands are used which will be replaced in the clone step of each instruction cloner
                Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(new MethodContext(methodBodyCloner), ilProcessor, sourceInstruction);
                ilProcessor.Append(targetInstruction);
                var instructionCloner = new InstructionCloner(methodBodyCloner, sourceInstruction, targetInstruction);
                this.Cloners.AddCloner(instructionCloner);
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
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="sourceEvent">Cloner for event.</param>
        private void Visit(EventCloner sourceEvent)
        {
            Contract.Requires(sourceEvent != null);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="genericParameterCloner">Cloner to visit.</param>
        private void Visit(GenericParameterCloner genericParameterCloner)
        {
            Contract.Requires(genericParameterCloner != null);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="parameterCloner">Cloner for the parameter.</param>
        private void Visit(ParameterCloner parameterCloner)
        {
            Contract.Requires(parameterCloner != null);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="exceptionHandlerCloner">Cloner for the exception handler.</param>
        private void Visit(ExceptionHandlerCloner exceptionHandlerCloner)
        {
            Contract.Requires(exceptionHandlerCloner != null);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="variableCloner">Cloner for the variable.</param>
        private void Visit(VariableCloner variableCloner)
        {
            Contract.Requires(variableCloner != null);
        }
    }
}
