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

using Cilador.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cilador.Graph
{
    internal sealed class ILDependencyGetDispatcher : ILItemFunctionDispatcherBase<IEnumerable<object>>
    {
        /// <summary>
        /// Creates a new <see cref="ILDependencyGetDispatcher"/>.
        /// </summary>
        public ILDependencyGetDispatcher()
        {
            Contract.Ensures(this.OperandDependencyGetter != null);

            this.OperandDependencyGetter = new OperandDependenciesGetDispatcher();
        }

        /// <summary>
        /// Handles null items.
        /// </summary>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<object> InvokeForNull()
        {
            throw new NotSupportedException("Cannot get dependencies of a null item.");
        }

        /// <summary>
        /// Handles non-IL items.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<object> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get dependencies of a non-IL item.");
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(TypeDefinition item)
        {
            // a type depends on its base type
            if (item.BaseType != null) { yield return item.BaseType.Resolve(); }

            // a type depends on its containing type, if any
            if (item.DeclaringType != null) { yield return item.DeclaringType; }

            // a type depends on its interface types, if any
            if (item.HasInterfaces) foreach (var interfaceType in item.Interfaces)
            {
                yield return interfaceType.Resolve();
            }

            // if a type is an array or generic instance, then it depends on its element type 
            if (item.IsArray || item.IsGenericInstance) { yield return item.GetElementType().Resolve(); }

            // a type depends on its generic parameters
            if (item.HasGenericParameters) foreach (var genericParameter in item.GenericParameters)
            {
                yield return genericParameter;
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(FieldDefinition item)
        {
            yield return item.DeclaringType;
            yield return item.FieldType.Resolve();
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodDefinition item)
        {
            yield return item.DeclaringType;

            yield return item.MethodReturnType;

            if (item.HasParameters) foreach (var parameter in item.Parameters)
            {
                yield return parameter;
            }

            if (item.HasGenericParameters) foreach (var genericParameter in item.GenericParameters)
            {
                yield return genericParameter;
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodBody item)
        {
            yield return item.Method;
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(PropertyDefinition item)
        {
            yield return item.DeclaringType;
            yield return item.PropertyType.Resolve();

            if (item.HasParameters) foreach (var parameter in item.Parameters)
            {
                yield return parameter;
            }

            if (item.GetMethod != null) { yield return item.GetMethod; }
            if (item.SetMethod != null) { yield return item.SetMethod; }

            if (item.HasOtherMethods) foreach (var otherMethod in item.OtherMethods)
            {
                yield return otherMethod;
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(EventDefinition item)
        {
            yield return item.DeclaringType;
            yield return item.EventType.Resolve();
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(GenericParameter item)
        {
            if (item.HasGenericParameters) foreach (var genericParameter in item.GenericParameters)
            {
                yield return genericParameter;
            }

            if (item.HasConstraints) foreach (var type in item.Constraints)
            {
                yield return type.Resolve();
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ParameterDefinition item)
        {
            yield return item.ParameterType.Resolve();
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodReturnType item)
        {
            yield return item.ReturnType.Resolve();
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(CustomAttribute item)
        {
            yield return item.AttributeType.Resolve();
            yield return item.Constructor.Resolve();
            if (item.HasConstructorArguments) foreach (var constructorArgument in item.ConstructorArguments)
            {
                yield return constructorArgument.Value;
            }

            if (item.HasFields) foreach (var field in item.Fields)
            {
                yield return field.Argument.Value;
            }

            if (item.HasProperties) foreach (var property in item.Properties)
            {
                yield return property.Argument.Value;
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(VariableDefinition item)
        {
            // a variable depends on its type
            yield return item.VariableType.Resolve();
        }

        /// <summary>
        /// Gets or sets the dispatcher for collecting dependencies based on an instruction operand.
        /// </summary>
        private OperandDependenciesGetDispatcher OperandDependencyGetter { get; set; }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(Instruction item)
        {
            // an instruction depends on its operand if there is one
            return this.OperandDependencyGetter.InvokeFor(item);
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ExceptionHandler item)
        {
            // an exception handler depends on its catch type if there is one
            if (item.CatchType != null) { yield return item.CatchType.Resolve(); }

            // an exception handler depends on its instruction boundaries
            if (item.TryStart != null) { yield return item.TryStart; }
            if (item.TryEnd != null) { yield return item.TryEnd; }
            if (item.HandlerStart != null) { yield return item.HandlerStart; }
            if (item.HandlerEnd != null) { yield return item.HandlerEnd; }
            if (item.FilterStart != null) { yield return item.FilterStart; }
        }
    }
}
