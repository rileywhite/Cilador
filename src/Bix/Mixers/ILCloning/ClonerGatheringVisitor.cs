﻿/***************************************************************************/
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
        /// <param name="rootTypeCloner">Cloner to gather child cloners for.</param>
        private void Visit(NestedTypeCloner typeCloner)
        {
            Contract.Requires(typeCloner != null);

            this.Visit((ClonerBase<TypeDefinition>)typeCloner);

            this.Visit(
                (IGenericParameterProvider)typeCloner.Source,
                (IGenericParameterProvider)typeCloner.Target); // TODO don't access target
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="typeCloner">Cloner to gather child cloners for.</param>
        private void Visit(ClonerBase<TypeDefinition> typeCloner)
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
            var voidReference = typeCloner.ILCloningContext.RootTarget.Module.Import(typeof(void)); // TODO get rid of void ref

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

                MethodDefinition targetMethod = null;

                if (sourceMethod.IsConstructor &&
                    sourceMethod.IsStatic &&
                    sourceMethod.DeclaringType == this.ILCloningContext.RootSource)
                {
                    var targetStaticConstructor = this.ILCloningContext.RootTarget.Methods.SingleOrDefault(method => method.IsConstructor && method.IsStatic);
                    // if there is no static constructor in the target, then treat it like any other method
                    // otherwise we need to merge the methods
                    if (targetStaticConstructor != null)
                    {
                        // if there is already a target static constructor, then redirect the clone to a different method
                        // and then add a call to the new method into the existing static constructor
                        targetMethod = new MethodDefinition(
                            string.Format("cctor_{0:N}", Guid.NewGuid()),
                            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                            voidReference);
                        targetType.Methods.Add(targetMethod);
                        this.VisitStaticConstructor(targetStaticConstructor, targetMethod);
                    }
                }

                if (targetMethod == null)
                {
                    targetMethod = new MethodDefinition(sourceMethod.Name, sourceMethod.Attributes, voidReference);
                    targetType.Methods.Add(targetMethod);
                }
                this.Visit(sourceMethod, targetMethod);
            }

            foreach (var sourceProperty in sourceType.Properties)
            {
                var propertyCloner = new PropertyCloner(typeCloner, sourceProperty);
                this.Cloners.AddCloner(propertyCloner);
                this.Visit(propertyCloner);
            }

            foreach (var sourceEvent in sourceType.Events)
            {
                var targetEvent = new EventDefinition(sourceEvent.Name, 0, voidReference);
                targetType.Events.Add(targetEvent);
                this.Visit(sourceEvent, targetEvent);
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
        /// <param name="sourceMethod">Cloning source to gather cloners for.</param>
        /// <param name="targetMethod">Cloning target to gather cloners for.</param>
        private void Visit(MethodDefinition sourceMethod, MethodDefinition targetMethod)
        {
            Contract.Requires(sourceMethod != null);
            Contract.Requires(targetMethod != null);

            var methodSignatureCloner = new MethodSignatureCloner(this.ILCloningContext, sourceMethod, targetMethod);
            this.Cloners.AddCloner(methodSignatureCloner);

            Contract.Assert(methodSignatureCloner.ParameterCloners != null);
            this.Cloners.AddCloners(methodSignatureCloner.ParameterCloners);

            this.Visit(
                (IGenericParameterProvider)sourceMethod,
                (IGenericParameterProvider)targetMethod);

            if (sourceMethod.HasBody)
            {
                var methodBodyCloner = new MethodBodyCloner(methodSignatureCloner, sourceMethod.Body, targetMethod.Body);
                this.Cloners.AddCloner(methodBodyCloner);
                this.Cloners.AddCloners(methodBodyCloner.VariableCloners);
                this.Cloners.AddCloners(methodBodyCloner.InstructionCloners);
                this.Cloners.AddCloners(methodBodyCloner.ExceptionHandlerCloners);
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
        /// <param name="sourceEvent">Cloning source to gather cloners for.</param>
        /// <param name="targetEvent">Cloning target to gather cloners for.</param>
        private void Visit(EventDefinition sourceEvent, EventDefinition targetEvent)
        {
            Contract.Requires(sourceEvent != null);
            Contract.Requires(targetEvent != null);

            this.Cloners.AddCloner(new EventCloner(this.ILCloningContext, sourceEvent, targetEvent));
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="sourceGenericParameterProvider">Cloning source to gather cloners for.</param>
        /// <param name="targetGenericParameterProvider">Cloning target to gather cloners for.</param>
        private void Visit(IGenericParameterProvider sourceGenericParameterProvider, IGenericParameterProvider targetGenericParameterProvider)
        {
            Contract.Requires(sourceGenericParameterProvider != null);
            Contract.Requires(targetGenericParameterProvider != null);

            var voidReference = targetGenericParameterProvider.Module.Import(typeof(void));
            foreach (var sourceGenericParameter in sourceGenericParameterProvider.GenericParameters)
            {
                // save the parameter in a local variable
                // because compiler versions differ on handling of foreach parameters within closures
                var currentSourceGenericParameter = sourceGenericParameter;

                targetGenericParameterProvider.GenericParameters.Add(new GenericParameter(voidReference)); // this is just a placeholder since null is not allowed
                this.Cloners.AddCloner(new GenericParameterCloner(
                    this.ILCloningContext,
                    currentSourceGenericParameter,
                    () => targetGenericParameterProvider.GenericParameters[currentSourceGenericParameter.Position],
                    targetGenericParameter => targetGenericParameterProvider.GenericParameters[currentSourceGenericParameter.Position] = targetGenericParameter));
            }
        }

        /// <summary>
        /// Gathers cloners for cloning the source static constructor instructions and variables into the existing
        /// target static constructor.
        /// </summary>
        /// <param name="targetStaticConstructor">Target static constructor.</param>
        /// <param name="sourceStaticConstructorTarget">New target method which will contain contents of the original source static constructor.</param>
        private void VisitStaticConstructor(MethodDefinition targetStaticConstructor, MethodDefinition sourceStaticConstructorTarget)
        {
            Contract.Requires(targetStaticConstructor != null);
            Contract.Requires(targetStaticConstructor.IsConstructor);
            Contract.Requires(targetStaticConstructor.IsStatic);
            Contract.Requires(targetStaticConstructor.HasBody);
            Contract.Requires(targetStaticConstructor.DeclaringType == this.ILCloningContext.RootTarget);
            Contract.Requires(sourceStaticConstructorTarget != null);
            Contract.Requires(sourceStaticConstructorTarget.DeclaringType == this.ILCloningContext.RootTarget);

            var firstInstruction = targetStaticConstructor.Body.Instructions[0];
            var targetILProcessor = targetStaticConstructor.Body.GetILProcessor();

            targetILProcessor.InsertBefore(firstInstruction, targetILProcessor.Create(OpCodes.Call, sourceStaticConstructorTarget));
        }
    }
}
