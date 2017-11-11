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

namespace Cilador.Graph.Factory
{
    /// <summary>
    /// Gets parent items for CIL items. Many items (instructions, variables, etc) do not have efficiently
    /// accessible parent items. As a general rule, these items do not stand alone well, so it's okay
    /// that these items to not return a parent. Other items, such as modules and assemblies are never
    /// returned as they are not meaningful at this time.
    /// </summary>
    internal sealed class CilParentGetDispatcher : CilItemFunctionDispatcherBase<IEnumerable<object>>
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
            throw new NotSupportedException("Cannot get parent of a null item.");
        }

        /// <summary>
        /// Handles non-CIL items.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<object> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get parent of a non-CIL item.");
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(AssemblyDefinition item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ModuleDefinition item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ExportedType item)
        {
            return new object[] { item.DeclaringType };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(Resource item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(SecurityDeclaration item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(SecurityAttribute item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(TypeDefinition item)
        {
            if (item.IsNested)
            {
                return new object[] { item.DeclaringType };
            }
            else
            {
                return new object[0];
            }
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(FieldDefinition item)
        {
            return new object[] { item.DeclaringType };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodDefinition item)
        {
            return new object[] { item.DeclaringType };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodBody item)
        {
            return new object[] { item.Method };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(PropertyDefinition item)
        {
            return new object[] { item.DeclaringType };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(EventDefinition item)
        {
            return new object[] { item.DeclaringType };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(GenericParameter item)
        {
            if (item.DeclaringMethod != null)
            {
                return new object[] { item.DeclaringMethod };
            }
            else
            {
                return new object[] { item.DeclaringType };
            }
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ParameterDefinition item)
        {
            return new object[] { item.Method };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(MethodReturnType item)
        {
            return new object[] { item.Method };
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(CustomAttribute item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(VariableDefinition item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(Instruction item)
        {
            return new object[0];
        }

        /// <summary>
        /// Gets the parent of the given item.
        /// </summary>
        /// <param name="item">Item to get parent of.</param>
        /// <returns>Parent of <paramref name="item"/>.</returns>
        protected override IEnumerable<object> InvokeForItem(ExceptionHandler item)
        {
            return new object[0];
        }
    }
}
