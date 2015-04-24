/***************************************************************************/
// Copyright 2013-2015 Riley White
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

using Cilador.Fody.Config;
using Cilador.Fody.Core;
using Cilador.Fody.TestMixinInterfaces;
using Cilador.Fody.TestMixins;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{
    using Cilador.Fody.Config;
    using Cilador.Fody.TestMixinInterfaces;
    using Cilador.Fody.TestMixins;
    using Cilador.Fody.TestMixinTargets;
    using Cilador.Fody.Tests.Common;

    [TestFixture]
    internal class GenericsFixture
    {
        [Test]
        public void CanMixGenericMethod()
        {
            var config = new CiladorConfigType();

            config.WeaverConfig = new WeaverConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(GenericMethodMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 2, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            // simple generic method
            var method = targetType.GetMethod("GenericMethod", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                method,
                GenericParameterTypeValidator.Named("T"),
                GenericParameterTypeValidator.Named("T"));

            var genericParameters = method.GetGenericArguments();
            Assert.That(genericParameters, Is.Not.Null);
            Assert.That(genericParameters.Length, Is.EqualTo(1));

            var instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instance is IEmptyInterface);

            var genericInstanceMethod = method.MakeGenericMethod(typeof(int));
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                NonGenericTypeValidator.ForType<int>(),
                NonGenericTypeValidator.ForType<int>());
            Assert.That(genericInstanceMethod.Invoke(instance, new object[] { 94387 }), Is.EqualTo(94387));

            genericInstanceMethod = method.MakeGenericMethod(typeof(object));
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                NonGenericTypeValidator.ForType<object>(),
                NonGenericTypeValidator.ForType<object>());
            var someObject = new object();
            Assert.That(genericInstanceMethod.Invoke(instance, new object[] { someObject }), Is.SameAs(someObject));

            // generic method with constraints
            method = targetType.GetMethod("GenericMethodWithConstraints", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                method,
                new GenericTypeValidator(typeof(Tuple<,,,,,>),
                    GenericParameterTypeValidator.Named("TClass"),
                    GenericParameterTypeValidator.Named("TStruct"),
                    GenericParameterTypeValidator.Named("TNew"),
                    GenericParameterTypeValidator.Named("TClassNew"),
                    GenericParameterTypeValidator.Named("TDisposable"),
                    GenericParameterTypeValidator.Named("TTClass")),
                GenericParameterTypeValidator.Named("TClass"),
                GenericParameterTypeValidator.Named("TStruct"),
                GenericParameterTypeValidator.Named("TNew"),
                GenericParameterTypeValidator.Named("TClassNew"),
                GenericParameterTypeValidator.Named("TDisposable"),
                GenericParameterTypeValidator.Named("TTClass"));

            genericParameters = method.GetGenericArguments();
            Assert.That(genericParameters, Is.Not.Null);
            Assert.That(genericParameters.Length, Is.EqualTo(6));

            instance = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instance is IEmptyInterface);

            genericInstanceMethod = method.MakeGenericMethod(
                typeof(Stream),
                typeof(DictionaryEntry),
                typeof(DictionaryEntry),
                typeof(object),
                typeof(StringWriter),
                typeof(FileStream));
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                new GenericTypeValidator(typeof(Tuple<,,,,,>),
                    NonGenericTypeValidator.ForType<Stream>(),
                    NonGenericTypeValidator.ForType<DictionaryEntry>(),
                    NonGenericTypeValidator.ForType<DictionaryEntry>(),
                    NonGenericTypeValidator.ForType<object>(),
                    NonGenericTypeValidator.ForType<StringWriter>(),
                    NonGenericTypeValidator.ForType<FileStream>()),
                NonGenericTypeValidator.ForType<Stream>(),
                NonGenericTypeValidator.ForType<DictionaryEntry>(),
                NonGenericTypeValidator.ForType<DictionaryEntry>(),
                NonGenericTypeValidator.ForType<object>(),
                NonGenericTypeValidator.ForType<StringWriter>(),
                NonGenericTypeValidator.ForType<FileStream>());
        }

        [Test]
        public void CanMixGenericNestedType()
        {
            var config = new CiladorConfigType();

            config.WeaverConfig = new WeaverConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMixinMap = new InterfaceMixinMapType[]
                    {
                        new InterfaceMixinMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(GenericNestedTypeMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 0, 0, 0, 6);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            // basic nested generic type
            var type = targetType.GetNestedType("GenericType`1", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.ContainsGenericParameters);
            TypeValidatorBase.ValidateType(
                type,
                new GenericTypeValidator(
                    type,
                    GenericParameterTypeValidator.Named("T")));

            var method = type.GetMethod("GetThing", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                method,
                GenericParameterTypeValidator.Named("T"),
                GenericParameterTypeValidator.Named("T"));

            var genericInstanceType = type.MakeGenericType(typeof(int));
            Assert.That(!genericInstanceType.ContainsGenericParameters);
            Assert.That(genericInstanceType.IsConstructedGenericType);

            var instance = Activator.CreateInstance(genericInstanceType, new object[0]);

            var genericInstanceMethod = genericInstanceType.GetMethod("GetThing", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(genericInstanceMethod, Is.Not.Null);
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                NonGenericTypeValidator.ForType<int>(),
                NonGenericTypeValidator.ForType<int>());
            Assert.That(genericInstanceMethod.Invoke(instance, new object[] { 556 }), Is.EqualTo(556));

            genericInstanceType = type.MakeGenericType(typeof(object));
            Assert.That(!genericInstanceType.ContainsGenericParameters);
            Assert.That(genericInstanceType.IsConstructedGenericType);

            instance = Activator.CreateInstance(genericInstanceType, new object[0]);

            genericInstanceMethod = genericInstanceType.GetMethod("GetThing", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(genericInstanceMethod, Is.Not.Null);
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                NonGenericTypeValidator.ForType<object>(),
                NonGenericTypeValidator.ForType<object>());
            var someObject = new object();
            Assert.That(genericInstanceMethod.Invoke(instance, new object[] { someObject }), Is.SameAs(someObject));

            // nested generic type with constraints
            type = targetType.GetNestedType("GenericTypeWithConstraints`6", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.ContainsGenericParameters);
            TypeValidatorBase.ValidateType(
                type,
                new GenericTypeValidator(
                    type,
                    GenericParameterTypeValidator.Named("TClass"),
                    GenericParameterTypeValidator.Named("TStruct"),
                    GenericParameterTypeValidator.Named("TNew"),
                    GenericParameterTypeValidator.Named("TClassNew"),
                    GenericParameterTypeValidator.Named("TDisposable"),
                    GenericParameterTypeValidator.Named("TTClass")));

            genericInstanceType = type.MakeGenericType(
                typeof(Stream),
                typeof(DictionaryEntry),
                typeof(DictionaryEntry),
                typeof(object),
                typeof(StringWriter),
                typeof(FileStream));
            Assert.That(!genericInstanceType.ContainsGenericParameters);
            Assert.That(genericInstanceType.IsConstructedGenericType);

            instance = Activator.CreateInstance(genericInstanceType, new object[0]);

            genericInstanceMethod = genericInstanceType.GetMethod("GetThings", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(genericInstanceMethod, Is.Not.Null);
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                new GenericTypeValidator(typeof(Tuple<,,,,,>),
                    NonGenericTypeValidator.ForType<Stream>(),
                    NonGenericTypeValidator.ForType<DictionaryEntry>(),
                    NonGenericTypeValidator.ForType<DictionaryEntry>(),
                    NonGenericTypeValidator.ForType<object>(),
                    NonGenericTypeValidator.ForType<StringWriter>(),
                    NonGenericTypeValidator.ForType<FileStream>()),
                NonGenericTypeValidator.ForType<Stream>(),
                NonGenericTypeValidator.ForType<DictionaryEntry>(),
                NonGenericTypeValidator.ForType<DictionaryEntry>(),
                NonGenericTypeValidator.ForType<object>(),
                NonGenericTypeValidator.ForType<StringWriter>(),
                NonGenericTypeValidator.ForType<FileStream>());

            // nested generic type with open generic method
            type = targetType.GetNestedType("GenericTypeWithGenericMethod`1", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.ContainsGenericParameters);
            TypeValidatorBase.ValidateType(
                type,
                new GenericTypeValidator(
                    type,
                    GenericParameterTypeValidator.Named("TType")));

            method = type.GetMethod("GetThings", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                method,
                new GenericTypeValidator(typeof(Tuple<,>),
                    GenericParameterTypeValidator.Named("TType"),
                    GenericParameterTypeValidator.Named("TMethod")),
                GenericParameterTypeValidator.Named("TType"),
                GenericParameterTypeValidator.Named("TMethod"));

            genericInstanceType = type.MakeGenericType(typeof(int));
            Assert.That(!genericInstanceType.ContainsGenericParameters);
            Assert.That(genericInstanceType.IsConstructedGenericType);

            instance = Activator.CreateInstance(genericInstanceType, new object[0]);

            var genericInstanceGenericMethod = genericInstanceType.GetMethod("GetThings", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(genericInstanceGenericMethod, Is.Not.Null);
            Assert.That(genericInstanceGenericMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceGenericMethod,
                new GenericTypeValidator(typeof(Tuple<,>),
                    NonGenericTypeValidator.ForType<int>(),
                    GenericParameterTypeValidator.Named("TMethod")),
                NonGenericTypeValidator.ForType<int>(),
                GenericParameterTypeValidator.Named("TMethod"));

            genericInstanceMethod = genericInstanceGenericMethod.MakeGenericMethod(typeof(object));
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                new GenericTypeValidator(typeof(Tuple<,>),
                    NonGenericTypeValidator.ForType<int>(),
                    NonGenericTypeValidator.ForType<object>()),
                NonGenericTypeValidator.ForType<int>(),
                NonGenericTypeValidator.ForType<object>());
            someObject = new object();
            var things = (Tuple<int, object>)genericInstanceMethod.Invoke(instance, new object[] { 45866, someObject });
            Assert.That(things.Item1, Is.EqualTo(45866));
            Assert.That(things.Item2, Is.SameAs(someObject));

            // generic type with multiple parameters
            type = targetType.GetNestedType("GenericTypeWithMultipleParameters`2", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(type, Is.Not.Null);
            TypeValidatorBase.ValidateType(
                type,
                new GenericTypeValidator(
                    type,
                    GenericParameterTypeValidator.Named("T1"),
                    GenericParameterTypeValidator.Named("T2")));

            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                type.GetProperty("Thing1"),
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                GenericParameterTypeValidator.Named("T1"));

            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                type.GetProperty("Thing2"),
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                GenericParameterTypeValidator.Named("T2"));

            genericInstanceType = type.MakeGenericType(typeof(int), typeof(string));
            Assert.That(!genericInstanceType.ContainsGenericParameters);
            Assert.That(genericInstanceType.IsConstructedGenericType);

            instance = Activator.CreateInstance(genericInstanceType, new object[0]);

            var genericInstanceProperty = genericInstanceType.GetProperty("Thing1", TestContent.BindingFlagsForWeavedMembers);
            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                genericInstanceProperty,
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                NonGenericTypeValidator.ForType<int>());
            genericInstanceProperty.SetValue(instance, 48008);
            Assert.That(genericInstanceProperty.GetValue(instance), Is.EqualTo(48008));

            genericInstanceProperty = genericInstanceType.GetProperty("Thing2", TestContent.BindingFlagsForWeavedMembers);
            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                genericInstanceProperty,
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                NonGenericTypeValidator.ForType<string>());
            genericInstanceProperty.SetValue(instance, "SAdfiohoqiweAsiohnewlroi asoidfh inra6f");
            Assert.That(genericInstanceProperty.GetValue(instance), Is.EqualTo("SAdfiohoqiweAsiohnewlroi asoidfh inra6f"));

            // partially closed generic type
            type = targetType.GetNestedType("PartiallyClosedGenericType`1", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(type, Is.Not.Null);
            TypeValidatorBase.ValidateType(
                type,
                new GenericTypeValidator(
                    type,
                    GenericParameterTypeValidator.Named("T3")));

            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                type.GetProperty("Thing1"),
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                NonGenericTypeValidator.ForType<int>());

            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                type.GetProperty("Thing2"),
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                GenericParameterTypeValidator.Named("T3"));

            genericInstanceType = type.MakeGenericType(typeof(string));
            Assert.That(!genericInstanceType.ContainsGenericParameters);
            Assert.That(genericInstanceType.IsConstructedGenericType);

            instance = Activator.CreateInstance(genericInstanceType, new object[0]);

            genericInstanceProperty = genericInstanceType.GetProperty(
                "Thing1",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                genericInstanceProperty,
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                NonGenericTypeValidator.ForType<int>());
            genericInstanceProperty.SetValue(instance, 5446);
            Assert.That(genericInstanceProperty.GetValue(instance), Is.EqualTo(5446));

            genericInstanceProperty = genericInstanceType.BaseType.GetProperty(
                "Thing2",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            TypeValidatorBase.ValidatePropertyTypeAndAccessors(
                genericInstanceProperty,
                TypeValidatorBase.PropertyAccessorExpectations.Both,
                NonGenericTypeValidator.ForType<string>());
            genericInstanceProperty.SetValue(instance, "AFioenAjeiona aesoing ijeng inuia eabruibeg uybvwaytvr  b");
            Assert.That(genericInstanceProperty.GetValue(instance), Is.EqualTo("AFioenAjeiona aesoing ijeng inuia eabruibeg uybvwaytvr  b"));


            // type with method with partially closed generic type
            type = targetType.GetNestedType("TypeWithPartiallyClosedGenericMethod", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(type, Is.Not.Null);
            Assert.That(!type.IsGenericType);

            instance = Activator.CreateInstance(type);

            var returnTypeGenericDefinition = targetType.GetNestedType("GenericTypeWithMultipleParameters`2", TestContent.BindingFlagsForWeavedMembers);

            method = type.GetMethod("GetThing", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            TypeValidatorBase.ValidateParameters(
                method,
                new GenericTypeValidator(
                    returnTypeGenericDefinition,
                    NonGenericTypeValidator.ForType<int>(),
                    GenericParameterTypeValidator.Named("T4")),
                GenericParameterTypeValidator.Named("T4"));

            genericInstanceMethod = method.MakeGenericMethod(new Type[] { typeof(string) });
            var returnValue = genericInstanceMethod.Invoke(instance, new object[] { "Eoahio eioua bueagingnpiuw nufib fuiwebyu" });
            var genericInstanceReturnType = returnTypeGenericDefinition.MakeGenericType(new Type[] { typeof(int), typeof(string) });
            Assert.That(returnValue.GetType(), Is.EqualTo(genericInstanceReturnType));
            Assert.That(genericInstanceReturnType.GetProperty("Thing1").GetValue(returnValue), Is.EqualTo(297387));
            Assert.That(genericInstanceReturnType.GetProperty("Thing2").GetValue(returnValue), Is.EqualTo("Eoahio eioua bueagingnpiuw nufib fuiwebyu"));

            method = type.GetMethod("GetOtherThing", TestContent.BindingFlagsForWeavedMembers);
            Assert.That(method, Is.Not.Null);
            TypeValidatorBase.ValidateParameters(
                method,
                new GenericTypeValidator(
                    returnTypeGenericDefinition,
                    GenericParameterTypeValidator.Named("T5"),
                    NonGenericTypeValidator.ForType<int>()),
                GenericParameterTypeValidator.Named("T5"));

            genericInstanceMethod = method.MakeGenericMethod(new Type[] { typeof(string) });
            returnValue = genericInstanceMethod.Invoke(instance, new object[] { "Eaiosn4oil ifhiu 3ybusabu 3byuvsdf yuvaytithomn gfd" });
            genericInstanceReturnType = returnTypeGenericDefinition.MakeGenericType(new Type[] { typeof(string), typeof(int) });
            Assert.That(returnValue.GetType(), Is.EqualTo(genericInstanceReturnType));
            Assert.That(genericInstanceReturnType.GetProperty("Thing1").GetValue(returnValue), Is.EqualTo("Eaiosn4oil ifhiu 3ybusabu 3byuvsdf yuvaytithomn gfd"));
            Assert.That(genericInstanceReturnType.GetProperty("Thing2").GetValue(returnValue), Is.EqualTo(789437));
        }

        //[Test]
        //public void CanUseOpenGenericMixinWithoutProvidingTypeArguments()
        //{
        //    var config = new CiladorConfigType();

        //    config.WeaverConfig = new WeaverConfigTypeBase[]
        //    {
        //        new InterfaceMixinConfigType
        //        {
        //            InterfaceMixinMap = new InterfaceMixinMapType[]
        //            {
        //                new InterfaceMixinMapType
        //                {
        //                    Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
        //                    Mixin = typeof(OpenGenericMixin<>).GetShortAssemblyQualifiedName()
        //                }
        //            }
        //        },
        //    };

        //    ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
        //}

        //        [Test]
        //        public void CanUseOpenGenericMixinInterface()
        //        {
        //            var config = new CiladorConfigType();

        //            config.WeaverConfig = new WeaverConfigTypeBase[]
        //            {
        //                new InterfaceMixinConfigType
        //                {
        //                    InterfaceMixinMap = new InterfaceMixinMapType[]
        //                    {
        //                        new InterfaceMixinMapType
        //                        {
        //                            Interface = typeof(IGenericMixinDefinition<,,,>).GetShortAssemblyQualifiedName(),
        //                            Mixin = typeof(GenericMethodMixin).GetShortAssemblyQualifiedName()
        //                        }
        //                    }
        //                },
        //            };

        //            ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
        //        }

        //        [Test]
        //        public void CanMixOpenGenericMixinIfClosedWithTypeArguments()
        //        {
        //            var config = new CiladorConfigType();

        //            config.WeaverConfig = new WeaverConfigTypeBase[]
        //            {
        //                new InterfaceMixinConfigType
        //                {
        //                    InterfaceMixinMap = new InterfaceMixinMapType[]
        //                    {
        //                        new InterfaceMixinMapType
        //                        {
        //                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
        //                            Mixin = typeof(OpenGenericMixin<int>).GetShortAssemblyQualifiedName()
        //                        }
        //                    }
        //                },
        //            };

        //            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
        //            var targetType = assembly.GetType("Cilador.Fody.TestMixinTargets.EmptyInterfaceTarget");
        //            Assert.That(typeof(Cilador.Fody.TestMixinInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
        //            Assert.That(targetType.GetConstructors(TestContent.BindingFlagsForWeavedMembers).Length == 1, "Expected 1 constructor");
        //            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

        //            var valueField = targetType.GetField("Value", TestContent.BindingFlagsForWeavedMembers);
        //            Assert.That(valueField != null);
        //            Assert.That(typeof(int) == valueField.FieldType);
        //        }

        //        [Test]
        //        public void CanUseClosedGenericMixinInterface()
        //        {
        //            var config = new CiladorConfigType();

        //            config.WeaverConfig = new WeaverConfigTypeBase[]
        //            {
        //                new InterfaceMixinConfigType
        //                {
        //                    InterfaceMixinMap = new InterfaceMixinMapType[]
        //                    {
        //                        new InterfaceMixinMapType
        //                        {
        //                            Interface = typeof(IGenericMixinDefinition<object, GenericMixinStruct, Stream, UnhandledExceptionEventArgs>).GetShortAssemblyQualifiedName(),
        //                            Mixin = typeof(GenericMethodMixin).GetShortAssemblyQualifiedName()
        //                        }
        //                    }
        //                },
        //            };

        //            ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
        //        }
    }
}
