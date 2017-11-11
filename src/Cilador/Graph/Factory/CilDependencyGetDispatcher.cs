/***************************************************************************/
// Copyright 2013-2017 Riley White
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

using Cilador.Dispatch;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cilador.Graph.Factory
{
    internal sealed class CilDependencyGetDispatcher : CilItemFunctionDispatcherBase<IEnumerable<object>>
    {
        /// <summary>
        /// Creates a new <see cref="CilDependencyGetDispatcher"/>.
        /// </summary>
        public CilDependencyGetDispatcher()
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
        /// Handles non-CIL items.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<object> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get dependencies of a non-CIL item.");
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(AssemblyDefinition item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ModuleDefinition item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ExportedType item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(Resource item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(SecurityDeclaration item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(SecurityAttribute item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(TypeDefinition item)
        {
            // a type depends on its base type
            foreach (var dependency in item.BaseType.GetDependencies())
            {
                yield return dependency;
            }

            // a type depends on its interface types, if any
            if (item.HasInterfaces) foreach (var interfaceType in item.Interfaces)
            {
                foreach (var dependency in interfaceType.GetDependencies())
                {
                    yield return dependency;
                }
            }

            // if a type is an array or generic instance, then it depends on its element type 
            if (item.IsArray || item.IsGenericInstance)
            {
                foreach (var dependency in item.GetElementType().GetDependencies())
                {
                    yield return dependency;
                }
            }

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
            foreach (var dependency in item.FieldType.GetDependencies())
            {
                yield return dependency;
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodDefinition item)
        {
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
            foreach (var dependency in item.PropertyType.GetDependencies())
            {
                yield return dependency;
            }

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
            foreach (var dependency in item.EventType.GetDependencies())
            {
                yield return dependency;
            }
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
                foreach (var dependency in type.GetDependencies())
                {
                    yield return dependency;
                }
            }
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ParameterDefinition item)
        {
            return item.ParameterType.GetDependencies();
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodReturnType item)
        {
            return item.ReturnType.GetDependencies();
        }

        /// <summary>
        /// Gets the dependencies of the given item.
        /// </summary>
        /// <param name="item">Item to get dependencies of.</param>
        /// <returns>Dependencies of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(CustomAttribute item)
        {
            foreach (var dependency in item.AttributeType.GetDependencies())
            {
                yield return dependency;
            }

            foreach (var dependency in item.Constructor.GetDependencies())
            {
                yield return dependency;
            }

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
            return item.VariableType.GetDependencies();
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
