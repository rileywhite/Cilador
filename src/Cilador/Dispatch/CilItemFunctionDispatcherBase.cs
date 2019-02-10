/***************************************************************************/
// Copyright 2013-2019 Riley White
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

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Dispatch
{
    /// <summary>
    /// Base for types that define behavior to be run against CIL items where
    /// the behavior may vary based on the item type.
    /// </summary>
    /// <typeparam name="TReturn">Type of the return value for the behavior.</typeparam>
    [ContractClass(typeof(CilItemFunctionDispatcherContract<>))]
    public abstract class CilItemFunctionDispatcherBase<TReturn>
    {
        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        public TReturn InvokeFor(dynamic item)
        {
            return item == null ? this.InvokeForNull() : this.InvokeForItem(item);
        }

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        private TReturn InvokeForItem(object item)
        {
            Contract.Requires(item != null);

            if (DynamicResolver.DynamicTryResolve(item, out object resolvedItem))
            {
                // if a resolvable reference was passed in, that would be considered an
                // error because it means that the source graph may contain a reference where
                // a definition was expected
                throw new InvalidOperationException(string.Format(
                    "Cannot invoke on resolvable items of type {0}. Resolve the item first.",
                    item.GetType().FullName));
            }

            return this.InvokeForNonILItem(item);
        }

        /// <summary>
        /// Defines the behavior for <c>null</c> values.
        /// </summary>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForNull();

        /// <summary>
        /// Invokes the behavior for items that are not CIL items.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForNonILItem(object item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(AssemblyDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(ModuleDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(ExportedType item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(Resource item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(SecurityDeclaration item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(SecurityAttribute item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(TypeDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(FieldDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(MethodDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(MethodBody item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(PropertyDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(EventDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(GenericParameter item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(ParameterDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(MethodReturnType item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(CustomAttribute item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(VariableDefinition item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(Instruction item);

        /// <summary>
        /// Invokes the behavior.
        /// </summary>
        /// <param name="item">Item on which to invoke the behavior.</param>
        /// <returns>Result of the invocation.</returns>
        protected abstract TReturn InvokeForItem(ExceptionHandler item);
    }
}
