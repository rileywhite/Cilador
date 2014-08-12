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
    internal class ILCloningContext
    {
        /// <summary>
        /// Creates a new <see cref="ILCloningContext"/>
        /// </summary>
        /// <param name="rootSource">Top level source type for the cloning operation</param>
        /// <param name="rootTarget">Top level target type for the cloning operation</param>
        public ILCloningContext(TypeDefinition rootSource, TypeDefinition rootTarget)
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
            this.ClonerGatheringVisitor = new ClonerGatheringVisitor(this);
        }

        /// <summary>
        /// Executes the cloning actions specified by the context.
        /// </summary>
        internal void Execute()
        {
            Contract.Requires(this.RootSource != null);
            Contract.Requires(this.RootTarget != null);

            this.ClonerGatheringVisitor.Visit(this.RootSource, this.RootTarget);
            this.Cloners.SetAllClonersAdded();
            this.Cloners.InvokeCloners();
        }

        /// <summary>
        /// Gets or sets the top level source type for the cloning operation.
        /// </summary>
        public TypeDefinition RootSource { get; private set; }

        /// <summary>
        /// Gets or sets the top level source type for the cloning operation.
        /// </summary>
        public TypeDefinition RootTarget { get; private set; }

        /// <summary>
        /// Gets or sets the cloner gathering visitor used by this context.
        /// </summary>
        private ClonerGatheringVisitor ClonerGatheringVisitor { get; set; }

        private Cloners Cloners
        {
            get { return this.ClonerGatheringVisitor.Cloners; }
        }

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

            // if root import has already occurred, then return the previous result
            TypeReference importedType;
            if (this.TypeCache.TryGetValue(type.FullName, out importedType))
            {
                Contract.Assert(importedType != null);
                return importedType;
            }

            bool isDeclaringTypeReplaced;

            TypeReference newDeclaringType;
            if ((type.FullName == this.RootSource.FullName || type.IsNestedWithin(this.RootSource)) &&
                (type.IsAnyTypeAncestorAGenericInstanceWithArgumentsIn(this.RootSource)))
            {
                // not within the clone targets, so import from outside of clone source/targets

                // because types outside of the clone source/target may be closed generic types, we have to make sure that
                // the type is imported with the correct generic arguments for an arbitrary list of declaring type ancestors

                isDeclaringTypeReplaced = true;
                newDeclaringType = this.RootImport(type.DeclaringType);
            }
            else
            {
                // this will be either a mixed type or a non-mixed type with no generic argument reference to a mixed type
                isDeclaringTypeReplaced = false;
                newDeclaringType = null;
            }

            if (type.IsArray)
            {
                // an array type needs to have its element type root imported
                Contract.Assert(!isDeclaringTypeReplaced);

                var arrayType = (ArrayType)type;
                importedType = new ArrayType(this.RootImport(arrayType.ElementType), arrayType.Rank);
            }
            else if (type.IsGenericInstance)
            {
                // root import the generic definition and all generic arguments
                var genericInstanceType = (GenericInstanceType)type;
                var importedGenericInstanceType = new GenericInstanceType(this.RootTarget.Module.Import(genericInstanceType.ElementType));

                if (isDeclaringTypeReplaced) { importedGenericInstanceType.DeclaringType = newDeclaringType; }

                foreach (var genericArgument in genericInstanceType.GenericArguments)
                {
                    importedGenericInstanceType.GenericArguments.Add(this.RootImport(genericArgument));
                }

                importedType = importedGenericInstanceType;
            }
            else
            {
                if (isDeclaringTypeReplaced)
                {
                    // here we are working with an external type with a generic argument of a mixed type
                    importedType = this.RootTarget.Module.Import(
                        new TypeReference(type.Namespace, type.Name, type.Module, type.Module)
                        {
                            DeclaringType = newDeclaringType,
                        });
                }
                else
                {
                    // either return the found mixed target (for mixed types) or do a straight import of the type (for non-mixed types)
                    TypeDefinition foundTargetType;
                    importedType =
                        this.Cloners.TryGetTargetFor(type, out foundTargetType) ?
                        foundTargetType :
                        this.RootTarget.Module.Import(type);
                }
            }

            Contract.Assert(importedType != null);
            Contract.Assert(!(importedType is IMemberDefinition) || importedType.Module == this.RootTarget.Module);
            this.TypeCache[type.FullName] = importedType;
            return importedType;
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

            FieldDefinition targetField;
            if (this.Cloners.TryGetTargetFor(field, out targetField))
            {
                Contract.Assert(targetField != null);

                // all root importing comes from code that is being generated within the target module
                // so there is no need to do a module import
                importedField = targetField;
            }
            else
            {
                importedField = this.RootTarget.Module.Import(field);
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
                // try to get the method from within the clone targets that would correspond with a method within the clone source
                MethodDefinition importedMethodDefinition;
                if (this.Cloners.TryGetTargetFor(method, out importedMethodDefinition))
                {
                    // all root importing comes from code that is being generated within the target module
                    // so there is no need to do a module import
                    importedMethod = importedMethodDefinition;
                }
                else
                {
                    // not within the clone targets, so import from outside of clone source/targets
                    importedMethod = this.RootImportMethodOutsideOfSource(method);
                }

                this.MethodCache[method.FullName] = importedMethod;
            }

            Contract.Assert(importedMethod != null);
            Contract.Assert(!(importedMethod is IMemberDefinition) || importedMethod.Module == this.RootTarget.Module);
            return importedMethod;
        }

        /// <summary>
        /// Root import a method that is not within the <see cref="RootSource"/>.
        /// This may involve replacing generic type arguments in some cases.
        /// </summary>
        /// <param name="method">Method to root import.</param>
        /// <returns>Root imported method.</returns>
        private MethodReference RootImportMethodOutsideOfSource(MethodReference method)
        {
            Contract.Requires(method != null);
            Contract.Requires(!method.DeclaringType.IsNestedWithin(this.RootSource));
            Contract.Ensures(Contract.Result<MethodReference>() != null);

            // do a root import of the declaring type
            var importedDeclaringType = this.RootImport(method.DeclaringType);

            Contract.Assert(!importedDeclaringType.IsNestedWithin(this.RootTarget));

            // find the local method with a matching signature
            var resolvedMethod = method.Resolve();
            var localMethod = importedDeclaringType.Resolve().Methods.FirstOrDefault(possibleMethod => possibleMethod.SignatureEquals(resolvedMethod, this));

            // check for a generic instance because this will be more work
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
                // if this is not a generic instance, then just import the method
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
