/***************************************************************************/
// Copyright 2013-2018 Riley White
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

using NUnit.Framework;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Cilador.Fody.Tests.Common
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
            var actualCount = type.GetFields(TestContent.BindingFlagsForWeavedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                $"Expected {expectedCount} fields but found {actualCount} in type [{type.FullName}]");
        }

        public static void ValidatePropertyCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetProperties(TestContent.BindingFlagsForWeavedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                $"Expected {expectedCount} properties but found {actualCount} in type [{type.FullName}]");
        }

        public static void ValidateConstructorCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetConstructors(TestContent.BindingFlagsForWeavedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                $"Expected {expectedCount} constructors but found {actualCount} in type [{type.FullName}]");
        }

        public static void ValidateMethodCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetMethods(TestContent.BindingFlagsForWeavedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                $"Expected {expectedCount} non-constructor methods but found {actualCount} in type [{type.FullName}]");
        }

        public static void ValidateEventCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetEvents(TestContent.BindingFlagsForWeavedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                $"Expected {expectedCount} events but found {actualCount} in type [{type.FullName}]");
        }

        public static void ValidateNestedTypeCountIs(this Type type, int expectedCount)
        {
            Contract.Requires(type != null);
            var actualCount = type.GetNestedTypes(TestContent.BindingFlagsForWeavedMembers).Length;
            Assert.That(
                expectedCount == actualCount,
                $"Expected {expectedCount} nested types but found {actualCount} in type [{type.FullName}]");
        }

        public static string GetShortAssemblyQualifiedName(this Type type)
        {
            Contract.Requires(type != null);
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        public static void ValidateMemberSources(this Type targetType, Type sourceType)
        {
            Contract.Requires(targetType != null);
            Contract.Requires(sourceType != null);
            Contract.Requires(targetType != sourceType);

            var rootTargetAndSourceFullNames = Tuple.Create(targetType.FullName, sourceType.FullName);

            foreach (var targetField in targetType.GetFields(TestContent.BindingFlagsForWeavedMembers))
            {
                targetField.ValidateSourceEqual(sourceType.GetField(targetField.Name, TestContent.BindingFlagsForWeavedMembers), rootTargetAndSourceFullNames);
            }

            foreach (var targetMethod in targetType.GetMethods(TestContent.BindingFlagsForWeavedMembers))
            {
                // can't match by parameter types because some of the generic type arguments may have been redirected during mixing
                targetMethod.ValidateSourceEqual(
                    sourceType.GetMethods(TestContent.BindingFlagsForWeavedMembers)
                        .SingleOrDefault(method => targetMethod.IsSourceNameEqual(method, rootTargetAndSourceFullNames)),
                    rootTargetAndSourceFullNames);
            }

            foreach (var targetProperty in targetType.GetProperties(TestContent.BindingFlagsForWeavedMembers))
            {
                targetProperty.ValidateSourceEqual(sourceType.GetProperty(
                    targetProperty.Name,
                    TestContent.BindingFlagsForWeavedMembers,
                    null,
                    targetProperty.PropertyType,
                    targetProperty.GetIndexParameters().Select(each => each.ParameterType).ToArray(),
                    null), rootTargetAndSourceFullNames);
            }

            foreach (var targetEvent in targetType.GetEvents(TestContent.BindingFlagsForWeavedMembers))
            {
                targetEvent.ValidateSourceEqual(sourceType.GetEvent(
                    targetEvent.Name,
                    TestContent.BindingFlagsForWeavedMembers), rootTargetAndSourceFullNames);
            }

            foreach (var targetNestedType in targetType.GetNestedTypes(TestContent.BindingFlagsForWeavedMembers))
            {
                targetNestedType.ValidateSourceEqual(sourceType.GetNestedType(targetNestedType.Name, TestContent.BindingFlagsForWeavedMembers), rootTargetAndSourceFullNames);
            }
        }

        private static void ValidateSourceEqual(this FieldInfo targetField, FieldInfo sourceField, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetField != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(sourceField != null, "Could not find source type");
            Assert.That(sourceField != targetField);
            Assert.That(sourceField.Attributes == targetField.Attributes);
            Assert.That(sourceField.DeclaringType != targetField.DeclaringType);
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

            targetField.FieldType.ValidateSourceNameEqual(sourceField.FieldType, rootTargetAndSourceFullNames);

            if (sourceField.IsLiteral)
            {
                var sourceValue = sourceField.GetRawConstantValue();
                var targetValue = targetField.GetRawConstantValue();

                if (sourceValue == null) { Assert.That(targetValue == null); }
                else { Assert.That(sourceValue.Equals(targetValue)); }
            }

            Attribute.GetCustomAttributes(targetField, false).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceField, false), rootTargetAndSourceFullNames);
        }

        private static void ValidateSourceEqual(this MethodInfo targetMethod, MethodInfo sourceMethod, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetMethod != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

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


            targetMethod.ReturnType.ValidateSourceNameEqual(sourceMethod.ReturnType, rootTargetAndSourceFullNames);

            targetMethod.ReturnParameter.ValidateSourceEqual(sourceMethod.ReturnParameter, rootTargetAndSourceFullNames);
            targetMethod.GetParameters().ValidateSourceEqual(sourceMethod.GetParameters(), rootTargetAndSourceFullNames);
            Attribute.GetCustomAttributes(targetMethod).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceMethod), rootTargetAndSourceFullNames);
        }

        private static void ValidateSourceEqual(this PropertyInfo targetProperty, PropertyInfo sourceProperty, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetProperty != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(sourceProperty != null, "Could not find source property");
            Assert.That(sourceProperty != targetProperty);
            Assert.That(sourceProperty.Attributes == targetProperty.Attributes);
            Assert.That(sourceProperty.CanRead == targetProperty.CanRead);
            Assert.That(sourceProperty.CanWrite == targetProperty.CanWrite);
            Assert.That(sourceProperty.DeclaringType != targetProperty.DeclaringType);
            Assert.That(sourceProperty.IsSpecialName == targetProperty.IsSpecialName);
            Assert.That(sourceProperty.MemberType == targetProperty.MemberType);
            Assert.That(sourceProperty.Name == targetProperty.Name);

            targetProperty.PropertyType.ValidateSourceNameEqual(sourceProperty.PropertyType, rootTargetAndSourceFullNames); // won't work for mixed types

            if (sourceProperty.GetMethod == null) { Assert.That(targetProperty.GetMethod == null); }
            else { targetProperty.GetMethod.ValidateSourceNameEqual(sourceProperty.GetMethod, rootTargetAndSourceFullNames); }

            if (sourceProperty.SetMethod == null) { Assert.That(targetProperty.SetMethod == null); }
            else { targetProperty.SetMethod.ValidateSourceNameEqual(sourceProperty.SetMethod, rootTargetAndSourceFullNames); }

            Attribute.GetCustomAttributes(targetProperty).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceProperty), rootTargetAndSourceFullNames);

            targetProperty.GetIndexParameters().ValidateSourceEqual(sourceProperty.GetIndexParameters(), rootTargetAndSourceFullNames);
        }

        private static void ValidateSourceEqual(this EventInfo targetEvent, EventInfo sourceEvent, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetEvent != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(sourceEvent != null, "Could not find source event");
            Assert.That(sourceEvent != targetEvent);
            Assert.That(sourceEvent.Attributes == targetEvent.Attributes);
            Assert.That(sourceEvent.DeclaringType != targetEvent.DeclaringType);
            Assert.That(sourceEvent.IsMulticast == targetEvent.IsMulticast);
            Assert.That(sourceEvent.IsSpecialName == targetEvent.IsSpecialName);
            Assert.That(sourceEvent.MemberType == targetEvent.MemberType);
            Assert.That(sourceEvent.Name == targetEvent.Name);

            targetEvent.EventHandlerType.ValidateSourceNameEqual(sourceEvent.EventHandlerType, rootTargetAndSourceFullNames);

            // only check names here; full check should be done separately for the methods
            if (sourceEvent.AddMethod == null) { Assert.That(targetEvent.AddMethod == null); }
            else { targetEvent.AddMethod.ValidateSourceNameEqual(sourceEvent.AddMethod, rootTargetAndSourceFullNames); }

            if (sourceEvent.RemoveMethod == null) { Assert.That(targetEvent.RemoveMethod == null); }
            else { targetEvent.RemoveMethod.ValidateSourceNameEqual(sourceEvent.RemoveMethod, rootTargetAndSourceFullNames); }

            if (sourceEvent.RaiseMethod == null) { Assert.That(targetEvent.RaiseMethod == null); }
            else { targetEvent.RaiseMethod.ValidateSourceNameEqual(sourceEvent.RaiseMethod, rootTargetAndSourceFullNames); }

            Attribute.GetCustomAttributes(targetEvent).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceEvent), rootTargetAndSourceFullNames);
        }

        private static void ValidateSourceEqual(this Type targetType, Type sourceType, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetType != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(sourceType != null, "Could not find source type");
            Assert.That(sourceType != targetType);
            Assert.That(sourceType.Attributes == targetType.Attributes);
            Assert.That(sourceType.ContainsGenericParameters == targetType.ContainsGenericParameters);
            Assert.That(sourceType.DeclaringType != targetType.DeclaringType);
            Assert.That(sourceType.GUID != targetType.GUID);
            Assert.That(sourceType.HasElementType == targetType.HasElementType);
            Assert.That(sourceType.IsAbstract == targetType.IsAbstract);
            Assert.That(sourceType.IsAnsiClass == targetType.IsAnsiClass);
            Assert.That(sourceType.IsArray == targetType.IsArray);
            Assert.That(sourceType.IsAutoClass == targetType.IsAutoClass);
            Assert.That(sourceType.IsAutoLayout == targetType.IsAutoLayout);
            Assert.That(sourceType.IsByRef == targetType.IsByRef);
            Assert.That(sourceType.IsClass == targetType.IsClass);
            Assert.That(sourceType.IsCOMObject == targetType.IsCOMObject);
            Assert.That(sourceType.IsConstructedGenericType == targetType.IsConstructedGenericType);
            Assert.That(sourceType.IsContextful == targetType.IsContextful);
            Assert.That(sourceType.IsEnum == targetType.IsEnum);
            Assert.That(sourceType.IsExplicitLayout == targetType.IsExplicitLayout);
            Assert.That(sourceType.IsGenericType == targetType.IsGenericType);
            Assert.That(sourceType.IsGenericTypeDefinition == targetType.IsGenericTypeDefinition);
            Assert.That(sourceType.IsImport == targetType.IsImport);
            Assert.That(sourceType.IsInterface == targetType.IsInterface);
            Assert.That(sourceType.IsLayoutSequential == targetType.IsLayoutSequential);
            Assert.That(sourceType.IsMarshalByRef == targetType.IsMarshalByRef);
            Assert.That(sourceType.IsNested == targetType.IsNested);
            Assert.That(sourceType.IsNestedAssembly == targetType.IsNestedAssembly);
            Assert.That(sourceType.IsNestedFamANDAssem == targetType.IsNestedFamANDAssem);
            Assert.That(sourceType.IsNestedFamily == targetType.IsNestedFamily);
            Assert.That(sourceType.IsNestedFamORAssem == targetType.IsNestedFamORAssem);
            Assert.That(sourceType.IsNestedPrivate == targetType.IsNestedPrivate);
            Assert.That(sourceType.IsNestedPublic == targetType.IsNestedPublic);
            Assert.That(sourceType.IsNotPublic == targetType.IsNotPublic);
            Assert.That(sourceType.IsPointer == targetType.IsPointer);
            Assert.That(sourceType.IsPrimitive == targetType.IsPrimitive);
            Assert.That(sourceType.IsPublic == targetType.IsPublic);
            Assert.That(sourceType.IsSealed == targetType.IsSealed);
            Assert.That(sourceType.IsSecurityCritical == targetType.IsSecurityCritical);
            Assert.That(sourceType.IsSecuritySafeCritical == targetType.IsSecuritySafeCritical);
            Assert.That(sourceType.IsSecurityTransparent == targetType.IsSecurityTransparent);
            Assert.That(sourceType.IsSerializable == targetType.IsSerializable);
            Assert.That(sourceType.IsSpecialName == targetType.IsSpecialName);
            Assert.That(sourceType.IsUnicodeClass == targetType.IsUnicodeClass);
            Assert.That(sourceType.IsValueType == targetType.IsValueType);
            Assert.That(sourceType.IsVisible == targetType.IsVisible);
            Assert.That(sourceType.MemberType == targetType.MemberType);
            Assert.That(sourceType.Name == targetType.Name);
            Assert.That(sourceType.UnderlyingSystemType != targetType.UnderlyingSystemType);

            if (sourceType.BaseType == null) { Assert.That(targetType.BaseType == null); }
            else { targetType.BaseType.ValidateSourceNameEqual(sourceType.BaseType, rootTargetAndSourceFullNames); }

            Assert.That(targetType.IsGenericParameter == sourceType.IsGenericParameter);

            if (sourceType.StructLayoutAttribute == null) { Assert.That(targetType.StructLayoutAttribute == null); }
            else { Assert.That(sourceType.StructLayoutAttribute.Match(targetType.StructLayoutAttribute)); }

            Assert.That(targetType.GenericTypeArguments.Length == sourceType.GenericTypeArguments.Length);
            for (int i = 0; i < targetType.GenericTypeArguments.Length && i < sourceType.GenericTypeArguments.Length; i++)
            {
                targetType.GenericTypeArguments[i].ValidateSourceNameEqual(sourceType.GenericTypeArguments[i], rootTargetAndSourceFullNames);
            }

            targetType.ValidateMemberCountsAre(
                sourceType.GetConstructors(TestContent.BindingFlagsForWeavedMembers).Length,
                sourceType.GetMethods(TestContent.BindingFlagsForWeavedMembers).Length,
                sourceType.GetFields(TestContent.BindingFlagsForWeavedMembers).Length,
                sourceType.GetProperties(TestContent.BindingFlagsForWeavedMembers).Length,
                sourceType.GetEvents(TestContent.BindingFlagsForWeavedMembers).Length,
                sourceType.GetNestedTypes(TestContent.BindingFlagsForWeavedMembers).Length);

            Attribute.GetCustomAttributes(targetType).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceType), rootTargetAndSourceFullNames);
        }

        private static void ValidateSourceEqual(this ParameterInfo[] targetParameters, ParameterInfo[] sourceParameters, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetParameters != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(sourceParameters != null, "Could not find source parameters");
            Assert.That(sourceParameters != targetParameters);
            Assert.That(sourceParameters.Length == targetParameters.Length);

            for (int i = 0; i < sourceParameters.Length && i < targetParameters.Length; i++)
            {
                targetParameters[i].ValidateSourceEqual(sourceParameters[i], rootTargetAndSourceFullNames);
            }
        }

        private static void ValidateSourceEqual(this ParameterInfo targetParameter, ParameterInfo sourceParameter, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetParameter != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

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
            Assert.That(sourceParameter.Position == targetParameter.Position);

            targetParameter.ParameterType.ValidateSourceNameEqual(sourceParameter.ParameterType, rootTargetAndSourceFullNames);

            if (sourceParameter.RawDefaultValue == null) { Assert.That(targetParameter.RawDefaultValue == null); }
            else { Assert.That(sourceParameter.RawDefaultValue.Equals(targetParameter.RawDefaultValue)); }

            Attribute.GetCustomAttributes(targetParameter).ValidateSourceEqual(Attribute.GetCustomAttributes(sourceParameter), rootTargetAndSourceFullNames);
        }

        private static void ValidateSourceEqual(this Attribute[] targetAttributes, Attribute[] sourceAttributes, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(targetAttributes != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

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
                    return string.Compare(left.ToXElement().ToString(), right.ToXElement().ToString(), StringComparison.Ordinal);
                });

            sourceAttributeList.Sort(attributeSorter);
            targetAttributeList.Sort(attributeSorter);

            for (var i = 0; i < sourceAttributeList.Count && i < targetAttributeList.Count; i++)
            {
                Assert.That(sourceAttributeList[i].Match(targetAttributeList[i]));  // this will fail for mixed type arguments
            }
        }

        private static void ValidateSourceNameEqual(this Type target, Type source, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(target.IsSourceNameEqual(source, rootTargetAndSourceFullNames));
        }

        private static bool IsSourceNameEqual(this Type target, Type source, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            if (source.GenericTypeArguments.Length != target.GenericTypeArguments.Length) { return false; }
            if (source.IsArray != target.IsArray) { return false; }

            if (source.IsArray)
            {
                return
                    source.GetArrayRank() == target.GetArrayRank() &&
                    target.GetElementType().IsSourceNameEqual(source.GetElementType(), rootTargetAndSourceFullNames);
            }
            else if (source.GenericTypeArguments.Length == 0)
            {
                return source.FullName.Replace(rootTargetAndSourceFullNames.Item2, rootTargetAndSourceFullNames.Item1) == target.FullName;
            }
            else
            {
                if (!target.GetGenericTypeDefinition().IsSourceNameEqual(source.GetGenericTypeDefinition(), rootTargetAndSourceFullNames)) { return false; }

                return !source.GenericTypeArguments.Where(
                    (t, i) => !target.GenericTypeArguments[i].IsSourceNameEqual(t, rootTargetAndSourceFullNames)).Any();
            }
        }

        private static void ValidateSourceNameEqual(this MethodInfo target, MethodInfo source, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            Assert.That(target.IsSourceNameEqual(source, rootTargetAndSourceFullNames));
        }

        private static bool IsSourceNameEqual(this MethodInfo target, MethodInfo source, Tuple<string, string> rootTargetAndSourceFullNames)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Requires(rootTargetAndSourceFullNames != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item2 != null);
            Contract.Requires(rootTargetAndSourceFullNames.Item1 != rootTargetAndSourceFullNames.Item2);

            if (!target.DeclaringType.IsSourceNameEqual(source.DeclaringType, rootTargetAndSourceFullNames)) { return false; }

            if (source.IsGenericMethodDefinition || !source.IsGenericMethod)
            {
                return source.ToString().Replace(rootTargetAndSourceFullNames.Item2, rootTargetAndSourceFullNames.Item1) == target.ToString();
            }
            else
            {
                if (target.IsGenericMethodDefinition || !target.IsGenericMethod) { return false; }

                var sourceGenericArguments = source.GetGenericArguments();
                var targetGenericArguments = target.GetGenericArguments();

                if (sourceGenericArguments.Length != targetGenericArguments.Length) { return false; }

                if (!target.GetGenericMethodDefinition().IsSourceNameEqual(source.GetGenericMethodDefinition(), rootTargetAndSourceFullNames)) { return false; }

                return !sourceGenericArguments.Where(
                    (t, i) => !targetGenericArguments[i].IsSourceNameEqual(t, rootTargetAndSourceFullNames)).Any();
            }
        }
    }
}
