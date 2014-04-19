using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    internal static class ILCloningExtensions
    {
        public static void CloneStructure(this IEnumerable<IMemberCloner> cloners)
        {
            Contract.Requires(cloners != null);
            Contract.Requires(!cloners.Any(cloner => cloner.IsStructureCloned));
            Contract.Ensures(cloners.All(cloner => cloner.IsStructureCloned));

            foreach (var cloner in cloners) { cloner.CloneStructure(); }
        }

        public static bool SignatureEquals(this MethodReference left, MethodReference right)
        {
            if (left == null || right == null) { return left == null && right == null; }

            return left.FullName.Replace(left.DeclaringType.FullName + "::", string.Empty) == right.FullName.Replace(right.DeclaringType.FullName + "::", string.Empty);
        }

        public static bool SignatureEquals(this PropertyDefinition left, PropertyDefinition right)
        {
            if (left == null || right == null) { return left == null && right == null; }

            return left.FullName.Replace(left.DeclaringType.FullName + "::", string.Empty) == right.FullName.Replace(right.DeclaringType.FullName + "::", string.Empty);
        }

        public static bool IsSkipped(this IMemberDefinition member)
        {
            Contract.Requires(member != null);
            var method = member as MethodDefinition;
            if (method == null)
            {
                // a name comparison is not ideal, but until I see it break, I'll stick with it
                var skipAttributeFullName = typeof(SkipAttribute).FullName;
                foreach (var customAttribute in member.CustomAttributes)
                {
                    if (customAttribute.AttributeType.FullName == skipAttributeFullName)
                    {
                        return true;
                    }
                }
                return false;
            }
            else { return method.IsSkipped(); }
        }

        public static bool IsSkipped(this MethodDefinition method)
        {
            Contract.Requires(method != null);

            // a name comparison is not ideal, but until I see it break, I'll stick with it
            var skipAttributeFullName = typeof(SkipAttribute).FullName;
            foreach (var customAttribute in method.CustomAttributes)
            {
                if (customAttribute.AttributeType.FullName == skipAttributeFullName)
                {
                    return true;
                }
            }

            return method.DeclaringType.Properties.Any(
                property =>
                    (property.GetMethod == method || property.SetMethod == method) &&
                    property.IsSkipped());
        }

        public static void CloneAllParameters(
            this MethodReference target,
            MethodReference source,
            IRootImportProvider rootImporter)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.Parameters != null);
            Contract.Requires(source != null);
            Contract.Requires(source.Parameters != null);
            Contract.Requires(target != source);
            Contract.Requires(target.Parameters.Count == 0);
            Contract.Requires(rootImporter != null);
            Contract.Ensures(target.Parameters.Count == source.Parameters.Count);

            if (source.HasParameters)
            {
                foreach (var sourceParameter in source.Parameters)
                {
                    TypeReference targetParameterType;
                    if (!sourceParameter.ParameterType.IsGenericParameter) { targetParameterType = rootImporter.RootImport(sourceParameter.ParameterType); }
                    else
                    {
                        targetParameterType = ((GenericParameter)sourceParameter.ParameterType).ImportForDeclaringMethod(target);
                    }

                    var targetParameter =
                        new ParameterDefinition(sourceParameter.Name, sourceParameter.Attributes, targetParameterType)
                        {
                            Constant = sourceParameter.Constant,
                            HasConstant = sourceParameter.HasConstant,
                            HasDefault = sourceParameter.HasDefault,
                            HasFieldMarshal = sourceParameter.HasFieldMarshal,
                            IsIn = sourceParameter.IsIn,
                            IsLcid = sourceParameter.IsLcid,
                            IsOptional = sourceParameter.IsOptional,
                            IsOut = sourceParameter.IsOut,
                            IsReturnValue = sourceParameter.IsReturnValue,
                        };

                    // TODO research correct usage
                    if (sourceParameter.MarshalInfo != null)
                    {
                        targetParameter.MarshalInfo = new MarshalInfo(sourceParameter.MarshalInfo.NativeType);
                    }

                    // TODO research correct usage
                    targetParameter.MetadataToken = new MetadataToken(sourceParameter.MetadataToken.TokenType, sourceParameter.MetadataToken.RID);

                    // I did not check whether I get a similar issue here as with the duplication in the FieldCloner...adding a clear line just to make sure, though
                    targetParameter.CustomAttributes.Clear();
                    targetParameter.CloneAllCustomAttributes(sourceParameter, rootImporter);

                    target.Parameters.Add(targetParameter);
                }
            }
        }

        public static void CloneAllGenericParameters(
            this IGenericParameterProvider target,
            IGenericParameterProvider source,
            IRootImportProvider rootImporter)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.GenericParameters != null);
            Contract.Requires(target.GenericParameters.Count == 0 || target == rootImporter.RootTarget);
            Contract.Requires(source != null);
            Contract.Requires(source.GenericParameters != null);
            Contract.Requires(rootImporter != null);
            Contract.Ensures(
                target.GenericParameters.Count == source.GenericParameters.Count ||
                (target == rootImporter.RootTarget && target.GenericParameters.Count > source.GenericParameters.Count));

            if (source.HasGenericParameters)
            {
                foreach (var sourceGenericParameter in source.GenericParameters)
                {
                    var targetGenericParameter = new GenericParameter(sourceGenericParameter.Name, target)
                    {
                        Attributes = sourceGenericParameter.Attributes,
                        HasDefaultConstructorConstraint = sourceGenericParameter.HasDefaultConstructorConstraint,
                        HasNotNullableValueTypeConstraint = sourceGenericParameter.HasNotNullableValueTypeConstraint,
                        HasReferenceTypeConstraint = sourceGenericParameter.HasReferenceTypeConstraint,
                        IsContravariant = sourceGenericParameter.IsContravariant,
                        IsCovariant = sourceGenericParameter.IsCovariant,
                        IsNonVariant = sourceGenericParameter.IsNonVariant,
                        IsValueType = sourceGenericParameter.IsValueType,
                        MetadataToken = new MetadataToken(sourceGenericParameter.MetadataToken.TokenType, sourceGenericParameter.MetadataToken.RID),
                    };

                    if(sourceGenericParameter.DeclaringType != null)
                    {
                        targetGenericParameter.DeclaringType = rootImporter.RootImport(sourceGenericParameter.DeclaringType);
                    }

                    foreach (var sourceConstraint in sourceGenericParameter.Constraints)
                    {
                        targetGenericParameter.Constraints.Add(rootImporter.RootImport(sourceConstraint));
                    }

                    if (sourceGenericParameter.HasCustomAttributes)
                    {
                        // I did not check whether I get a similar issue here as with the duplication in the FieldCloner...adding a clear line just to make sure, though
                        targetGenericParameter.CustomAttributes.Clear();
                        targetGenericParameter.CloneAllCustomAttributes(sourceGenericParameter, rootImporter);
                    }

                    targetGenericParameter.CloneAllGenericParameters(sourceGenericParameter, rootImporter);

                    target.GenericParameters.Add(targetGenericParameter);
                }
            }
        }

        public static void CloneAllCustomAttributes(
            this ICustomAttributeProvider target,
            ICustomAttributeProvider source,
            IRootImportProvider rootImporter)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.CustomAttributes != null);
            Contract.Requires(target.CustomAttributes.Count == 0 || target == rootImporter.RootTarget);
            Contract.Requires(source != null);
            Contract.Requires(source.CustomAttributes != null);
            Contract.Requires(rootImporter != null);
            Contract.Ensures(
                target.CustomAttributes.Count == source.CustomAttributes.Count ||
                (target == rootImporter.RootTarget && target.CustomAttributes.Count > source.CustomAttributes.Count));

            if (source.HasCustomAttributes)
            {
                foreach (var sourceAttribute in source.CustomAttributes)
                {
                    var targetAttribute = new CustomAttribute(rootImporter.RootImport(sourceAttribute.Constructor));
                    if (sourceAttribute.HasConstructorArguments)
                    {
                        foreach (var sourceArgument in sourceAttribute.ConstructorArguments)
                        {
                            targetAttribute.ConstructorArguments.Add(
                                new CustomAttributeArgument(
                                    rootImporter.RootImport(sourceArgument.Type),
                                    rootImporter.DynamicRootImport(sourceArgument.Value)));
                        }
                    }

                    if (sourceAttribute.HasProperties)
                    {
                        foreach (var sourceProperty in sourceAttribute.Properties)
                        {
                            targetAttribute.Properties.Add(
                                new CustomAttributeNamedArgument(
                                    sourceProperty.Name,
                                    new CustomAttributeArgument(
                                        rootImporter.RootImport(sourceProperty.Argument.Type),
                                        rootImporter.DynamicRootImport(sourceProperty.Argument.Value))));
                        }
                    }
                    target.CustomAttributes.Add(targetAttribute);
                }
            }
        }

        public static GenericParameter ImportForDeclaringMethod(this GenericParameter sourceGenericParameter, MethodReference rootImportedDeclaringMethod)
        {
            Contract.Requires(sourceGenericParameter != null);
            Contract.Requires(sourceGenericParameter.Type == GenericParameterType.Method);
            Contract.Requires(sourceGenericParameter.DeclaringMethod != null);
            Contract.Requires(rootImportedDeclaringMethod != null);

            if (sourceGenericParameter.Position >= rootImportedDeclaringMethod.GenericParameters.Count)
            {
                throw new WeavingException(string.Format(
                    "Could not find generic parameter in position [{0}] of method [{1}]. Expected to find one with name [{2}]",
                    sourceGenericParameter.Position,
                    rootImportedDeclaringMethod.FullName,
                    sourceGenericParameter.Name));
            }

            var importedGenericParameter = rootImportedDeclaringMethod.GenericParameters[sourceGenericParameter.Position];
            Contract.Assert(importedGenericParameter != null);

            if (importedGenericParameter.Name != sourceGenericParameter.Name)
            {
                throw new WeavingException(string.Format(
                    "Expected to find generic parameter named [{0}] in position [{1}] of method [{2}]. Instead found one with name [{3}]",
                    sourceGenericParameter.Name,
                    sourceGenericParameter.Position,
                    rootImportedDeclaringMethod.FullName,
                    importedGenericParameter.Name));
            }

            return importedGenericParameter;
        }

    }
}
