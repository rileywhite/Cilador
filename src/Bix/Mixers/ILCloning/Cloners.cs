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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Bix.Mixers.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Simple type to hold and invoke collections of cloners for IL cloning.
    /// </summary>
    internal class Cloners
    {
        /// <summary>
        /// Creates a new <see cref="Cloners"/>
        /// </summary>
        public Cloners()
        {
            Contract.Ensures(this.InnerCloners != null);
            Contract.Ensures(this.TargetTypeBySourceFullName != null);
            Contract.Ensures(this.TargetFieldBySourceFullName != null);
            Contract.Ensures(this.TargetMethodBySourceFullName != null);
            Contract.Ensures(this.TargetPropertyBySourceFullName != null);
            Contract.Ensures(this.TargetEventBySourceFullName != null);

            this.InnerCloners = new List<ICloner<object, object>>();

            this.TargetTypeBySourceFullName = new Dictionary<string, TypeDefinition>();
            this.TargetGenericParameterBySourceOwnerFullNameAndPosition = new Dictionary<string, GenericParameter>();
            this.TargetFieldBySourceFullName = new Dictionary<string, FieldDefinition>();
            this.TargetMethodBySourceFullName = new Dictionary<string, MethodDefinition>();
            this.TargetPropertyBySourceFullName = new Dictionary<string, PropertyDefinition>();
            this.TargetEventBySourceFullName = new Dictionary<string, EventDefinition>();
        }

        private List<ICloner<object, object>> InnerCloners { get; set; }

        /// <summary>
        /// Gets or sets whether the cloners are currenty being invoked.
        /// </summary>
        public bool AreAllClonersAdded { get; private set; }

        /// <summary>
        /// Marks that all cloners have been added. Targets may be looked
        /// up and cloning may be invoked only after this has been called.
        /// </summary>
        public void SetAllClonersAdded()
        {
            this.AreAllClonersAdded = true;
        }

        /// <summary>
        /// Gets or sets whether the cloners are currently being invoked.
        /// </summary>
        public bool AreClonersInvoking { get; private set; }

        /// <summary>
        /// Gets or sets whether the cloners are have been invoked.
        /// </summary>
        public bool AreClonersInvoked { get; private set; }

        /// <summary>
        /// Gets whether the cloners may be invoked.
        /// </summary>
        public bool CanInvokeCloners
        {
            get { return this.AreAllClonersAdded && !this.AreClonersInvoking && !this.AreClonersInvoked; }
        }

        /// <summary>
        /// Invokes all contained cloners.
        /// </summary>
        public void InvokeCloners()
        {
            Contract.Requires(this.CanInvokeCloners);
            Contract.Ensures(!this.AreClonersInvoking);
            Contract.Ensures(this.AreClonersInvoked);
            Contract.Ensures(!this.CanInvokeCloners);

            this.AreClonersInvoking = true;

            this.InnerCloners.CloneAll();

            this.AreClonersInvoked = true;
            this.AreClonersInvoking = false;
        }

        #region Types

        /// <summary>
        /// Gets or sets the collection of target types indexed by full name of the cloning source.
        /// </summary>
        private Dictionary<string, TypeDefinition> TargetTypeBySourceFullName { get; set; }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(ClonerBase<TypeDefinition> cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
            this.TargetTypeBySourceFullName.Add(GetUniqueKeyFor(cloner.Source), cloner.Target);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(TypeReference source, out TypeDefinition target)
        {
            Contract.Requires(source != null);
            Contract.Requires(!source.IsArray);
            Contract.Requires(!source.IsGenericInstance);
            Contract.Requires(!source.IsGenericParameter);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            return this.TargetTypeBySourceFullName.TryGetValue(GetUniqueKeyFor(source.Resolve()), out target);
        }

        #endregion

        #region Generic Parameters

        /// <summary>
        /// Gets or sets the collection of target generic parameters indexed by full owner name and position of the cloning source.
        /// </summary>
        private Dictionary<string, GenericParameter> TargetGenericParameterBySourceOwnerFullNameAndPosition { get; set; }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(GenericParameterCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(cloner.Source != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
            this.TargetGenericParameterBySourceOwnerFullNameAndPosition.Add(GetUniqueKeyFor(cloner.Source), cloner.Target);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(GenericParameter source, out GenericParameter target)
        {
            Contract.Requires(source != null);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            if (!this.TargetGenericParameterBySourceOwnerFullNameAndPosition.TryGetValue(GetUniqueKeyFor(source), out target))
            {
                target = null;
                return false;
            }

            Contract.Assert(target != null);
            return true;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets or sets the collection of target field indexed by full name of the cloning source.
        /// </summary>
        private Dictionary<string, FieldDefinition> TargetFieldBySourceFullName { get; set; }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(FieldCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
            this.TargetFieldBySourceFullName.Add(GetUniqueKeyFor(cloner.Source), cloner.Target);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(FieldReference source, out FieldDefinition target)
        {
            Contract.Requires(source != null);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            return this.TargetFieldBySourceFullName.TryGetValue(GetUniqueKeyFor(source.Resolve()), out target);
        }

        #endregion

        #region Method Signatures

        /// <summary>
        /// Gets or sets the collection of target method indexed by full name of the cloning source.
        /// </summary>
        private Dictionary<string, MethodDefinition> TargetMethodBySourceFullName { get; set; }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(MethodSignatureCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
            this.TargetMethodBySourceFullName.Add(GetUniqueKeyFor(cloner.Source), cloner.Target);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(MethodReference source, out MethodDefinition target)
        {
            Contract.Requires(source != null);
            Contract.Requires(!source.IsGenericInstance);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            return this.TargetMethodBySourceFullName.TryGetValue(GetUniqueKeyFor(source.Resolve()), out target);
        }

        #endregion

        #region Constructor Logic Signatures

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(ConstructorLogicSignatureCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        #region Method Return Types

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(MethodReturnTypeCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        #region Method Parameters

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(ParameterCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(ParameterDefinition source, out ParameterDefinition target)
        {
            Contract.Requires(source != null);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            // this is not fast, but we'll fix that if performance becomes an issue
            var parameterCloner = this.InnerCloners.SingleOrDefault(cloner => cloner.Source == source) as ParameterCloner;

            if (parameterCloner == null)
            {
                target = null;
                return false;
            }
            else
            {
                target = parameterCloner.Target;
                return true;
            }
        }

        #endregion

        #region Constructor Initializations

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(ConstructorLogicBodyCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        #region Constructor Initializations

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(ConstructorInitializationCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        #region Method Bodies

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(MethodBodyCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        #region Method Body Variables

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(VariableCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloners">Cloners to add to the collection.</param>
        public void AddCloners(IEnumerable<VariableCloner> cloners)
        {
            Contract.Requires(cloners != null);
            Contract.Requires(!cloners.AreAnyNull());
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.AddRange(cloners);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(VariableDefinition source, out VariableDefinition target)
        {
            Contract.Requires(source != null);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            // this is not fast, but we'll fix that if performance becomes an issue
            var variableCloner = this.InnerCloners.SingleOrDefault(cloner => cloner.Source == source) as VariableCloner;

            if (variableCloner == null)
            {
                target = null;
                return false;
            }
            else
            {
                target = variableCloner.Target;
                return true;
            }
        }

        #endregion

        #region Method Body Instructions

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(InstructionCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        /// <summary>
        /// Adds a cloners to the collection.
        /// </summary>
        /// <param name="cloners">Cloners to add to the collection.</param>
        public void AddCloners(IEnumerable<InstructionCloner> cloners)
        {
            Contract.Requires(cloners != null);
            Contract.Requires(!cloners.AreAnyNull());
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.AddRange(cloners);
        }

        /// <summary>
        /// Attempt to retrieve the cloning target for a given source.
        /// </summary>
        /// <param name="source">Source to find target for.</param>
        /// <param name="target">Target for the given source if it exists, else <c>null</c>.</param>
        /// <returns><c>true</c> if a target was found, else <c>false</c>.</returns>
        public bool TryGetTargetFor(Instruction source, out Instruction target)
        {
            Contract.Requires(source != null);
            Contract.Requires(this.AreAllClonersAdded);
            Contract.Ensures(Contract.ValueAtReturn(out target) != null || !Contract.Result<bool>());

            // this is not fast, but we'll fix that if performance becomes an issue
            var instructionCloner = this.InnerCloners.SingleOrDefault(cloner => cloner.Source == source) as InstructionCloner;

            if (instructionCloner == null)
            {
                target = null;
                return false;
            }
            else
            {
                target = instructionCloner.Target;
                return true;
            }
        }

        #endregion

        #region Method Body Exception Handlers

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(ExceptionHandlerCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the collection of target property indexed by full name of the cloning source.
        /// </summary>
        private Dictionary<string, PropertyDefinition> TargetPropertyBySourceFullName { get; set; }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(PropertyCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
            this.TargetPropertyBySourceFullName.Add(GetUniqueKeyFor(cloner.Source), cloner.Target);
        }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets the collection of target event indexed by full name of the cloning source.
        /// </summary>
        private Dictionary<string, EventDefinition> TargetEventBySourceFullName { get; set; }

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(EventCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
            this.TargetEventBySourceFullName.Add(GetUniqueKeyFor(cloner.Source), cloner.Target);
        }

        #endregion

        #region Custom Attributes

        /// <summary>
        /// Adds a cloner to the collection.
        /// </summary>
        /// <param name="cloner">Cloner to add to the collection.</param>
        public void AddCloner(CustomAttributeCloner cloner)
        {
            Contract.Requires(cloner != null);
            Contract.Requires(cloner.Source != null);
            Contract.Requires(!this.AreAllClonersAdded);

            this.InnerCloners.Add(cloner);
        }

        #endregion

        /// <summary>
        /// Gets a unique string key for an item that is suitable for use in dictionaries and hashes.
        /// </summary>
        /// <param name="item">Item that will need indexed.</param>
        /// <returns>Unique index value for the item.</returns>
        private static string GetUniqueKeyFor(IMemberDefinition item)
        {
            return item.FullName;
        }

        /// <summary>
        /// Gets a unique string key for an item that is suitable for use in dictionaries and hashes.
        /// </summary>
        /// <param name="item">Item that will need indexed.</param>
        /// <returns>Unique index value for the item.</returns>
        public static string GetUniqueKeyFor(GenericParameter item)
        {
            switch(item.Owner.GenericParameterType)
            {
                case GenericParameterType.Method:
                    return string.Format(
                        "{0}:::{1}:::{2}",
                        item.DeclaringMethod.DeclaringType.FullName,
                        item.DeclaringMethod.Name,
                        item.Position);

                case GenericParameterType.Type:
                    return string.Format(
                        "{0}:::{1}",
                        item.DeclaringType.FullName,
                        item.Position);

                default:
                    throw new InvalidOperationException(string.Format(
                        "Unknwon Generic parameter type [{0}] for a generic parameter's owner.",
                        item.Owner.GenericParameterType.ToString()));
            }
        }
    }
}
