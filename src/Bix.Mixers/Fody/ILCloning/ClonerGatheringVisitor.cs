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
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
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
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(TypeDefinition sourceType, TypeDefinition targetType)
        {
            Contract.Requires(sourceType != null);
            Contract.Requires(targetType != null);

            this.Cloners.AddCloner(new TypeCloner(this.ILCloningContext, sourceType, targetType));

            foreach (var sourceNestedType in sourceType.NestedTypes)
            {
                var targetNestedType = new TypeDefinition(sourceNestedType.Namespace, sourceNestedType.Name, 0);
                targetType.NestedTypes.Add(targetNestedType);

                this.Visit(
                    (IGenericParameterProvider)sourceNestedType,
                    (IGenericParameterProvider)targetNestedType);
                
                this.Visit(sourceNestedType, targetNestedType);
            }

            var voidReference = targetType.Module.Import(typeof(void));

            foreach (var sourceField in sourceType.Fields)
            {
                var targetField = new FieldDefinition(sourceField.Name, 0, voidReference);
                targetType.Fields.Add(targetField);
                this.Visit(sourceField, targetField);
            }

            foreach (var sourceMethod in sourceType.Methods)
            {
                if (sourceMethod.Name == ".cctor" &&
                    sourceMethod.IsStatic &&
                    sourceMethod.DeclaringType == ILCloningContext.RootSource)
                {
                    // TODO should static constructors be supported on the root type?
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation cannot have a type initializer (i.e. static constructor): [{0}]",
                        this.ILCloningContext.RootSource.FullName));
                }

                if (sourceMethod.IsConstructor &&
                    sourceMethod.DeclaringType == ILCloningContext.RootSource)
                {
                    if (sourceMethod.HasParameters)
                    {
                        // TODO support constructors for the root type in some meaningful way
                        throw new WeavingException(string.Format(
                            "Configured mixin implementation cannot use constructors: [{0}]",
                            this.ILCloningContext.RootSource.FullName));
                    }

                    var constructorBroadcaster = new ConstructorBroadcaster(this.ILCloningContext, sourceMethod, targetType);
                    constructorBroadcaster.BroadcastConstructor();
                    this.Cloners.AddCloners(constructorBroadcaster.VariableCloners);
                    this.Cloners.AddCloners(constructorBroadcaster.InstructionCloners);

                    continue;
                }

                var targetMethod = new MethodDefinition(sourceMethod.Name, 0, voidReference);
                targetType.Methods.Add(targetMethod);
                this.Visit(sourceMethod, targetMethod);
            }

            foreach (var sourceProperty in sourceType.Properties)
            {
                var targetProperty = new PropertyDefinition(sourceProperty.Name, 0, voidReference);
                targetType.Properties.Add(targetProperty);
                this.Visit(sourceProperty, targetProperty);
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
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(FieldDefinition sourceField, FieldDefinition targetField)
        {
            Contract.Requires(sourceField != null);
            Contract.Requires(targetField != null);

            this.Cloners.AddCloner(new FieldCloner(this.ILCloningContext, sourceField, targetField));
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
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
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(PropertyDefinition sourceProperty, PropertyDefinition targetProperty)
        {
            Contract.Requires(sourceProperty != null);
            Contract.Requires(targetProperty != null);

            var propertyCloner = new PropertyCloner(this.ILCloningContext, sourceProperty, targetProperty);
            this.Cloners.AddCloner(propertyCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(EventDefinition sourceEvent, EventDefinition targetEvent)
        {
            Contract.Requires(sourceEvent != null);
            Contract.Requires(targetEvent != null);

            this.Cloners.AddCloner(new EventCloner(this.ILCloningContext, sourceEvent, targetEvent));
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(IGenericParameterProvider sourceGenericParameterProvider, IGenericParameterProvider targetGenericParameterProvider)
        {
            var voidReference = targetGenericParameterProvider.Module.Import(typeof(void));
            foreach (var sourceGenericParameter in sourceGenericParameterProvider.GenericParameters)
            {
                targetGenericParameterProvider.GenericParameters.Add(new GenericParameter(voidReference)); // this is just a placeholder since null is not allowed
                this.Cloners.AddCloner(new GenericParameterCloner(
                    this.ILCloningContext,
                    sourceGenericParameter,
                    () => targetGenericParameterProvider.GenericParameters[sourceGenericParameter.Position],
                    targetGenericParameter => targetGenericParameterProvider.GenericParameters[sourceGenericParameter.Position] = targetGenericParameter));
            }
        }
    }
}
