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
        public static void Clone(this IEnumerable<IMemberCloner> cloners)
        {
            Contract.Requires(cloners != null);
            Contract.Requires(!cloners.Any(cloner => cloner.IsCloned));
            Contract.Ensures(cloners.All(cloner => cloner.IsCloned));

            foreach (var cloner in cloners) { cloner.Clone(); }
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

        public static bool IsSkipped(this System.Reflection.MemberInfo member)
        {
            Contract.Requires(member != null);

            var method = member as System.Reflection.MethodBase;
            if (method == null)
            {
                return Attribute.IsDefined(member, typeof(SkipAttribute));
            }
            else { return method.IsSkipped(); }
        }

        public static bool IsSkipped(this System.Reflection.MethodBase method)
        {
            Contract.Requires(method != null);

            if (Attribute.IsDefined(method, typeof(SkipAttribute))) { return true; }

            return method.DeclaringType.GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic).Any(
                property =>
                    (property.GetMethod == method || property.SetMethod == method) &&
                    property.IsSkipped());
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
                        targetAttribute.ConstructorArguments.Add(new CustomAttributeArgument(
                            rootImporter.RootImport(sourceArgument.Type),

                            rootImporter.DynamicRootImport(sourceArgument.Value)));
                    }
                }
                target.CustomAttributes.Add(targetAttribute);
            }
        }
    }
}
