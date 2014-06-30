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
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <typeparam name="T">Type of item to gather cloners for.</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        /// <exception cref="ArgumentException">Raised when the type <typeparamref name="T"/> cannot be cloned.</exception>
        public void Visit<T>(T source, T target)
            where T : class
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);

            try { this.Visit((dynamic)source, (dynamic)target); }
            catch (MissingMethodException mme)
            {
                throw new ArgumentException(string.Format("Unable to gather cloners for type [{0}]", typeof(T).FullName), "source", mme);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <typeparam name="T">Type of item to gather cloners for</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(TypeDefinition sourceType, TypeDefinition targetType)
        {
            Contract.Requires(sourceType != null);
            Contract.Requires(targetType != null);

            this.Cloners.AddCloner(new TypeCloner(this.ILCloningContext, targetType, sourceType));

            var voidReference = targetType.Module.Import(typeof(void));

            foreach (var sourceNestedType in sourceType.NestedTypes)
            {
                var targetNestedType = new TypeDefinition(sourceNestedType.Namespace, sourceNestedType.Name, 0);
                targetType.NestedTypes.Add(targetNestedType);

                // TODO may need to traverse hierarchy of generic parameters
                foreach (var sourceGenericParameter in sourceNestedType.GenericParameters)
                {
                    var targetGenericParameter = new GenericParameter(sourceGenericParameter.Name, targetNestedType);
                    targetNestedType.GenericParameters.Add(targetGenericParameter);
                    this.Cloners.AddCloner(new GenericParameterCloner(this.ILCloningContext, targetGenericParameter, sourceGenericParameter));
                }
                
                this.Visit(sourceNestedType, targetNestedType);
            }

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
                    // TODO support constructors for the root type in some meaningful way
                    if (sourceMethod.HasParameters)
                    {
                        throw new WeavingException(string.Format(
                            "Configured mixin implementation cannot use constructors: [{0}]",
                            this.ILCloningContext.RootSource.FullName));
                    }
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
        /// <typeparam name="T">Type of item to gather cloners for</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(FieldDefinition sourceField, FieldDefinition targetField)
        {
            Contract.Requires(sourceField != null);
            Contract.Requires(targetField != null);

            this.Cloners.AddCloner(new FieldCloner(this.ILCloningContext, targetField, sourceField));
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <typeparam name="T">Type of item to gather cloners for</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(MethodDefinition sourceMethod, MethodDefinition targetMethod)
        {
            Contract.Requires(sourceMethod != null);
            Contract.Requires(targetMethod != null);

            var methodSignatureCloner = new MethodSignatureCloner(this.ILCloningContext, targetMethod, sourceMethod);
            this.Cloners.AddCloner(methodSignatureCloner);

            Contract.Assert(methodSignatureCloner.ParameterCloners != null);
            this.Cloners.AddCloners(methodSignatureCloner.ParameterCloners);

            // TODO may need to traverse hierarchy of generic parameters
            foreach (var sourceGenericParameter in sourceMethod.GenericParameters)
            {
                var targetGenericParameter = new GenericParameter(sourceGenericParameter.Name, targetMethod);
                targetMethod.GenericParameters.Add(targetGenericParameter);
                this.Cloners.AddCloner(new GenericParameterCloner(this.ILCloningContext, targetGenericParameter, sourceGenericParameter));
            }

            if (sourceMethod.HasBody)
            {
                var methodBodyCloner = new MethodBodyCloner(methodSignatureCloner, targetMethod.Body, sourceMethod.Body);
                this.Cloners.AddCloner(methodBodyCloner);
                this.Cloners.AddCloners(methodBodyCloner.VariableCloners);
                this.Cloners.AddCloners(methodBodyCloner.InstructionCloners);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <typeparam name="T">Type of item to gather cloners for</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(PropertyDefinition sourceProperty, PropertyDefinition targetProperty)
        {
            Contract.Requires(sourceProperty != null);
            Contract.Requires(targetProperty != null);

            var propertyCloner = new PropertyCloner(this.ILCloningContext, targetProperty, sourceProperty);
            this.Cloners.AddCloner(propertyCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <typeparam name="T">Type of item to gather cloners for</typeparam>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(EventDefinition sourceEvent, EventDefinition targetEvent)
        {
            Contract.Requires(sourceEvent != null);
            Contract.Requires(targetEvent != null);

            this.Cloners.AddCloner(new EventCloner(this.ILCloningContext, targetEvent, sourceEvent));
        }
    }
}
