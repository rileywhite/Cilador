using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixing;
using Bix.Mixers.Fody.TestInterfaces;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.InterfaceMixTests
{
    [TestFixture]
    internal class InterfaceWithOnlyPrimitiveTypesFixture
    {
        [Test]
        public void CanImplementInterfaceImplicitly()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IInterfaceWithOnlyPrimitiveTypes, Bix.Mixers.Fody.TestInterfaces",
                            Template = "Bix.Mixers.Fody.TestSources.InterfaceWithOnlyPrimitiveTypesImplicitTemplate, Bix.Mixers.Fody.TestSources"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config);
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTargets.InterfaceWithOnlyPrimitiveTypesTarget");

            Assert.That(typeof(IInterfaceWithOnlyPrimitiveTypes).IsAssignableFrom(targetType));

            Assert.That(targetType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 1, "Expected 1 constructor");
            Assert.That(targetType.GetConstructor(new Type[0]) != null, "Lost existing default constructor");

            Assert.That(targetType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 14, "Expected 14 fields");

            Assert.That(targetType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length == 14, "Expected 14 properties");

            var instanceObject = Activator.CreateInstance(targetType, new object[0]);
            Assert.That(instanceObject is IInterfaceWithOnlyPrimitiveTypes);
            var instance = (IInterfaceWithOnlyPrimitiveTypes)instanceObject;

            instance.BooleanProperty = true;
            Assert.That(instance.BooleanProperty);
            instance.BooleanProperty = false;
            Assert.That(!instance.BooleanProperty);

            instance.ByteProperty = Byte.MinValue;
            Assert.That(instance.ByteProperty == Byte.MinValue);
            instance.ByteProperty = 0;
            Assert.That(instance.ByteProperty == 0);
            instance.ByteProperty = Byte.MaxValue;
            Assert.That(instance.ByteProperty == Byte.MaxValue);

            instance.SByteProperty = SByte.MinValue;
            Assert.That(instance.SByteProperty == SByte.MinValue);
            instance.SByteProperty = 0;
            Assert.That(instance.SByteProperty == 0);
            instance.SByteProperty = SByte.MaxValue;
            Assert.That(instance.SByteProperty == SByte.MaxValue);

            instance.Int16Property = Int16.MinValue;
            Assert.That(instance.Int16Property == Int16.MinValue);
            instance.Int16Property = 0;
            Assert.That(instance.Int16Property == 0);
            instance.Int16Property = Int16.MaxValue;
            Assert.That(instance.Int16Property == Int16.MaxValue);
            
            instance.UInt16Property = UInt16.MinValue;
            Assert.That(instance.UInt16Property == UInt16.MinValue);
            instance.UInt16Property = 0;
            Assert.That(instance.UInt16Property == 0);
            instance.UInt16Property = UInt16.MaxValue;
            Assert.That(instance.UInt16Property == UInt16.MaxValue);
            
            instance.Int32Property = Int32.MinValue;
            Assert.That(instance.Int32Property == Int32.MinValue);
            instance.Int32Property = 0;
            Assert.That(instance.Int32Property == 0);
            instance.Int32Property = Int32.MaxValue;
            Assert.That(instance.Int32Property == Int32.MaxValue);
            
            instance.UInt32Property = UInt32.MinValue;
            Assert.That(instance.UInt32Property == UInt32.MinValue);
            instance.UInt32Property = 0;
            Assert.That(instance.UInt32Property == 0);
            instance.UInt32Property = UInt32.MaxValue;
            Assert.That(instance.UInt32Property == UInt32.MaxValue);
            
            instance.Int64Property = Int64.MinValue;
            Assert.That(instance.Int64Property == Int64.MinValue);
            instance.Int64Property = 0;
            Assert.That(instance.Int64Property == 0);
            instance.Int64Property = Int64.MaxValue;
            Assert.That(instance.Int64Property == Int64.MaxValue);
            
            instance.UInt64Property = UInt64.MinValue;
            Assert.That(instance.UInt64Property == UInt64.MinValue);
            instance.UInt64Property = 0;
            Assert.That(instance.UInt64Property == 0);
            instance.UInt64Property = UInt64.MaxValue;
            Assert.That(instance.UInt64Property == UInt64.MaxValue);

            instance.IntPtrProperty = IntPtr.Add(IntPtr.Zero, 402);
            Assert.That(instance.IntPtrProperty == IntPtr.Add(IntPtr.Zero, 402));
            instance.IntPtrProperty = IntPtr.Zero;
            Assert.That(instance.IntPtrProperty == IntPtr.Zero);
            
            instance.UIntPtrProperty = UIntPtr.Add(UIntPtr.Zero, 4987);
            Assert.That(instance.UIntPtrProperty == UIntPtr.Add(UIntPtr.Zero, 4987));
            instance.UIntPtrProperty = UIntPtr.Zero;
            Assert.That(instance.UIntPtrProperty == UIntPtr.Zero);
            
            instance.CharProperty = Char.MinValue;
            Assert.That(instance.CharProperty == Char.MinValue);
            instance.CharProperty = (Char)0;
            Assert.That(instance.CharProperty == 0);
            instance.CharProperty = Char.MaxValue;
            Assert.That(instance.CharProperty == Char.MaxValue);
            
            instance.DoubleProperty = Double.MinValue;
            Assert.That(instance.DoubleProperty == Double.MinValue);
            instance.DoubleProperty = 0;
            Assert.That(instance.DoubleProperty == 0);
            instance.DoubleProperty = Double.MaxValue;
            Assert.That(instance.DoubleProperty == Double.MaxValue);
            
            instance.SingleProperty = Single.MinValue;
            Assert.That(instance.SingleProperty == Single.MinValue);
            instance.SingleProperty = 0;
            Assert.That(instance.SingleProperty == 0);
            instance.SingleProperty = Single.MaxValue;
            Assert.That(instance.SingleProperty == Single.MaxValue);
        }
    }
}
