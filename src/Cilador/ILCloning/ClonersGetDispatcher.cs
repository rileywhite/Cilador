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
using Cilador.Graph;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.ILCloning
{
    internal class ClonersGetDispatcher : ILItemFunctionDispatcherBase<IEnumerable<ICloner<object>>>
    {
        public ClonersGetDispatcher(ILGraph ilGraph, IDictionary<object, IEnumerable<ICloner<object>>> clonersBySource)
        {
            Contract.Requires(ilGraph != null);
            Contract.Requires(clonersBySource != null);
            Contract.Ensures(this.ILGraph != null);
            Contract.Ensures(this.ClonersBySource != null);

            this.ILGraph = ilGraph;
            this.ClonersBySource = clonersBySource;
        }

        /// <summary>
        /// Graph containing all of the vertices and edges for the IL involved in the cloning operation.
        /// </summary>
        private ILGraph ILGraph { get; set; }

        /// <summary>
        /// Gets or sets the dictionary by which cloners can be looked up by the cloning source.
        /// </summary>
        private IDictionary<object, IEnumerable<ICloner<object>>> ClonersBySource { get; set; }

        /// <summary>
        /// Handles null items.
        /// </summary>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<ICloner<object>> InvokeForNull()
        {
            throw new NotSupportedException("Cannot get cloners for a null item.");
        }

        /// <summary>
        /// Handles non-IL items.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IEnumerable<ICloner<object>> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get cloners for a non-IL item.");
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(TypeDefinition item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(FieldDefinition item)
        {
            var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[item.DeclaringType].Single();
            return new ICloner<object>[] { new FieldCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(MethodDefinition item)
        {
            throw new NotImplementedException("Handle root type constructors");
            var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[item.DeclaringType].Single();
            return new ICloner<object>[] { new MethodSignatureCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(MethodBody item)
        {
            var parentCloner = (MethodSignatureCloner)this.ClonersBySource[item.Method].Single();
            return new ICloner<object>[] { new MethodBodyCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(PropertyDefinition item)
        {
            var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[item.DeclaringType].Single();
            return new ICloner<object>[] { new PropertyCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(EventDefinition item)
        {
            var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[item.DeclaringType].Single();
            return new ICloner<object>[] { new EventCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(GenericParameter item)
        {
            ICloner<IGenericParameterProvider> parentCloner;
            if (item.DeclaringMethod != null)
            {
                parentCloner = (ICloner<IGenericParameterProvider>)this.ClonersBySource[item.DeclaringMethod].Single();
            }
            else
            {
                parentCloner = (ICloner<IGenericParameterProvider>)this.ClonersBySource[item.DeclaringType].Single();
            }

            GenericParameterCloner previousSiblingCloner;
            if (item.Position == 0) {previousSiblingCloner = null;}
            else
            {
                previousSiblingCloner =
                    (GenericParameterCloner)this.ClonersBySource[parentCloner.Source.GenericParameters[item.Position - 1]].Single();
            }
            return new ICloner<object>[] { new GenericParameterCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(ParameterDefinition item)
        {
            MethodSignatureCloner parentCloner = (MethodSignatureCloner)this.ClonersBySource[item.Method].Single();

            ParameterCloner previousSiblingCloner;
            if (item.Sequence == 0) { previousSiblingCloner = null; }
            else
            {
                previousSiblingCloner =
                    ParameterCloner)this.ClonersBySource[parentCloner.Source.Parameters[item.Sequence - 1]].Single();
            }
            return new ICloner<object>[] { new ParameterCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(MethodReturnType item)
        {
            var parentCloner = (MethodSignatureCloner)this.ClonersBySource[item.Method].Single();
            return new ICloner<object>[] { new MethodReturnTypeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(CustomAttribute item)
        {
            throw new NotImplementedException("How to get the parent?");
            var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item.].Single();
            return new ICloner<object>[] { new CustomAttributeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(VariableDefinition item)
        {
            throw new NotImplementedException("How to get the parent and previous?");
            var parentCloner = (ICloneToMethodBody<object>)this.ClonersBySource[item].Single();

            VariableCloner previousSiblingCloner;
            if (item.Index == 0) { previousSiblingCloner = null; }
            else
            {
                //previousSiblingCloner =
                //    ParameterCloner)this.ClonersBySource[parentCloner.Source.Variables[item.Index - 1]].Single();
            }
            return new ICloner<object>[] { new VariableCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(Instruction item)
        {
            var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item.].Single();
            return new ICloner<object>[] { new CustomAttributeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IEnumerable<ICloner<object>> InvokeForItem(ExceptionHandler item)
        {
            var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item.].Single();
            return new ICloner<object>[] { new ExceptionHandlerCloner(parentCloner, item) };
        }
    }
}
