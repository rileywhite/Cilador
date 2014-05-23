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
    internal class ClonerGatheringVisitor
    {
        /// <summary>
        /// Create a new <see cref="ClonerGatheringVisitor"/>
        /// </summary>
        public ClonerGatheringVisitor(ILCloningContext ilCloningContext)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Ensures(this.ILCloningContext != null);

            this.ILCloningContext = ilCloningContext;

            this.TypeCloners = new List<TypeCloner>();
            this.FieldCloners = new List<FieldCloner>();
            this.MethodSignatureCloners = new List<MethodSignatureCloner>();
            this.MethodParameterCloners = new List<ParameterCloner>();
            this.MethodBodyCloners = new List<MethodBodyCloner>();
            this.VariableCloners = new List<VariableCloner>();
            this.PropertyCloners = new List<PropertyCloner>();
            this.EventCloners = new List<EventCloner>();
        }

        /// <summary>
        /// Invokes cloners that have been gathered.
        /// </summary>
        public void InvokeCloners()
        {
            this.TypeCloners.Clone();
            this.FieldCloners.Clone();
            this.MethodSignatureCloners.Clone();
            this.MethodParameterCloners.Clone();
            this.PropertyCloners.Clone();
            this.EventCloners.Clone();
            this.VariableCloners.Clone();
            this.MethodBodyCloners.Clone();
        }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        public ILCloningContext ILCloningContext { get; private set; }

        /// <summary>
        /// Gets or sets the collection of cloners for nested types.
        /// </summary>
        private List<TypeCloner> TypeCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained fields.
        /// </summary>
        private List<FieldCloner> FieldCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained method signatures.
        /// </summary>
        private List<MethodSignatureCloner> MethodSignatureCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for method parameters within contained items.
        /// </summary>
        private List<ParameterCloner> MethodParameterCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained method bodies.
        /// </summary>
        private List<MethodBodyCloner> MethodBodyCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for variables contained within method bodies.
        /// </summary>
        private List<VariableCloner> VariableCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained properties.
        /// </summary>
        private List<PropertyCloner> PropertyCloners { get; set; }

        /// <summary>
        /// Gets or sets the collection of cloners for contained events.
        /// </summary>
        private List<EventCloner> EventCloners { get; set; }

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

            this.TypeCloners.Add(new TypeCloner(this.ILCloningContext, targetType, sourceType));

            var voidReference = targetType.Module.Import(typeof(void));

            foreach (var sourceNestedType in sourceType.NestedTypes)
            {
                var targetNestedType = new TypeDefinition(sourceNestedType.Namespace, sourceNestedType.Name, 0);
                targetType.NestedTypes.Add(targetNestedType);
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

            this.FieldCloners.Add(new FieldCloner(this.ILCloningContext, targetField, sourceField));
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
            this.MethodSignatureCloners.Add(methodSignatureCloner);

            Contract.Assert(methodSignatureCloner.ParameterCloners != null);
            this.MethodParameterCloners.AddRange(methodSignatureCloner.ParameterCloners);

            if (sourceMethod.HasBody)
            {
                var methodBodyCloner = new MethodBodyCloner(methodSignatureCloner, targetMethod.Body, sourceMethod.Body);
                this.MethodBodyCloners.Add(methodBodyCloner);
                this.VariableCloners.AddRange(methodBodyCloner.VariableCloners);
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
            this.PropertyCloners.Add(propertyCloner);
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

            this.EventCloners.Add(new EventCloner(this.ILCloningContext, targetEvent, sourceEvent));
        }
    }
}
