/***************************************************************************/
// Copyright 2013-2016 Riley White
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

namespace Cilador.Graph
{
    /// <summary>
    /// Gets child items for CIL items in a way that preserves groupings of child types (e.g. types are together,
    /// fields are together, etc), and in a way that order is preserved where it matters, such as for parameters,
    /// generic parameters, and variables.
    /// </summary>
    internal sealed class CilOrderedChildrenGetDispatcher : CilItemFunctionDispatcherBase<IEnumerable<object>>
    {
        /// <summary>
        /// Handles null items.
        /// </summary>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<object> InvokeForNull()
        {
            throw new NotSupportedException("Cannot get children of a null item.");
        }

        /// <summary>
        /// Handles non-CIL items.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<object> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get children of a non-CIL item.");
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(AssemblyDefinition item)
        {
            foreach (var module in item.Modules)
            {
                yield return module;
            }

            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }

            if (item.HasSecurityDeclarations) foreach (var securityDeclaration in item.SecurityDeclarations)
            {
                yield return securityDeclaration;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ModuleDefinition item)
        {
            if (item.HasTypes) foreach (var type in item.Types)
            {
                yield return type;
            }

            if (item.HasExportedTypes) foreach (var exportedType in item.ExportedTypes)
            {
                yield return exportedType;
            }

            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }

            if (item.HasResources) foreach (var resource in item.Resources)
            {
                yield return resource;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ExportedType item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(Resource item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(SecurityDeclaration item)
        {
            return item.HasSecurityAttributes ?
                (IEnumerable<object>)item.SecurityAttributes :
                new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(SecurityAttribute item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(TypeDefinition item)
        {
            if (item.HasNestedTypes) foreach (var nestedType in item.NestedTypes)
            {
                yield return nestedType;
            }

            if (item.HasFields) foreach (var field in item.Fields)
            {
                yield return field;
            }

            if (item.HasMethods) foreach (var method in item.Methods)
            {
                yield return method;
            }

            if (item.HasProperties) foreach (var property in item.Properties)
            {
                yield return property;
            }

            if (item.HasEvents) foreach (var @event in item.Events)
            {
                yield return @event;
            }

            if (item.HasGenericParameters) foreach (var genericParameter in item.GenericParameters)
            {
                yield return genericParameter;
            }

            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }

            if (item.HasSecurityDeclarations) foreach (var securityDeclaration in item.SecurityDeclarations)
            {
                yield return securityDeclaration;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(FieldDefinition item)
        {
            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodDefinition item)
        {
            yield return item.MethodReturnType;

            if (item.HasParameters) foreach (var parameter in item.Parameters)
            {
                yield return parameter;
            }

            if (item.HasBody) { yield return item.Body; }

            if (item.HasGenericParameters) foreach (var genericParameter in item.GenericParameters)
            {
                yield return genericParameter;
            }

            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }

            if (item.HasSecurityDeclarations) foreach (var securityDeclaration in item.SecurityDeclarations)
            {
                yield return securityDeclaration;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodBody item)
        {
            if (item.HasVariables) foreach (var variable in item.Variables)
            {
                yield return variable;
            }

            foreach (var instruction in item.Instructions)
            {
                yield return instruction;
            }

            foreach (var exceptionHandler in item.ExceptionHandlers)
            {
                yield return exceptionHandler;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(PropertyDefinition item)
        {
            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(EventDefinition item)
        {
            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(GenericParameter item)
        {
            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }

            if (item.HasGenericParameters) foreach (var genericParameter in item.GenericParameters)
            {
                yield return genericParameter;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ParameterDefinition item)
        {
            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodReturnType item)
        {
            if (item.HasCustomAttributes) foreach (var customAttribute in item.CustomAttributes)
            {
                yield return customAttribute;
            }
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(CustomAttribute item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(VariableDefinition item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(Instruction item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the children of the given item.
        /// </summary>
        /// <param name="item">Item to get children of.</param>
        /// <returns>Children of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ExceptionHandler item)
        {
            return new object[0];
        }
    }
}
