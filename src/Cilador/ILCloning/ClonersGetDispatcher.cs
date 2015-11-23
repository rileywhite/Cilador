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
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cilador.ILCloning
{
    /// <summary>
    /// Dispatches a call to get cloners for an item.
    /// </summary>
    internal sealed class ClonersGetDispatcher : ILItemFunctionDispatcherBase<IReadOnlyCollection<ICloner<object, object>>>
    {
        /// <summary>
        /// Creates a new <see cref="ClonersGetDispatcher"/>.
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context for the cloning operation.</param>
        /// <param name="targetsByRoot">Dictionary containing a collection of targets for each root in the IL graph of <paramref name="ilCloningContext"/>.</param>
        /// <param name="clonersBySource">Dictionary containing a collection of cloners for each possible source.</param>
        public ClonersGetDispatcher(
            IILCloningContext ilCloningContext,
            IReadOnlyDictionary<object, IReadOnlyCollection<object>> targetsByRoot,
            IReadOnlyDictionary<object, IReadOnlyCollection<ICloner<object, object>>> clonersBySource)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(targetsByRoot != null);
            Contract.Requires(clonersBySource != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.TargetsByRoot != null);
            Contract.Ensures(this.ClonersBySource != null);

            this.ILCloningContext = ilCloningContext;
            this.TargetsByRoot = targetsByRoot;
            this.ClonersBySource = clonersBySource;
        }

        /// <summary>
        /// IL cloning context for the cloning operation.
        /// </summary>
        private IILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Gets or sets the collection of target object for each root in the graph of items.
        /// </summary>
        private IReadOnlyDictionary<object, IReadOnlyCollection<object>> TargetsByRoot { get; set; }

        /// <summary>
        /// Gets or sets the dictionary by which cloners can be looked up by the cloning source.
        /// </summary>
        private IReadOnlyDictionary<object, IReadOnlyCollection<ICloner<object, object>>> ClonersBySource { get; set; }

        /// <summary>
        /// Handles null items.
        /// </summary>
        /// <returns>Nothing. Throws exception.</returns>
        /// <exception cref="NotSupportedException">
        /// Always thrown.
        /// </exception>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForNull()
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
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForNonILItem(object item)
        {
            throw new NotSupportedException("Cannot get cloners for a non-IL item.");
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(TypeDefinition item)
        {
            return this.ILCloningContext.ILGraph.Roots.Contains(item) ?
                this.InvokeForRootType(item) :
                this.InvokeForNestedType(item);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        private IReadOnlyCollection<ICloner<object, object>> InvokeForRootType(TypeDefinition item)
        {
            Contract.Requires(item != null);
            Contract.Requires(this.ILCloningContext.ILGraph.Roots.Contains(item));
            Contract.Ensures(Contract.Result<IReadOnlyCollection<ICloner<object, object>>>() != null);

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

            return new List<ICloner<object, object>>(
                from TypeDefinition target in targets
                select new RootTypeCloner(this.ILCloningContext, item, target));
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        private IReadOnlyCollection<ICloner<object, object>> InvokeForNestedType(TypeDefinition item)
        {
            Contract.Requires(item != null);
            Contract.Requires(!this.ILCloningContext.ILGraph.Roots.Contains(item));
            Contract.Ensures(Contract.Result<IReadOnlyCollection<ICloner<object, object>>>() != null);

            var parent = this.ILCloningContext.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            return
                (from ICloner<TypeDefinition> parentCloner in parentCloners
                 select new NestedTypeCloner(parentCloner, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(FieldDefinition item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            return
                (from ICloner<TypeDefinition> parentCloner in parentCloners
                 select new FieldCloner(parentCloner, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(PropertyDefinition item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            cloners.AddRange(from ICloner<TypeDefinition> parentCloner in parentCloners
                             select new PropertyCloner(parentCloner, item));

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(EventDefinition item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>(parentCloners.Count);
            cloners.AddRange(from ICloner<TypeDefinition> parentCloner in parentCloners
                             select new EventCloner(parentCloner, item));

            return new ReadOnlyCollection<ICloner<object, object>>(cloners);
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(MethodDefinition item)
        {
            // check whether this is the constructor for a root type
            if (item.IsConstructor && !item.IsStatic && !item.HasParameters && this.ILCloningContext.ILGraph.GetDepth(item) == 1)
            {
                return this.InvokeForRootTypeConstructor(item);
            }

            var parent = this.ILCloningContext.ILGraph.GetParentOf<TypeDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            return
                (from ICloner<TypeDefinition> parentCloner in parentCloners
                 select new MethodSignatureCloner(parentCloner, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        private IReadOnlyCollection<ICloner<object, object>> InvokeForRootTypeConstructor(MethodDefinition item)
        {
            Contract.Requires(item != null);
            Contract.Requires(item.IsConstructor && !item.IsStatic && !item.HasParameters);
            Contract.Ensures(Contract.Result<IReadOnlyCollection<ICloner<object, object>>>() != null);

            var parent = this.ILCloningContext.ILGraph.GetParentOf<TypeDefinition>(item);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var cloners = new List<ICloner<object, object>>();

            foreach (var parentCloner in parentCloners.Cast<RootTypeCloner>())
            {
                // add one no-op cloner per target constructor
                cloners.AddRange(
                    from constructor in parentCloner.Target.Methods
                    where constructor.IsConstructor && !constructor.IsStatic
                    select NoOpCloner.Create(this.ILCloningContext, item, constructor));

                // add a single constructor logic signature cloner to generate a new method, if necessary
                var sourceMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, item);

                // only continue if we have construction items
                if (!sourceMultiplexedConstructor.HasConstructionItems) { continue; }

                // add the constructor logic signature cloner
                var constructorLogicSignatureCloner =
                    new ConstructorLogicSignatureCloner(parentCloner, sourceMultiplexedConstructor);
                cloners.Add(constructorLogicSignatureCloner);
            }

            return cloners;
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(MethodBody item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<MethodDefinition>(item);
            Contract.Assert(parent != null);

            if (parent.IsConstructor && !parent.IsStatic && this.ILCloningContext.ILGraph.GetDepth(item) == 2)
            {
                return this.InvokeForRootTypeConstructorBody(item, parent);
            }

            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            return
                (from MethodSignatureCloner parentCloner in parentCloners
                 select new MethodBodyCloner(parentCloner, item)).ToArray();
        }

        private IReadOnlyCollection<ICloner<object, object>> InvokeForRootTypeConstructorBody(MethodBody item, MethodDefinition parent)
        {
            Contract.Requires(item != null);
            Contract.Requires(parent != null);
            Contract.Ensures(Contract.Result<IReadOnlyCollection<ICloner<object, object>>>() != null);

            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            var constructorLogicSignatureCloner =
                parentCloners.SingleOrDefault(parentCloner => parentCloner is ConstructorLogicSignatureCloner) as ConstructorLogicSignatureCloner;
            var sourceMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, parent);

            var cloners = new List<ICloner<object, object>>();

            if (constructorLogicSignatureCloner != null)
            {
                cloners.Add(new ConstructorLogicBodyCloner(constructorLogicSignatureCloner, sourceMultiplexedConstructor));
            }

            foreach (var parentCloner in parentCloners
                .Where(parentCloner => parentCloner is NoOpCloner<MethodDefinition, MethodDefinition>)
                .Cast<NoOpCloner<MethodDefinition, MethodDefinition>>())
            {
                // the initialization cloning includes calling redirected construction methods
                // so we want to do this if we have initialization items or if we have to create a logic cloner
                if (!sourceMultiplexedConstructor.HasInitializationItems && constructorLogicSignatureCloner == null) { continue; }

                var targetMultiplexedConstructor = MultiplexedConstructor.Get(this.ILCloningContext, parentCloner.Target);

                if (!targetMultiplexedConstructor.IsInitializingConstructor)
                {
                    // skip non-initializing constructors because they will eventually call into an initializing constructor
                    continue;
                }

                Contract.Assert(parentCloner.Target.HasBody);
                var constructorCloner = new ConstructorInitializationCloner(
                    parentCloner,
                    constructorLogicSignatureCloner,
                    sourceMultiplexedConstructor,
                    parentCloner.Target.Body);

                cloners.Add(constructorCloner);
            }

            return cloners;
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(ParameterDefinition item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<MethodDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            if (parentCloners.Count == 0) { return new ICloner<object, object>[0]; }

            IEnumerable<Tuple<ICloner<object, object>, ICloner<object, object>>> parentAndSiblingCloners;

            ParameterDefinition previousSibling;
            if (this.ILCloningContext.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling))
            {
                parentAndSiblingCloners = parentCloners.Zip(this.ClonersBySource[previousSibling], Tuple.Create);
            }
            else
            {
                parentAndSiblingCloners =
                    from parentCloner in parentCloners
                    select Tuple.Create<ICloner<object, object>, ICloner<object, object>>(parentCloner, null);
            }

            return
                (from parentAndSiblingCloner in parentAndSiblingCloners
                 select new ParameterCloner((MethodSignatureCloner)parentAndSiblingCloner.Item1, (ParameterCloner)parentAndSiblingCloner.Item2, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(MethodReturnType item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<MethodDefinition>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            return
                (from ICloner<object, MethodDefinition> parentCloner in parentCloners
                 select new MethodReturnTypeCloner(parentCloner, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(VariableDefinition item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<MethodBody>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            if (parentCloners.Count == 0) { return new ICloner<object, object>[0]; }

            IEnumerable<Tuple<ICloner<object, object>, ICloner<object, object>>> parentAndSiblingCloners;

            VariableDefinition previousSibling;
            if (this.ILCloningContext.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling))
            {
                parentAndSiblingCloners = parentCloners.Zip(this.ClonersBySource[previousSibling], Tuple.Create);
            }
            else
            {
                parentAndSiblingCloners =
                    from parentCloner in parentCloners
                    select Tuple.Create<ICloner<object, object>, ICloner<object, object>>(parentCloner, null);
            }

            return
                (from parentAndSiblingCloner in parentAndSiblingCloners
                 select new VariableCloner((ICloneToMethodBody<object>)parentAndSiblingCloner.Item1, (VariableCloner)parentAndSiblingCloner.Item2, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(Instruction item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<MethodBody>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            Instruction previousSibling;
            if (!this.ILCloningContext.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling)) { previousSibling = null; }

            var cloners = new List<ICloner<object, object>>();

            foreach (var parentCloner in parentCloners.Cast<ICloneToMethodBody<object>>().Where(parentCloner => parentCloner.IsValidSourceInstruction(item)))
            {
                if (previousSibling != null && parentCloner.IsValidSourceInstruction(previousSibling))
                {
                    var previousCloner = this.ClonersBySource[previousSibling].Cast<InstructionCloner>().Single(possiblePrevious => possiblePrevious.Parent == parentCloner);
                    cloners.Add(new InstructionCloner(
                        parentCloner,
                        previousCloner,
                        item,
                        parentCloner.PossiblyReferencedVariables,
                        parentCloner.GetVariableTranslation(item),
                        parentCloner.InstructionInsertAction));
                }
                else
                {
                    cloners.Add(new InstructionCloner(
                        parentCloner,
                        null,
                        item,
                        parentCloner.PossiblyReferencedVariables,
                        parentCloner.GetVariableTranslation(item),
                        parentCloner.InstructionInsertAction));
                }
            }

            return cloners;
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(ExceptionHandler item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<MethodBody>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            if (parentCloners.Count == 0) { return new ICloner<object, object>[0]; }

            IEnumerable<Tuple<ICloner<object, object>, ICloner<object, object>>> parentAndSiblingCloners;

            ExceptionHandler previousSibling;
            if (this.ILCloningContext.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling))
            {
                parentAndSiblingCloners = parentCloners.Zip(this.ClonersBySource[previousSibling], Tuple.Create);
            }
            else
            {
                parentAndSiblingCloners =
                    from parentCloner in parentCloners
                    select Tuple.Create<ICloner<object, object>, ICloner<object, object>>(parentCloner, null);
            }

            return
                (from parentAndSiblingCloner in parentAndSiblingCloners
                 select new ExceptionHandlerCloner((ICloneToMethodBody<object>)parentAndSiblingCloner.Item1, (ExceptionHandlerCloner)parentAndSiblingCloner.Item2, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(GenericParameter item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<IGenericParameterProvider>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            if (parentCloners.Count == 0) { return new ICloner<object, object>[0]; }

            IEnumerable<Tuple<ICloner<object, object>, ICloner<object, object>>> parentAndSiblingCloners;

            GenericParameter previousSibling;
            if (this.ILCloningContext.ILGraph.TryGetPreviousSiblingOf(item, out previousSibling))
            {
                parentAndSiblingCloners = parentCloners.Zip(this.ClonersBySource[previousSibling], Tuple.Create);
            }
            else
            {
                parentAndSiblingCloners =
                    from parentCloner in parentCloners
                    select Tuple.Create<ICloner<object, object>, ICloner<object, object>>(parentCloner, null);
            }

            return
                (from parentAndSiblingCloner in parentAndSiblingCloners
                 select new GenericParameterCloner((ICloner<IGenericParameterProvider>)parentAndSiblingCloner.Item1, (GenericParameterCloner)parentAndSiblingCloner.Item2, item)).ToArray();
        }

        /// <summary>
        /// Gets cloners for the given source item.
        /// </summary>
        /// <param name="item">Item to get cloners for.</param>
        /// <returns>Cloners for the <paramref name="item"/>.</returns>
        protected override IReadOnlyCollection<ICloner<object, object>> InvokeForItem(CustomAttribute item)
        {
            var parent = this.ILCloningContext.ILGraph.GetParentOf<ICustomAttributeProvider>(item);
            Contract.Assert(parent != null);
            var parentCloners = this.ClonersBySource[parent];
            Contract.Assume(parentCloners != null);

            return
                (from ICloner<ICustomAttributeProvider> parentCloner in parentCloners
                 select new CustomAttributeCloner(parentCloner, item)).ToArray();
        }
    }
}
