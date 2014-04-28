/***************************************************************************/
// Copyright 2013-2014 Riley White
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
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Handles importing referenced types into the mixin target module. Also handles
    /// mixin type redirection, where referenced members of a mixin implementation
    /// are replaced with the target's versions of those members.
    /// </summary>
    /// <remarks>
    /// The way generics for non-mixin implementation types are handled in this file
    /// is certainly in need of change. For now it works, but it will be rewritten when full
    /// mixin generic support is added.
    /// </remarks>
    internal class RootContext
    {
        /// <summary>
        /// Creates a new <see cref="RootContext"/>
        /// </summary>
        /// <param name="rootSource">Top level source type for the cloning operation</param>
        /// <param name="rootTarget">Top level target type for the cloning operation</param>
        public RootContext(TypeDefinition rootSource, TypeDefinition rootTarget)
        {
            Contract.Requires(rootSource != null);
            Contract.Requires(rootTarget != null);
            Contract.Ensures(this.RootSource != null);
            Contract.Ensures(this.RootTarget != null);

            this.RootSource = rootSource;
            this.RootTarget = rootTarget;
            this.TypeCache = new Dictionary<string, TypeReference>();
            this.FieldCache = new Dictionary<string, FieldReference>();
            this.MethodCache = new Dictionary<string, MethodReference>();
        }

        /// <summary>
        /// Gets or sets the top level source type for the cloning operation
        /// </summary>
        public TypeDefinition RootSource { get; private set; }

        /// <summary>
        /// Gets or sets the top level source type for the cloning operation
        /// </summary>
        public TypeDefinition RootTarget { get; private set; }

        /// <summary>
        /// Root import an item when the exact item type may not be known.
        /// </summary>
        /// <typeparam name="TItem">Type of the item to the precision to which it is known. Might just be <see cref="object"/>.</typeparam>
        /// <param name="item">Item to root import.</param>
        /// <returns>Root imported item.</returns>
        public TItem DynamicRootImport<TItem>(TItem item)
        {
            return (TItem)this.RootImport((dynamic)item);
        }

        /// <summary>
        /// Catch-all method for dynamic-dispatched item. If a call is sent to
        /// this method, then no special handling is required, so the object
        /// is simply returned.
        /// </summary>
        /// <param name="item">Item to root import.</param>
        /// <returns>Unmodified <paramref name="item"/></returns>
        private object RootImport(object item)
        {
            return item;
        }

        /// <summary>
        /// Cache of root-imported types so that any given type is only looked up once
        /// </summary>
        private Dictionary<string, TypeReference> TypeCache { get; set; }

        /// <summary>
        /// Root imports a type. That is, it finds the type with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="type">Type to root import.</param>
        /// <returns>Root imported type.</returns>
        public TypeReference RootImport(TypeReference type)
        {
            if (type == null) { return null; }
            if (type.IsGenericParameter) { return type; }

            TypeReference importedType;
            if (!this.TypeCache.TryGetValue(type.FullName, out importedType))
            {
                // if root import has already occurred, then return the previous result

                importedType =
                    type.FullName == this.RootSource.FullName || type.IsNestedWithin(this.RootSource) ?
                    this.RootImportTypeWithinSource(type) :
                    this.RootImportTypeOutsideOfSource(type);

                this.TypeCache[type.FullName] = importedType;
            }

            Contract.Assert(importedType != null);
            Contract.Assert(!(importedType is IMemberDefinition) || importedType.Module == this.RootTarget.Module);
            return importedType;
        }

        /// <summary>
        /// Handles root importing for a type that exists within the <see cref="RootSource"/>.
        /// Mixin redirection for types occurs here.
        /// </summary>
        /// <param name="type">Type to root import.</param>
        /// <returns>Root imported type.</returns>
        private TypeReference RootImportTypeWithinSource(TypeReference type)
        {
            Contract.Requires(type != null);
            Contract.Requires(!type.IsGenericParameter);
            Contract.Requires(type.FullName == this.RootSource.FullName || type.IsNestedWithin(this.RootSource));
            Contract.Ensures(Contract.Result<TypeReference>() != null);

            // if the root source type is being imported, then select the root target type
            if (type.FullName == this.RootSource.FullName) { return this.RootTarget; }

            // because generic types are not supported within mixins, we do not have to worry about generic types or arguments in this method
            // (the source may be generic or nested within a generic, but we will not touch any of that)

            // first doing a root import on the declaring type
            Contract.Assert(type.DeclaringType != null);
            var importedDeclaringType = this.RootImport(type.DeclaringType);
            Contract.Assert(importedDeclaringType != null);

            // find the nested type with the same local name as the type being imported
            var localType = importedDeclaringType.Resolve().NestedTypes.FirstOrDefault(nestedType => nestedType.Name == type.Name);

            if (localType == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Could not find expected type [{0}] inside of imported declaring type [{1}] for root import of [{2}] to root target [{3}]",
                    type.Name,
                    importedDeclaringType.FullName,
                    type.FullName,
                    this.RootTarget.FullName));
            }

            return this.RootTarget.Module.Import(localType);
        }

        /// <summary>
        /// Root import a type that is not nested within the <see cref="RootSource"/>.
        /// This may involve replacing generic type arguments in some cases.
        /// </summary>
        /// <param name="type">Type to root import.</param>
        /// <returns>Root imported type.</returns>
        private TypeReference RootImportTypeOutsideOfSource(TypeReference type)
        {
            Contract.Requires(type != null);
            Contract.Requires(!type.IsGenericParameter);
            Contract.Requires(type.FullName != this.RootSource.FullName && !type.IsNestedWithin(this.RootSource));
            Contract.Ensures(Contract.Result<TypeReference>() != null);

            // because non-mixin types may be closed generic types, we have to make sure that we the type is imported
            // with the correct generic arguments for an arbitrary list of declaring type ancestors
            bool isDeclaringTypeReplaced;
            TypeReference newDeclaringType;
            if (type.IsAnyTypeAncestorAGenericInstanceWithArgumentsIn(this.RootSource))
            {
                isDeclaringTypeReplaced = true;
                newDeclaringType = this.RootImportTypeOutsideOfSource(type.DeclaringType);
            }
            else
            {
                isDeclaringTypeReplaced = false;
                newDeclaringType = null;
            }

            if (type.IsArray)
            {
                Contract.Assert(!isDeclaringTypeReplaced);

                // TODO this seems to work for C#...research whether array importing may need to be more thorough (e.g. dimensions, etc)
                var arrayType = (ArrayType)type;
                return new ArrayType(this.RootImport(arrayType.ElementType), arrayType.Rank);
            }
            else if (type.IsGenericInstance)
            {
                // root import the generic definition and all generic arguments
                // (I believe that this way of resolving generic instances would break if open generic nested types were allowed)
                var genericInstanceType = (GenericInstanceType)type;
                var importedGenericInstanceType = new GenericInstanceType(this.RootTarget.Module.Import(genericInstanceType.ElementType));

                if (isDeclaringTypeReplaced) { importedGenericInstanceType.DeclaringType = newDeclaringType; }

                foreach (var genericArgument in genericInstanceType.GenericArguments)
                {
                    importedGenericInstanceType.GenericArguments.Add(this.RootImport(genericArgument));
                }

                return importedGenericInstanceType;
            }
            else
            {
                if (isDeclaringTypeReplaced)
                {
                    return this.RootTarget.Module.Import(
                        new TypeReference(type.Namespace, type.Name, type.Module, type.Module)
                        {
                            DeclaringType = newDeclaringType,
                        });
                }
                else { return this.RootTarget.Module.Import(type); }
            }
        }

        /// <summary>
        /// Cache of root-imported fields so that any given type is only looked up once
        /// </summary>
        private Dictionary<string, FieldReference> FieldCache { get; set; }

        /// <summary>
        /// Root imports a field. That is, it finds the field with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="field">Field to root import.</param>
        /// <returns>Root imported field.</returns>
        public FieldReference RootImport(FieldReference field)
        {
            if (field == null) { return null; }

            FieldReference importedField;

            // look for cached import
            if (this.FieldCache.TryGetValue(field.FullName, out importedField))
            {
                Contract.Assert(importedField != null);
                return importedField;
            }

            // do a root import of the declaring type
            var importedDeclaringType = this.RootImport(field.DeclaringType);

            // if the declaring type is unchanged, then import the field directly
            if (importedDeclaringType.FullName == field.DeclaringType.FullName)
            {
                importedField = this.RootTarget.Module.Import(field);
            }
            else
            {
                // if there was a change, then find the field with a matching local name
                var localField = importedDeclaringType.Resolve().Fields.FirstOrDefault(possibleField => possibleField.Name == field.Name);

                if (localField == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Could not find expected field [{0}] inside of imported declaring type [{1}] for root import of [{2}] to root target [{3}]",
                        field.Name,
                        importedDeclaringType.FullName,
                        field.FullName,
                        this.RootTarget.FullName));
                }

                importedField = this.RootTarget.Module.Import(localField);
            }

            Contract.Assert(importedField != null);
            Contract.Assert(!(importedField is IMemberDefinition) || importedField.Module == this.RootTarget.Module);
            this.FieldCache[field.FullName] = importedField;
            return importedField;
        }

        /// <summary>
        /// Cache of root-imported fields so that any given type is only looked up once
        /// </summary>
        private Dictionary<string, MethodReference> MethodCache { get; set; }

        /// <summary>
        /// Root imports a method. That is, it finds the method with respect to the <see cref="RootTarget"/> type.
        /// If necessary, this handles mixin redirection, meaning that a member within the <see cref="RootTarget"/>
        /// will be returned in place of a member within the <see cref="RootSource"/>
        /// </summary>
        /// <param name="method">Method to root import.</param>
        /// <returns>Root imported method.</returns>
        public MethodReference RootImport(MethodReference method)
        {
            if (method == null) { return null; }

            MethodReference importedMethod;
            if (!this.MethodCache.TryGetValue(method.FullName, out importedMethod))
            {
                // do a root import of the declaring type
                var importedDeclaringType = this.RootImport(method.DeclaringType);

                importedMethod =
                    method.DeclaringType.IsNestedWithin(this.RootSource) ?
                    this.RootImportMethodWithinSource(method, importedDeclaringType) :
                    this.RootImportMethodOutsideOfSource(method, importedDeclaringType);

                this.MethodCache[method.FullName] = importedMethod;
            }

            Contract.Assert(importedMethod != null);
            Contract.Assert(!(importedMethod is IMemberDefinition) || importedMethod.Module == this.RootTarget.Module);
            return importedMethod;
        }

        /// <summary>
        /// Handles root importing for a method that exists within the <see cref="RootSource"/>.
        /// Mixin redirection for methods occurs here.
        /// </summary>
        /// <param name="type">Type to root import.</param>
        /// <returns>Root imported type.</returns>
        private MethodReference RootImportMethodWithinSource(MethodReference method, TypeReference importedDeclaringType)
        {
            Contract.Requires(method != null);
            Contract.Requires(method.DeclaringType.IsNestedWithin(this.RootSource));
            Contract.Requires(importedDeclaringType != null);
            Contract.Ensures(Contract.Result<MethodReference>() != null);

            Contract.Assert(importedDeclaringType.IsNestedWithin(this.RootTarget));
            Contract.Assert(method.GenericParameters.Count == 0);
            Contract.Assert(!method.IsGenericInstance);
            Contract.Assert(!method.IsAnyTypeAncestorAGenericInstanceWithArgumentsIn(this.RootSource));

            // find the local method with a matching signature
            var localMethod = importedDeclaringType.Resolve().Methods.FirstOrDefault(possibleMethod => possibleMethod.SignatureEquals(method, this));

            if (localMethod == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Could not find expected method matching signature for [{0}] inside of imported declaring type [{1}] for root import to target [{2}]",
                    method.FullName,
                    importedDeclaringType.FullName,
                    this.RootTarget.FullName));
            }

            return this.RootTarget.Module.Import(localMethod);
        }

        /// <summary>
        /// Root import a method that is not within the <see cref="RootSource"/>.
        /// This may involve replacing generic type arguments in some cases.
        /// </summary>
        /// <param name="method">Method to root import.</param>
        /// <returns>Root imported method.</returns>
        private MethodReference RootImportMethodOutsideOfSource(MethodReference method, TypeReference importedDeclaringType)
        {
            Contract.Requires(method != null);
            Contract.Requires(!method.DeclaringType.IsNestedWithin(this.RootSource));
            Contract.Requires(importedDeclaringType != null);
            Contract.Ensures(Contract.Result<MethodReference>() != null);

            Contract.Assert(!importedDeclaringType.IsNestedWithin(this.RootTarget));

            // find the local method with a matching signature
            var resolvedMethod = method.Resolve();
            var localMethod = importedDeclaringType.Resolve().Methods.FirstOrDefault(possibleMethod => possibleMethod.SignatureEquals(resolvedMethod, this));

            // if this is not a generic instance, then just import the method
            if (method.IsGenericInstance)
            {
                Contract.Assert(localMethod.GenericParameters.Count > 0);

                // if this is a generic instance, then create a new generic instance reference and root import all generic arguments
                var genericInstanceMethod = (GenericInstanceMethod)method;

                var importedLocalMethod = this.RootTarget.Module.Import(localMethod);
                if (method.DeclaringType.IsGenericInstance)
                {
                    Contract.Assert(importedDeclaringType.IsGenericInstance);
                    importedLocalMethod.DeclaringType = importedDeclaringType;
                }

                var importedGenericInstanceMethod = new GenericInstanceMethod(importedLocalMethod);

                foreach (var genericArgument in genericInstanceMethod.GenericArguments)
                {
                    importedGenericInstanceMethod.GenericArguments.Add(this.RootImport(genericArgument));
                }

                return importedGenericInstanceMethod;
            }
            else
            {
                var importedMethod = this.RootTarget.Module.Import(localMethod);

                if (method.DeclaringType.IsGenericInstance)
                {
                    Contract.Assert(importedDeclaringType.IsGenericInstance);
                    importedMethod.DeclaringType = importedDeclaringType;
                }

                return importedMethod;
            }
        }
    }
}
