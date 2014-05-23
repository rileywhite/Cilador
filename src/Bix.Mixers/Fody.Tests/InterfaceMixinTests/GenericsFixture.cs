///***************************************************************************/
//// Copyright 2013-2014 Riley White
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
///***************************************************************************/

//using Bix.Mixers.Fody.Config;
//using Bix.Mixers.Fody.Core;
//using Bix.Mixers.Fody.TestMixinInterfaces;
//using Bix.Mixers.Fody.TestMixins;
//using Bix.Mixers.Fody.Tests.Common;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
//{
//    [TestFixture]
//    internal class GenericsFixture
//    {
//        [Test]
//        public void CanUseOpenGenericMixinWithoutProvidingTypeArguments()
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
//                            Mixin = typeof(OpenGenericMixin<>).GetShortAssemblyQualifiedName()
//                        }
//                    }
//                },
//            };

//            ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
//        }

//        [Test]
//        public void CanMixGenericMethod()
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
//                            Mixin = typeof(GenericMethodMixin).GetShortAssemblyQualifiedName()
//                        }
//                    }
//                },
//            };

//            ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
//        }

//        [Test]
//        public void CanMixOpenGenericNestedType()
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
//                            Mixin = typeof(OpenGenericNestedTypeMixin).GetShortAssemblyQualifiedName()
//                        }
//                    }
//                },
//            };

//            ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
//        }

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

        //[Test]
        //public void CanMixOpenGenericMixinIfClosedWithTypeArguments()
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
        //                    Mixin = typeof(OpenGenericMixin<int>).GetShortAssemblyQualifiedName()
        //                }
        //            }
        //        },
        //    };

        //    var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
        //    var targetType = assembly.GetType("Bix.Mixers.Fody.TestMixinTargets.EmptyInterfaceTarget");
        //    Assert.That(typeof(Bix.Mixers.Fody.TestMixinInterfaces.IEmptyInterface).IsAssignableFrom(targetType));
        //    Assert.That(targetType.GetConstructors(TestContent.BindingFlagsForMixedMembers).Length == 1, "Expected 1 constructor");
        //    Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

        //    var valueField = targetType.GetField("Value", TestContent.BindingFlagsForMixedMembers);
        //    Assert.That(valueField != null);
        //    Assert.That(typeof(int) == valueField.FieldType);
        //}

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
//    }
//}
