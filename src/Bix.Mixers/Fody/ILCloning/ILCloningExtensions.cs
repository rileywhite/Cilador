﻿using Mono.Cecil;
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
            this Collection<ParameterDefinition> targetParameters,
            Collection<ParameterDefinition> sourceParameters,
            IRootImportProvider rootImporter,
            Dictionary<ParameterDefinition, ParameterDefinition> parameterOperandReplacementMap = null)
        {
            Contract.Requires(targetParameters != null);
            Contract.Requires(sourceParameters != null);
            Contract.Requires(targetParameters != sourceParameters);
            Contract.Requires(targetParameters.Count == 0);
            Contract.Requires(rootImporter != null);
            Contract.Ensures(targetParameters.Count == sourceParameters.Count);

            foreach (var sourceParameter in sourceParameters)
            {
                var targetParameter =
                    new ParameterDefinition(sourceParameter.Name, sourceParameter.Attributes, rootImporter.RootImport(sourceParameter.ParameterType));
                targetParameter.Constant = sourceParameter.Constant;
                targetParameter.HasConstant = sourceParameter.HasConstant;
                targetParameter.HasDefault = sourceParameter.HasDefault;
                targetParameter.HasFieldMarshal = sourceParameter.HasFieldMarshal;
                targetParameter.IsIn = sourceParameter.IsIn;
                targetParameter.IsLcid = sourceParameter.IsLcid;
                targetParameter.IsOptional = sourceParameter.IsOptional;
                targetParameter.IsOut = sourceParameter.IsOut;
                targetParameter.IsReturnValue = sourceParameter.IsReturnValue;

                // TODO research correct usage
                if (sourceParameter.MarshalInfo != null)
                {
                    targetParameter.MarshalInfo = new MarshalInfo(sourceParameter.MarshalInfo.NativeType);
                }

                // TODO research correct usage
                targetParameter.MetadataToken = new MetadataToken(sourceParameter.MetadataToken.TokenType, sourceParameter.MetadataToken.RID);

                // I did not check whether I get a similar issue here as with the duplication in the FieldCloner...adding a clear line just to make sure, though
                targetParameter.CustomAttributes.Clear();
                targetParameter.RootImportAllCustomAttributes(rootImporter, sourceParameter.CustomAttributes);

                targetParameters.Add(targetParameter);
                if (parameterOperandReplacementMap != null) { parameterOperandReplacementMap.Add(sourceParameter, targetParameter); }
            }
        }

        public static void RootImportAllCustomAttributes(this ICustomAttributeProvider target, IRootImportProvider rootImporter, Collection<CustomAttribute> sourceAttributes)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.CustomAttributes != null);
            Contract.Requires(target.CustomAttributes.Count == 0 || target == rootImporter.RootTarget);
            Contract.Requires(rootImporter != null);
            Contract.Requires(sourceAttributes != null);
            Contract.Ensures(
                target.CustomAttributes.Count == sourceAttributes.Count ||
                (target == rootImporter.RootTarget && target.CustomAttributes.Count > sourceAttributes.Count));

            foreach (var sourceAttribute in sourceAttributes)
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
}
