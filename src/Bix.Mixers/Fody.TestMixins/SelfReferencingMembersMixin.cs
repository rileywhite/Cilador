using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class SelfReferencingMembersMixin : IEmptyInterface
    {
        [Skip]
        public SelfReferencingMembersMixin() { }

        private int field;
        public int Property { get; set; }
        public void Method() { }
        public event EventHandler<InnerEventArgs> EventHappened;

        public enum InnerEnum { One, Two }

        public interface IInnerInterface
        {
            int InnerInterfaceProperty { get; set; }
            void InnerInterfaceMethod();
            event EventHandler InnerInterfaceEventHappened;
        }

        public class InnerEventArgs : EventArgs { }

        public delegate void InnerDelegate();

        public struct InnerStruct
        {
            private int innerStructField;
            public int InnerStructProperty { get; set; }
            public void InnerStructMethod() { }
            public event EventHandler InnerStructEventHappened;
            public enum InnerStructInnerEnum { One, Two }
            public struct InnerStructInnerStruct { }
            public interface IInnerStructInnerInterface
            {
                int InnerStructInnerInterfaceProperty { get; set; }
                void InnerStructInnerInterfaceMethod();
                event EventHandler InnerStructInnerInterfaceEventHappened;
            }
            public class InnerStructInnerClass { }
        }

        public abstract class InnerAbstractClass
        {
            private int innerAbstractClassField;

            public int InnerAbstractClassProperty { get; set; }
            public abstract int InnerAbstractClassAbstractProperty { get; set; }
            public void InnerAbstractClassMethod() { }
            public abstract void InnerAbstractClassAbstractMethod();
            public abstract event EventHandler InnerAbstractClassAbstractEventHappened;
            public event EventHandler InnerAbstractClassEventHappened;
            public enum InnerAbstractClassInnerEnum { One, Two }
            public struct InnerAbstractClassStruct { }
            public interface IInnerAbstractClassInnerInterface
            {
                int InnerAbstractClassInnerInterfaceProperty { get; set; }
                void InnerAbstractClassInnerInterfaceMethod();
                event EventHandler InnerAbstractClassInnerInterfaceEventHappened;
            }
        }

        public class InnerImplementingClass
            : InnerAbstractClass, IInnerInterface, InnerStruct.IInnerStructInnerInterface, InnerAbstractClass.IInnerAbstractClassInnerInterface
        {
            public override int InnerAbstractClassAbstractProperty { get; set; }
            public override void InnerAbstractClassAbstractMethod() { }
            public int InnerStructInnerInterfaceProperty { get; set; }
            public int InnerAbstractClassInnerInterfaceProperty { get; set; }
            public int InnerInterfaceProperty { get; set; }
            public void InnerInterfaceMethod() { }
            public void InnerStructInnerInterfaceMethod() { }
            public void InnerAbstractClassInnerInterfaceMethod() { }
            public override event EventHandler InnerAbstractClassAbstractEventHappened;
            public event EventHandler InnerInterfaceEventHappened;
            public event EventHandler InnerStructInnerInterfaceEventHappened;
            public event EventHandler InnerAbstractClassInnerInterfaceEventHappened;
        }

        public void ReferenceAllTypesInParameters(
            IInnerInterface innerInterface,
            InnerEventArgs innerEventArgs,
            InnerDelegate innerDelegate,
            InnerEnum innerEnum,
            InnerStruct innerStruct,
            InnerStruct.InnerStructInnerEnum innerStructInnerEnum,
            InnerStruct.IInnerStructInnerInterface innerStructInnerInterface,
            InnerStruct.InnerStructInnerStruct innerStructInnerStruct,
            InnerStruct.InnerStructInnerClass innerStructInnerClass,
            InnerAbstractClass innerAbstractClass,
            InnerAbstractClass.InnerAbstractClassInnerEnum innerAbstractClassInnerEnum,
            InnerAbstractClass.IInnerAbstractClassInnerInterface innerAbstractClassInnerInterface,
            InnerAbstractClass.InnerAbstractClassStruct innerAbstractClassStruct,
            InnerImplementingClass innerImplementingClass) { }
    }
}
