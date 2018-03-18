/***************************************************************************/
// Copyright 2013-2018 Riley White
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

using Cilador.Graph.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;

namespace Cilador.Clone
{
    /// <summary>
    /// Defines an interface for a cloning context.
    /// </summary>
    [ContractClass(typeof(CloningContextContract))]
    public interface ICloningContext
    {
        /// <summary>
        /// Gets the CilGraph of items for the cloning operation.
        /// </summary>
        ICilGraph CilGraph { get; }

        /// <summary>
        /// Gets the top level source type for the cloning operation.
        /// </summary>
        TypeDefinition RootSource { get; }

        /// <summary>
        /// Gets the top level source type for the cloning operation.
        /// </summary>
        TypeDefinition RootTarget { get; }

        /// <summary>
        /// Root import an item when the exact item type may not be known.
        /// </summary>
        /// <typeparam name="TItem">Type of the item to the precision to which it is known. Might just be <see cref="object"/>.</typeparam>
        /// <param name="item">Item to root import.</param>
        /// <returns>Root imported item.</returns>
        TItem DynamicRootImport<TItem>(TItem item);

        /// <summary>
        /// Root imports a type. That is, it finds the type with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="type">Type to root import.</param>
        /// <returns>Root imported type.</returns>
        TypeReference RootImport(TypeReference type);

        /// <summary>
        /// Root imports a method. That is, it finds the method with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="method">Method to root import.</param>
        /// <returns>Root imported method.</returns>
        MethodReference RootImport(MethodReference method);

        /// <summary>
        /// Root imports a field. That is, it finds the field with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="field">Field to root import.</param>
        /// <returns>Root imported field.</returns>
        FieldReference RootImport(FieldReference field);

        /// <summary>
        /// Root imports a parameter. That is, it finds the parameter with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="parameter">Parameter to root import.</param>
        /// <returns>Root imported parameter.</returns>
        ParameterDefinition RootImport(ParameterDefinition parameter);

        /// <summary>
        /// Root imports a variable. That is, it finds the variable with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="variable">Variable to root import.</param>
        /// <returns>Root imported variable.</returns>
        VariableDefinition RootImport(VariableDefinition variable);

        /// <summary>
        /// Root imports an instruction. That is, it finds the instruction with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="instruction">Instruction to root import.</param>
        /// <returns>Root imported instruction.</returns>
        Instruction RootImport(Instruction instruction);
    }
}
