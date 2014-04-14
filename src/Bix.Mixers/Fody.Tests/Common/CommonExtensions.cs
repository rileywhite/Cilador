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
        public static BindingFlags BindingFlagsForMixedMembers =
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

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
            var actualCount = type.GetFields(BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} fields but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidatePropertyCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetProperties(BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} properties but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateConstructorCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetConstructors(BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} constructors but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateMethodCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetMethods(BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} non-constructor methods but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateEventCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetEvents(BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} events but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }

        public static void ValidateNestedTypeCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetNestedTypes(BindingFlagsForMixedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                string.Format("Expected {0} nested types but found {1} in type [{2}]", expectedCount, actualCount, type.FullName));
        }
    }
}
