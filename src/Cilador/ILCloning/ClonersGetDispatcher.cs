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
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Dispatches a call to get cloners for an item.
    /// </summary>
    internal sealed class ClonersGetDispatcher : ILItemFunctionDispatcherBase<ReadOnlyCollection<ICloner<object>>>
    {
        /// <summary>
        /// Creates a new <see cref="ClonersGetDispatcher"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context for the cloning operation.</param>
        /// <param name="ilGraph">Graph of all items that are being looked at.</param>
        /// <param name="targetsByRoot">Dictionary containing a collection of targets for each root in <paramref name="ilGraph"/>.</param>
        /// <param name="clonersBySource">Dictionary containing a collection of cloners for each possible source.</param>
        public ClonersGetDispatcher(
            IILCloningContext ilCloningContext,
            ILGraph ilGraph,
            IReadOnlyDictionary<object, ReadOnlyCollection<object>> targetsByRoot,
            IReadOnlyDictionary<object, ReadOnlyCollection<ICloner<object>>> clonersBySource)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(ilGraph != null);
            Contract.Requires(targetsByRoot != null);
            Contract.Requires(clonersBySource != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.ILGraph != null);
            Contract.Ensures(this.TargetsByRoot != null);
            Contract.Ensures(this.ClonersBySource != null);

            this.ILCloningContext = ilCloningContext;
            this.ILGraph = ilGraph;
            this.TargetsByRoot = targetsByRoot;
            this.ClonersBySource = clonersBySource;
        }

        /// <summary>
        /// IL cloning context for the cloning operation.
        /// </summary>
        private IILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Graph containing all of the vertices and edges for the IL involved in the cloning operation.
        /// </summary>
        private ILGraph ILGraph { get; set; }

        /// <summary>
        /// Gets or sets the collection of target object for each root in the graph of items.
        /// </summary>
        private IReadOnlyDictionary<object, ReadOnlyCollection<object>> TargetsByRoot { get; set; }

        /// <summary>
        /// Gets or sets the dictionary by which cloners can be looked up by the cloning source.
        /// </summary>
        private IReadOnlyDictionary<object, ReadOnlyCollection<ICloner<object>>> ClonersBySource { get; set; }

        /// <summary>
        /// Handles null items.
        /// </summary>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForNull()
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
        protected override ReadOnlyCollection<ICloner<object>> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get cloners for a non-IL item.");
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(TypeDefinition item)
        {
            List<ICloner<object>> cloners;

            TypeDefinition parent;
            if (this.ILGraph.TryGetParentOf(item, out parent))
            {
                Contract.Assert(parent != null);
                var parentCloners = this.ClonersBySource[parent];
                Contract.Assert(parentCloners != null);

                cloners = new List<ICloner<object>>(parentCloners.Count);
                foreach(ICloner<TypeDefinition> parentCloner in parentCloners)
                {
                    cloners.Add(new NestedTypeCloner(parentCloner, item));
                }
            }
            else
            {
                Contract.Assert(this.ILGraph.Roots.Contains(item));

                var targets = this.TargetsByRoot[item];
                cloners = new List<ICloner<object>>(targets.Count);
                foreach (TypeDefinition target in targets)
                {
                    cloners.Add(new RootTypeCloner(this.ILCloningContext, item, target));
                }
            }

            return new ReadOnlyCollection<ICloner<object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(FieldDefinition item)
        {
            throw new NotImplementedException();
            //var parentCloners = (IEnumerable<ICloner<TypeDefinition>>)this.ClonersBySource[this.ILGraph.GetParentOf<TypeDefinition>(item)];
            //return new ICloner<object>[] { new FieldCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(MethodDefinition item)
        {
            throw new NotImplementedException("Handle root type constructors");
            //var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[item.DeclaringType].Single();
            //return new ICloner<object>[] { new MethodSignatureCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(MethodBody item)
        {
            throw new NotImplementedException();
            //var parentCloner = (MethodSignatureCloner)this.ClonersBySource[this.ILGraph.GetParentOf<MethodDefinition>(item)].Single();
            //return new ICloner<object>[] { new MethodBodyCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(PropertyDefinition item)
        {
            throw new NotImplementedException();
            //var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[this.ILGraph.GetParentOf<TypeDefinition>(item)].Single();
            //return new ICloner<object>[] { new PropertyCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(EventDefinition item)
        {
            throw new NotImplementedException();
            //var parentCloner = (ICloner<TypeDefinition>)this.ClonersBySource[this.ILGraph.GetParentOf<TypeDefinition>(item)].Single();
            //return new ICloner<object>[] { new EventCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(GenericParameter item)
        {
            throw new NotImplementedException();
            //ICloner<IGenericParameterProvider> parentCloner =
            //    (ICloner<IGenericParameterProvider>)this.ClonersBySource[this.ILGraph.GetParentOf<IGenericParameterProvider>(item)].Single();

            //GenericParameter previousSibling;
            //GenericParameterCloner previousSiblingCloner;
            //if (this.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling))
            //{
            //    previousSiblingCloner = null;
            //}
            //else
            //{
            //    previousSiblingCloner = (GenericParameterCloner)this.ClonersBySource[previousSibling].Single();
            //}

            //return new ICloner<object>[] { new GenericParameterCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(ParameterDefinition item)
        {
            throw new NotImplementedException();
            //MethodSignatureCloner parentCloner =
            //    (MethodSignatureCloner)this.ClonersBySource[this.ILGraph.GetParentOf<MethodDefinition>(item)].Single();

            //ParameterDefinition previousSibling;
            //ParameterCloner previousSiblingCloner;
            //if (this.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling))
            //{
            //    previousSiblingCloner = (ParameterCloner)this.ClonersBySource[previousSibling].Single();
            //}
            //else
            //{
            //    previousSiblingCloner = null;
            //}

            //return new ICloner<object>[] { new ParameterCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(MethodReturnType item)
        {
            throw new NotImplementedException();
            //var parentCloner = (MethodSignatureCloner)this.ClonersBySource[this.ILGraph.GetParentOf<MethodDefinition>(item)].Single();
            //return new ICloner<object>[] { new MethodReturnTypeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(CustomAttribute item)
        {
            throw new NotImplementedException();
            //var parentCloner =
            //    (ICloner<ICustomAttributeProvider>)this.ClonersBySource[this.ILGraph.GetParentOf<ICustomAttributeProvider>(item)].Single();
            //return new ICloner<object>[] { new CustomAttributeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(VariableDefinition item)
        {
            throw new NotImplementedException("How to get the parent and previous given that the parent may generate multiple cloners?");
            //var parentCloner = (ICloneToMethodBody<object>)this.ClonersBySource[item].Single();

            //VariableCloner previousSiblingCloner;
            //if (item.Index == 0) { previousSiblingCloner = null; }
            //else
            //{
            //    //previousSiblingCloner =
            //    //    ParameterCloner)this.ClonersBySource[parentCloner.Source.Variables[item.Index - 1]].Single();
            //}
            //return new ICloner<object>[] { new VariableCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(Instruction item)
        {
            throw new NotImplementedException();
            //var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item].Single();
            //return new ICloner<object>[] { new CustomAttributeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object>> InvokeForItem(ExceptionHandler item)
        {
            throw new NotImplementedException();
            //var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item].Single();
            //return new ICloner<object>[] { new ExceptionHandlerCloner(parentCloner, item) };
        }
    }
}
