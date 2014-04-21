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

        private TypeDefinition RootSource { get; set; }

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

            TypeReference importedType;

            // if root import has already occurred, then return the previous result
            if (this.TypeCache.TryGetValue(type.FullName, out importedType))
            {
                Contract.Assert(importedType != null);
                return importedType;
            }

            // if the root source type is being imported, then select the root target type
            if (type.FullName == this.RootSource.FullName) { importedType = this.RootTarget.Module.Import(this.RootTarget); }

            // otherwise if this is not a nested type, then import the type
            else if (type.DeclaringType == null) { importedType = this.RootTarget.Module.Import(type); }

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

            // if the declaring type is unchanged, then import the method directly
            if (importedDeclaringType.FullName == method.DeclaringType.FullName)
            {
                importedMethod = this.RootTarget.Module.Import(method);
            }
            else
            {
                // if there was a change, then find the local method with a matching signature
                var localMethod = importedDeclaringType.Resolve().Methods.FirstOrDefault(possibleMethod => possibleMethod.SignatureEquals(method));

                if (localMethod == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Could not find expected method matching signature for [{0}] inside of imported declaring type [{1}] for root import to target [{2}]",
                        method.FullName,
                        importedDeclaringType.FullName,
                        this.RootTarget.FullName));
                }

                importedMethod = this.RootTarget.Module.Import(localMethod);
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
