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
            Assert.That(sourceField.Attributes == targetField.Attributes);
            Assert.That(sourceField.DeclaringType != targetField.DeclaringType);    // should be in different types
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
            Assert.That(sourceField.MetadataToken != targetField.MetadataToken); // should be different fields
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

        public static void ValidateSourceEqual(this Attribute[] targetAttributes, Attribute[] sourceAttributes)
        {
            Contract.Requires(targetAttributes != null);

            Assert.That(sourceAttributes != null, "Could not find source attributes");

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
                    // (not picking TypeId because that might sort differently for two types with same attributes)
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
