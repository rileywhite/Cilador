using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Tests.Common
{
    internal static class CommonExtensions
    {
        public static XElement ToXElement(this object @object)
        {
            Contract.Requires(@object != null);
            Contract.Requires(@object.GetType().IsSerializable);

            var objectType = @object.GetType();
            using (var memoryStream = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(objectType);
                xmlSerializer.Serialize(memoryStream, @object);
                memoryStream.Position = 0;
                var xElement = XElement.Load(memoryStream);
                return xElement;
            }
        }

        public static void ValidateMemberCountsAre(
            this Type type,
            int expectedConstructorCount,
            int expectedMethodCount,
            int expectedFieldCount,
            int expectedPropertyCount,
            int expectedEventCount,
            int expectedNestedTypeCount)
        {
            Contract.Requires(type != null);
            type.ValidateConstructorCountIs(expectedConstructorCount);
            type.ValidateMethodCountIs(expectedMethodCount);
            type.ValidateFieldCountIs(expectedFieldCount);
            type.ValidatePropertyCountIs(expectedPropertyCount);
            type.ValidateEventCountIs(expectedEventCount);
            type.ValidateNestedTypeCountIs(expectedNestedTypeCount);
        }

        public static void ValidateFieldCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetFields(TestContent.BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} fields but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidatePropertyCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetProperties(TestContent.BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} properties but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateConstructorCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetConstructors(TestContent.BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} constructors but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateMethodCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetMethods(TestContent.BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} non-constructor methods but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateEventCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetEvents(TestContent.BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} events but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateNestedTypeCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetNestedTypes(TestContent.BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} nested types but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static string GetShortAssemblyQualifiedName(this Type type)
        {
            Contract.Requires(type != null);
            return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
        }

        public static void ValidateSourceEqual(this FieldInfo targetField, FieldInfo sourceField)
        {
            Contract.Requires(targetField != null);

            Assert.That(sourceField != null, "Could not find source type");
            Assert.That(sourceField != targetField);
            Assert.That(sourceField.Attributes == targetField.Attributes);
            Assert.That(sourceField.DeclaringType != targetField.DeclaringType);
            Assert.That(sourceField.FieldType == targetField.FieldType);    // this will fail for mixed types (top-level target and inner types)
            Assert.That(sourceField.IsAssembly == targetField.IsAssembly);
            Assert.That(sourceField.IsFamily == targetField.IsFamily);
            Assert.That(sourceField.IsFamilyAndAssembly == targetField.IsFamilyAndAssembly);
            Assert.That(sourceField.IsFamilyOrAssembly == targetField.IsFamilyOrAssembly);
            Assert.That(sourceField.IsInitOnly == targetField.IsInitOnly);
            Assert.That(sourceField.IsLiteral == targetField.IsLiteral);
            Assert.That(sourceField.IsNotSerialized == targetField.IsNotSerialized);
            Assert.That(sourceField.IsPinvokeImpl == targetField.IsPinvokeImpl);
            Assert.That(sourceField.IsPrivate == targetField.IsPrivate);
            Assert.That(sourceField.IsPublic == targetField.IsPublic);
            Assert.That(sourceField.IsSecurityCritical == targetField.IsSecurityCritical);
            Assert.That(sourceField.IsSecuritySafeCritical == targetField.IsSecuritySafeCritical);
            Assert.That(sourceField.IsSecurityTransparent == targetField.IsSecurityTransparent);
            Assert.That(sourceField.IsSpecialName == targetField.IsSpecialName);
            Assert.That(sourceField.IsStatic == targetField.IsStatic);
            Assert.That(sourceField.MemberType == targetField.MemberType);
            Assert.That(sourceField.Name == targetField.Name);

            if (sourceField.IsLiteral)
            {
                var sourceValue = sourceField.GetRawConstantValue();
                var targetValue = targetField.GetRawConstantValue();

                if (sourceValue == null) { Assert.That(targetValue == null); }
                else { Assert.That(sourceValue.Equals(targetValue)); }
            }

            Attribute.GetCustomAttributes(targetField, false).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceField, false));
        }

        public static void ValidateSourceEqual(this MethodInfo targetMethod, MethodInfo sourceMethod)
        {
            Contract.Requires(targetMethod != null);

            Assert.That(sourceMethod != null, "Could not find source method");
            Assert.That(sourceMethod != targetMethod);
            Assert.That(sourceMethod.Attributes == targetMethod.Attributes);
            Assert.That(sourceMethod.CallingConvention == targetMethod.CallingConvention);
            Assert.That(sourceMethod.ContainsGenericParameters == targetMethod.ContainsGenericParameters);
            Assert.That(sourceMethod.DeclaringType != targetMethod.DeclaringType);
            Assert.That(sourceMethod.IsAbstract == targetMethod.IsAbstract);
            Assert.That(sourceMethod.IsAssembly == targetMethod.IsAssembly);
            Assert.That(sourceMethod.IsConstructor == targetMethod.IsConstructor);
            Assert.That(sourceMethod.IsFamily == targetMethod.IsFamily);
            Assert.That(sourceMethod.IsFamilyAndAssembly == targetMethod.IsFamilyAndAssembly);
            Assert.That(sourceMethod.IsFamilyOrAssembly == targetMethod.IsFamilyOrAssembly);
            Assert.That(sourceMethod.IsFinal == targetMethod.IsFinal);
            Assert.That(sourceMethod.IsGenericMethod == targetMethod.IsGenericMethod);
            Assert.That(sourceMethod.IsGenericMethodDefinition == targetMethod.IsGenericMethodDefinition);
            Assert.That(sourceMethod.IsHideBySig == targetMethod.IsHideBySig);
            Assert.That(sourceMethod.IsPrivate == targetMethod.IsPrivate);
            Assert.That(sourceMethod.IsPublic == targetMethod.IsPublic);
            Assert.That(sourceMethod.IsSecurityCritical == targetMethod.IsSecurityCritical);
            Assert.That(sourceMethod.IsSecuritySafeCritical == targetMethod.IsSecuritySafeCritical);
            Assert.That(sourceMethod.IsSecurityTransparent == targetMethod.IsSecurityTransparent);
            Assert.That(sourceMethod.IsSpecialName == targetMethod.IsSpecialName);
            Assert.That(sourceMethod.IsStatic == targetMethod.IsStatic);
            Assert.That(sourceMethod.IsVirtual == targetMethod.IsVirtual);
            Assert.That(sourceMethod.MemberType == targetMethod.MemberType);
            Assert.That(sourceMethod.MethodImplementationFlags == targetMethod.MethodImplementationFlags);
            Assert.That(sourceMethod.Name == targetMethod.Name);
            Assert.That(sourceMethod.ReturnType == targetMethod.ReturnType);    // this will fail for mixed types (top-level target and inner types)

            targetMethod.ReturnParameter.ValidateSourceEqual(sourceMethod.ReturnParameter);
            targetMethod.GetParameters().ValidateSourceEqual(sourceMethod.GetParameters());
            Attribute.GetCustomAttributes(targetMethod).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceMethod));
        }


        public static void ValidateSourceEqual(this PropertyInfo targetProperty, PropertyInfo sourceProperty)
        {
            Contract.Requires(targetProperty != null);

            Assert.That(sourceProperty != null, "Could not find source property");
            Assert.That(sourceProperty != targetProperty);
            Assert.That(sourceProperty.Attributes == targetProperty.Attributes);
            Assert.That(sourceProperty.CanRead == targetProperty.CanRead);
            Assert.That(sourceProperty.CanWrite == targetProperty.CanWrite);
            Assert.That(sourceProperty.DeclaringType != targetProperty.DeclaringType);
            Assert.That(sourceProperty.IsSpecialName == targetProperty.IsSpecialName);
            Assert.That(sourceProperty.MemberType == targetProperty.MemberType);
            Assert.That(sourceProperty.Name == targetProperty.Name);
            Assert.That(sourceProperty.PropertyType == targetProperty.PropertyType); // won't work for mixed types

            // only check names here; full check should be done separately for the methods
            if (sourceProperty.GetMethod == null) { Assert.That(targetProperty.GetMethod == null); }
            else { Assert.That(sourceProperty.GetMethod.Name == targetProperty.GetMethod.Name); }

            if (sourceProperty.SetMethod == null) { Assert.That(targetProperty.SetMethod == null); }
            else { Assert.That(sourceProperty.SetMethod.Name == targetProperty.SetMethod.Name); }

            Attribute.GetCustomAttributes(targetProperty).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceProperty));

            targetProperty.GetIndexParameters().ValidateSourceEqual(sourceProperty.GetIndexParameters());
        }

        public static void ValidateSourceEqual(this ParameterInfo[] targetParameters, ParameterInfo[] sourceParameters)
        {
            Contract.Requires(targetParameters != null);

            Assert.That(sourceParameters != null, "Could not find source parameters");
            Assert.That(sourceParameters != targetParameters);
            Assert.That(sourceParameters.Length == targetParameters.Length);

            for (int i = 0; i < sourceParameters.Length && i < targetParameters.Length; i++)
            {
                targetParameters[i].ValidateSourceEqual(sourceParameters[i]);
            }
        }

        public static void ValidateSourceEqual(this ParameterInfo targetParameter, ParameterInfo sourceParameter)
        {
            Contract.Requires(targetParameter != null);

            Assert.That(sourceParameter != null, "Could not find source parameter");
            Assert.That(sourceParameter != targetParameter);
            Assert.That(sourceParameter.Attributes == targetParameter.Attributes);

            if (sourceParameter.DefaultValue == null) { Assert.That(targetParameter.DefaultValue == null); }
            else { Assert.That(sourceParameter.DefaultValue.Equals(targetParameter.DefaultValue)); }
            
            Assert.That(sourceParameter.HasDefaultValue == targetParameter.HasDefaultValue);
            Assert.That(sourceParameter.IsIn == targetParameter.IsIn);
            Assert.That(sourceParameter.IsLcid == targetParameter.IsLcid);
            Assert.That(sourceParameter.IsOptional == targetParameter.IsOptional);
            Assert.That(sourceParameter.IsOut == targetParameter.IsOut);
            Assert.That(sourceParameter.IsRetval == targetParameter.IsRetval);
            Assert.That(sourceParameter.Member != targetParameter.Member);
            Assert.That(sourceParameter.Name == targetParameter.Name);
            Assert.That(sourceParameter.ParameterType == targetParameter.ParameterType);    // won't work for mixed types
            Assert.That(sourceParameter.Position == targetParameter.Position);

            if (sourceParameter.RawDefaultValue == null) { Assert.That(targetParameter.RawDefaultValue == null); }
            else { Assert.That(sourceParameter.RawDefaultValue.Equals(targetParameter.RawDefaultValue)); }

            Attribute.GetCustomAttributes(targetParameter).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceParameter));
        }

        public static void ValidateSourceEqual(this Attribute[] targetAttributes, Attribute[] sourceAttributes)
        {
            Contract.Requires(targetAttributes != null);

            Assert.That(sourceAttributes != null, "Could not find source attributes");
            Assert.That(sourceAttributes != targetAttributes);
            Assert.That(sourceAttributes.Length == targetAttributes.Length);

            var sourceAttributeList = sourceAttributes.ToList();
            var targetAttributeList = targetAttributes.ToList();

            Assert.That(sourceAttributeList.Count == targetAttributeList.Count);

            if (sourceAttributeList.Count == 0) { return; }

            var attributeSorter = new Comparison<Attribute>(
                (left, right) =>
                {
                    var comparison = left.GetType().MetadataToken.CompareTo(right.GetType().MetadataToken);
                    if (comparison != 0 || left.Match(right)) { return comparison; }

                    // same type, and non-matching instances
                    // don't really care what "less than" means, just picking something consistent
                    return left.ToXElement().ToString().CompareTo(right.ToXElement().ToString());
                });

            sourceAttributeList.Sort(attributeSorter);
            targetAttributeList.Sort(attributeSorter);

            for (var i = 0; i < sourceAttributeList.Count && i < targetAttributeList.Count; i++)
            {
                Assert.That(sourceAttributeList[i].Match(targetAttributeList[i]));  // this will fail for mixed type arguments
            }
        }
    }
}
