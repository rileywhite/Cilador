/***************************************************************************/
// Copyright 2013-2014 Riley White
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

using Bix.Mixers.Fody.Config;
using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.TestMixinInterfaces;
using Bix.Mixers.Fody.TestMixins;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    internal class GenericsFixture
    {
        //[Test]
        //public void CanUseOpenGenericMixinWithoutProvidingTypeArguments()
        //{
        //    var config = new BixMixersConfigType();

        //    config.MixCommandConfig = new MixCommandConfigTypeBase[]
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

        [Test]
        public void CanMixGenericMethod()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 2, 0, 0, 0, 0);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            // simple generic method
            var method = targetType.GetMethod("GenericMethod", TestContent.BindingFlagsForMixedMembers);
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
            method = targetType.GetMethod("GenericMethodWithConstraints", TestContent.BindingFlagsForMixedMembers);
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
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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
            var targetType = assembly.GetType(typeof(Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 0, 0, 0, 0, 2);
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            // basic nested generic type
            var type = targetType.GetNestedType("GenericType`1", TestContent.BindingFlagsForMixedMembers);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.ContainsGenericParameters);
            TypeValidatorBase.ValidateType(
                type,
                new GenericTypeValidator(
                    type,
                    GenericParameterTypeValidator.Named("T")));

            var method = type.GetMethod("GetThing", TestContent.BindingFlagsForMixedMembers);
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

            var genericInstanceMethod = genericInstanceType.GetMethod("GetThing", TestContent.BindingFlagsForMixedMembers);
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

            genericInstanceMethod = genericInstanceType.GetMethod("GetThing", TestContent.BindingFlagsForMixedMembers);
            Assert.That(genericInstanceMethod, Is.Not.Null);
            Assert.That(!genericInstanceMethod.ContainsGenericParameters);
            TypeValidatorBase.ValidateParameters(
                genericInstanceMethod,
                NonGenericTypeValidator.ForType<object>(),
                NonGenericTypeValidator.ForType<object>());
            var someObject = new object();
            Assert.That(genericInstanceMethod.Invoke(instance, new object[] { someObject }), Is.SameAs(someObject));

            // nested generic type with constraints
            type = targetType.GetNestedType("GenericTypeWithConstraints`6", TestContent.BindingFlagsForMixedMembers);
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

            genericInstanceMethod = genericInstanceType.GetMethod("GetThings", TestContent.BindingFlagsForMixedMembers);
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

            // TODO methods that use the type generic parameter and have their own generic parameters

            // TODO nested, partially closed generic types
        }

//        [Test]
//        public void CanUseOpenGenericMixinInterface()
//        {
//            var config = new BixMixersConfigType();

//            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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
//            var config = new BixMixersConfigType();

//            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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
//            var targetType = assembly.GetType("Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget");
//            Assert.That(typeof(Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
//            Assert.That(targetType.GetConstructors(TestContent.BindingFlagsForMixedMembers).Length == 1, "Expected 1 constructor");
//            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

//            var valueField = targetType.GetField("Value", TestContent.BindingFlagsForMixedMembers);
//            Assert.That(valueField != null);
//            Assert.That(typeof(int) == valueField.FieldType);
//        }

//        public struct GenericMixinStruct : IGenericMixinDefinition<object, GenericMixinStruct, Stream, UnhandledExceptionEventArgs>
//        {
//            public IGenericMixinDefinition<object, GenericMixinStruct, Stream, UnhandledExceptionEventArgs> AsInterface(GenericMixinStruct arg0)
//            {
//                throw new NotSupportedException();
//            }

//            public void GenericMethod<T5>(object arg0, UnhandledExceptionEventArgs arg1, T5 arg2)
//            {
//                throw new NotSupportedException();
//            }

//            public Stream SomeProperty
//            {
//                get { throw new NotSupportedException(); }
//                set { throw new NotSupportedException(); }
//            }

//            public int IntProperty
//            {
//                get { throw new NotSupportedException(); }
//                set { throw new NotSupportedException(); }
//            }

//#pragma warning disable 67
//            public event EventHandler<UnhandledExceptionEventArgs> Thinged;
//#pragma warning restore 67
//        }

//        [Test]
//        public void CanUseClosedGenericMixinInterface()
//        {
//            var config = new BixMixersConfigType();

//            config.MixCommandConfig = new MixCommandConfigTypeBase[]
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
