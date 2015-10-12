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
    internal sealed class ClonersGetDispatcher : ILItemFunctionDispatcherBase<ReadOnlyCollection<ICloner<object, object>>>
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
            IReadOnlyDictionary<object, ReadOnlyCollection<ICloner<object, object>>> clonersBySource)
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
        private IReadOnlyDictionary<object, ReadOnlyCollection<ICloner<object, object>>> ClonersBySource { get; set; }

        /// <summary>
        /// Handles null items.
        /// </summary>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForNull()
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
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get cloners for a non-IL item.");
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(TypeDefinition item)
        {
            return this.ILGraph.Roots.Contains(item) ?
                this.InvokeForRootType(item) :
                this.InvokeForNestedType(item);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        private ReadOnlyCollection<ICloner<object, object>> InvokeForRootType(TypeDefinition item)
        {
            Contract.Requires(item != null);
            Contract.Requires(this.ILGraph.Roots.Contains(item));
            Contract.Ensures(Contract.Result<ReadOnlyCollection<ICloner<object, object>>>() != null);

            if (item.Methods.Any(
                method => method.IsConstructor && !method.IsStatic && method.HasParameters))
            {
                // at some point in the future if it becomes clear that it would be useful,
                // it may be possible to create all combinations of source and target constructor
                // arguments and put them into the final mixed target
                // but that's a complex and time-consuming task with unknown payoff
                // so for now we don't support mixin implementations that have constructors with parameters
                throw new InvalidOperationException(
                    string.Format(
                        "Cloning root source type cannot have constructors with parameters: [{0}]",
                        item.FullName));
            }

            var targets = this.TargetsByRoot[item];
            var cloners = new List<ICloner<object, object>>(targets.Count);
            foreach (TypeDefinition target in targets)
            {
                cloners.Add(new RootTypeCloner(this.ILCloningContext, item, target));
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        private ReadOnlyCollection<ICloner<object, object>> InvokeForNestedType(TypeDefinition item)
        {
            Contract.Requires(item != null);
            Contract.Requires(!this.ILGraph.Roots.Contains(item));
            Contract.Ensures(Contract.Result<ReadOnlyCollection<ICloner<object, object>>>() != null);

            var parent = this.ILGraph.GetParentOf<TypeDefinition>(item);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            foreach (ICloner<TypeDefinition> parentCloner in parentCloners)
            {
                cloners.Add(new NestedTypeCloner(parentCloner, item));
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(FieldDefinition item)
        {
            var parent = this.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            foreach(ICloner<TypeDefinition> parentCloner in parentCloners)
            {
                cloners.Add(new FieldCloner(parentCloner, item));
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(PropertyDefinition item)
        {
            var parent = this.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            foreach (ICloner<TypeDefinition> parentCloner in parentCloners)
            {
                cloners.Add(new PropertyCloner(parentCloner, item));
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(EventDefinition item)
        {
            var parent = this.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            foreach (ICloner<TypeDefinition> parentCloner in parentCloners)
            {
                cloners.Add(new EventCloner(parentCloner, item));
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(MethodDefinition item)
        {
            // check whether this is the constructor for a root type
            if (item.IsConstructor && !item.IsStatic && !item.HasParameters && this.ILGraph.GetDepth(item) == 1)
            {
                return this.InvokeForRootTypeConstructor(item);
            }

            var parent = this.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            foreach (ICloner<TypeDefinition> parentCloner in parentCloners)
            {
                cloners.Add(new MethodSignatureCloner(parentCloner, item));
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        private ReadOnlyCollection<ICloner<object, object>> InvokeForRootTypeConstructor(MethodDefinition item)
        {
            Contract.Requires(item != null);
            Contract.Requires(item.IsConstructor && !item.IsStatic && !item.HasParameters && this.ILGraph.GetDepth(item) == 1);

            var parent = this.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>();

            foreach (RootTypeCloner parentCloner in parentCloners)
            {
                var sourceMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, item);

                ConstructorLogicSignatureCloner constructorLogicSignatureCloner = null;
                if (sourceMultiplexedConstructor.HasConstructionItems)
                {
                    constructorLogicSignatureCloner =
                        new ConstructorLogicSignatureCloner(parentCloner, sourceMultiplexedConstructor);
                    cloners.Add(constructorLogicSignatureCloner);
                }

                // the initialization cloning includes calling redirected construction methods
                // so we want to do this if we have either initialization or if we have to create a logic cloner
                if (sourceMultiplexedConstructor.HasInitializationItems || constructorLogicSignatureCloner != null)
                {
                    foreach (var targetConstructor in
                        parentCloner.Target.Methods.Where(targetMethod => targetMethod.IsConstructor && !targetMethod.IsStatic))
                    {
                        var targetMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, targetConstructor);

                        if (!targetMultiplexedConstructor.IsInitializingConstructor)
                        {
                            // skip non-initializing constructors because they will eventually call into an initializing constructor
                            continue;
                        }

                        Contract.Assert(targetConstructor.HasBody);
                        var constructorCloner = new ConstructorInitializationCloner(
                            parentCloner,
                            constructorLogicSignatureCloner,
                            sourceMultiplexedConstructor,
                            targetConstructor.Body);

                        cloners.Add(constructorCloner);
                    }
                }
            }

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(MethodBody item)
        {
            throw new NotImplementedException();
            var parent = this.ILGraph.GetParentOf<MethodDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            foreach(ICloneToMethodBody<object> cloner in parentCloners)
            {

            }
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(ParameterDefinition item)
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

            //return new ICloner<object, object>[] { new ParameterCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(MethodReturnType item)
        {
            throw new NotImplementedException();
            //var parentCloner = (MethodSignatureCloner)this.ClonersBySource[this.ILGraph.GetParentOf<MethodDefinition>(item)].Single();
            //return new ICloner<object, object>[] { new MethodReturnTypeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(VariableDefinition item)
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
            //return new ICloner<object, object>[] { new VariableCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(Instruction item)
        {
            throw new NotImplementedException();
            //var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item].Single();
            //return new ICloner<object, object>[] { new CustomAttributeCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(ExceptionHandler item)
        {
            throw new NotImplementedException();
            //var parentCloner = (ICloner<ICustomAttributeProvider>)this.ClonersBySource[item].Single();
            //return new ICloner<object, object>[] { new ExceptionHandlerCloner(parentCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(GenericParameter item)
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

            //return new ICloner<object, object>[] { new GenericParameterCloner(parentCloner, previousSiblingCloner, item) };
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override ReadOnlyCollection<ICloner<object, object>> InvokeForItem(CustomAttribute item)
        {
            throw new NotImplementedException();
            //var parentCloner =
            //    (ICloner<ICustomAttributeProvider>)this.ClonersBySource[this.ILGraph.GetParentOf<ICustomAttributeProvider>(item)].Single();
            //return new ICloner<object, object>[] { new CustomAttributeCloner(parentCloner, item) };
        }
    }
}
