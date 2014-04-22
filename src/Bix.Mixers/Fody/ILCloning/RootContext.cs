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
    internal class RootContext : IRootImportProvider
    {
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

        public TypeDefinition RootSource { get; private set; }

        public TypeDefinition RootTarget { get; private set; }

        public TItem DynamicRootImport<TItem>(TItem item)
        {
            return (TItem)this.RootImport((dynamic)item);
        }

        private object RootImport(object item)
        {
            return item;
        }

        private Dictionary<string, TypeReference> TypeCache { get; set; }

        public TypeReference RootImport(TypeReference type)
        {
            if (type == null) { return null; }
            if (type.IsGenericParameter) { return type; }

            TypeReference importedType;

            // if root import has already occurred, then return the previous result
            if (this.TypeCache.TryGetValue(type.FullName, out importedType))
            {
                Contract.Assert(importedType != null);
                return importedType;
            }

            // if the root source type is being imported, then select the root target type
            if (type.FullName == this.RootSource.FullName) { importedType = this.RootTarget.Module.Import(this.RootTarget); }

            // check if this is not nested within the mixin type
            else if (!type.IsNestedWithin(this.RootSource))
            {
                // if this is not an array or a generic instance, then just import the type
                if (type.IsArray)
                {
                    // TODO this seems to work for C#...research whether array importing may need to be more thorough (e.g. dimensions, etc)
                    var arrayType = (ArrayType)type;
                    importedType = new ArrayType(this.RootImport(arrayType.ElementType), arrayType.Rank);
                }
                else if (!type.IsGenericInstance) { importedType = this.RootTarget.Module.Import(type); }
                else
                {
                    // if this is a generic instance, then root import the generic definition and all generic arguments
                    // (this way of resolving generic instances would break if open generic nested types were allowed)
                    var genericInstanceType = (GenericInstanceType)type;
                    var importedGenericInstanceType = new GenericInstanceType(this.RootTarget.Module.Import(genericInstanceType.ElementType));

                    foreach(var genericArgument in genericInstanceType.GenericArguments)
                    {
                        importedGenericInstanceType.GenericArguments.Add(this.RootImport(genericArgument));
                    }

                    importedType = importedGenericInstanceType;
                }
            }

            // handle nested types
            else
            {
                // first doing a root import on the declaring type
                var importedDeclaringType = this.RootImport(type.DeclaringType);
                Contract.Assert(importedDeclaringType != null);

                // if the imported declaring type is unchanged, then import the type
                if (type.DeclaringType.FullName == importedDeclaringType.FullName)
                {
                    importedType = this.RootTarget.Module.Import(type);
                }
                else
                {
                    // if the declaring type was imported, then find the nested type with the same local name as the type being imported
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

                    importedType = this.RootTarget.Module.Import(localType);
                }
            }

            Contract.Assert(importedType != null);
            Contract.Assert(!(importedType is IMemberDefinition) || importedType.Module == this.RootTarget.Module);
            this.TypeCache[type.FullName] = importedType;
            return importedType;
        }

        private Dictionary<string, FieldReference> FieldCache { get; set; }

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

        public PropertyReference RootImport(PropertyReference property)
        {
            if (property == null) { return null; }
            // TODO root importing of property
            throw new NotImplementedException("Implement root importing of property when needed");
        }

        private Dictionary<string, MethodReference> MethodCache { get; set; }

        public MethodReference RootImport(MethodReference method)
        {
            if (method == null) { return null; }

            MethodReference importedMethod;

            // look for cached import
            if(this.MethodCache.TryGetValue(method.FullName, out importedMethod))
            {
                Contract.Assert(importedMethod != null);
                return importedMethod;
            }
            
            // do a root import of the declaring type
            var importedDeclaringType = this.RootImport(method.DeclaringType);

            // if the declaring type is unchanged, then this is a non-mixed method
            // it might be generic method with a mixin redirected type argument, however,
            // or it might be attached to a closed generic type with a mixin redirected argument
            if (importedDeclaringType.FullName == method.DeclaringType.FullName)
            {
                // if this is not a generic instance, then just import the method
                if (!method.IsGenericInstance) { importedMethod = this.RootTarget.Module.Import(method); }
                else
                {
                    // if this is a generic instance, then root import the generic definition and all generic arguments
                    var genericInstanceMethod = (GenericInstanceMethod)method;
                    var importedGenericInstanceMethod = new GenericInstanceMethod(this.RootTarget.Module.Import(genericInstanceMethod.ElementMethod));

                    foreach (var genericArgument in genericInstanceMethod.GenericArguments)
                    {
                        importedGenericInstanceMethod.GenericArguments.Add(this.RootImport(genericArgument));
                    }

                    importedMethod = importedGenericInstanceMethod;
                }
            }
            else
            {
                // if there was a change, then find the local method with a matching signature
                var resolvedMethod = method.Resolve();  // this clears any generic data in case the change was due to a generic type closed with mixin redirected types
                var localMethod = importedDeclaringType.Resolve().Methods.FirstOrDefault(possibleMethod => resolvedMethod.SignatureEquals(possibleMethod));

                if (localMethod == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Could not find expected method matching signature for [{0}] inside of imported declaring type [{1}] for root import to target [{2}]",
                        method.FullName,
                        importedDeclaringType.FullName,
                        this.RootTarget.FullName));
                }

                if (!method.DeclaringType.IsGenericInstance) { importedMethod = this.RootTarget.Module.Import(localMethod); }
                else
                {
                    var importedGenericTypeMethod = new MethodReference(
                        localMethod.Name,
                        this.RootImport(localMethod.ReturnType),
                        importedDeclaringType)
                    {
                        CallingConvention = localMethod.CallingConvention,
                        ExplicitThis = localMethod.ExplicitThis,
                        HasThis = localMethod.HasThis
                    };

                    foreach (var parameter in localMethod.Parameters)
                    {
                        importedGenericTypeMethod.Parameters.Add(new ParameterDefinition(this.RootImport(parameter.ParameterType)));
                    }

                    importedMethod = this.RootTarget.Module.Import(importedGenericTypeMethod);

                    if (method.IsGenericInstance)
                    {
                        var importedGenericMethod = new GenericInstanceMethod(importedMethod);
                        foreach(var genericArgument in ((GenericInstanceMethod)method).GenericArguments)
                        {
                            importedGenericMethod.GenericArguments.Add(this.RootImport(genericArgument));
                        }
                        importedMethod = importedGenericMethod;
                    }
                }
            }

            Contract.Assert(importedMethod != null);
            Contract.Assert(!(importedMethod is IMemberDefinition) || importedMethod.Module == this.RootTarget.Module);
            this.MethodCache[method.FullName] = importedMethod;
            return importedMethod;
        }

        public EventReference RootImport(EventReference @event)
        {
            if (@event == null) { return null; }
            // TODO root import of event
            throw new NotImplementedException("Implement root import of event when needed");
        }
    }
}
