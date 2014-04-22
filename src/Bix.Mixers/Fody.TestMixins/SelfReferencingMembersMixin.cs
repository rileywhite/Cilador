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

        public delegate void InnerDelegate(string arg0);

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

        public void ReferenceAllTypesInParametersWithReferences(
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
            InnerImplementingClass innerImplementingClass)
        {
            string toStringHolder;

            toStringHolder = innerInterface.ToString();
            toStringHolder = innerEventArgs.ToString();
            toStringHolder = innerDelegate.ToString();
            toStringHolder = innerEnum.ToString();
            toStringHolder = innerStruct.ToString();
            toStringHolder = innerStructInnerEnum.ToString();
            toStringHolder = innerStructInnerInterface.ToString();
            toStringHolder = innerStructInnerStruct.ToString();
            toStringHolder = innerStructInnerClass.ToString();
            toStringHolder = innerAbstractClass.ToString();
            toStringHolder = innerAbstractClassInnerEnum.ToString();
            toStringHolder = innerAbstractClassInnerInterface.ToString();
            toStringHolder = innerAbstractClassStruct.ToString();
            toStringHolder = innerImplementingClass.ToString();
        }

        public void ReferenceAllTypesInLocalVariables()
        {
#pragma warning disable 168
            IInnerInterface innerInterface;
            InnerEventArgs innerEventArgs;
            InnerDelegate innerDelegate;
            InnerEnum innerEnum;
            InnerStruct innerStruct;
            InnerStruct.InnerStructInnerEnum innerStructInnerEnum;
            InnerStruct.IInnerStructInnerInterface innerStructInnerInterface;
            InnerStruct.InnerStructInnerStruct innerStructInnerStruct;
            InnerStruct.InnerStructInnerClass innerStructInnerClass;
            InnerAbstractClass innerAbstractClass;
            InnerAbstractClass.InnerAbstractClassInnerEnum innerAbstractClassInnerEnum;
            InnerAbstractClass.IInnerAbstractClassInnerInterface innerAbstractClassInnerInterface;
            InnerAbstractClass.InnerAbstractClassStruct innerAbstractClassStruct;
            InnerImplementingClass innerImplementingClass;
#pragma warning restore 168
        }

        public void ReferenceAllTypesInLocalVariablesWithInitialization()
        {
#pragma warning disable 219
            IInnerInterface innerInterface = new InnerImplementingClass();
            InnerEventArgs innerEventArgs = new InnerEventArgs();
            InnerDelegate innerDelegate = Console.WriteLine;
            InnerEnum innerEnum = InnerEnum.Two;
            InnerStruct innerStruct = new InnerStruct();
            InnerStruct.InnerStructInnerEnum innerStructInnerEnum = InnerStruct.InnerStructInnerEnum.Two;
            InnerStruct.IInnerStructInnerInterface innerStructInnerInterface = new InnerImplementingClass();
            InnerStruct.InnerStructInnerStruct innerStructInnerStruct = new InnerStruct.InnerStructInnerStruct();
            InnerStruct.InnerStructInnerClass innerStructInnerClass = new InnerStruct.InnerStructInnerClass();
            InnerAbstractClass innerAbstractClass = new InnerImplementingClass();
            InnerAbstractClass.InnerAbstractClassInnerEnum innerAbstractClassInnerEnum = InnerAbstractClass.InnerAbstractClassInnerEnum.Two;
            InnerAbstractClass.IInnerAbstractClassInnerInterface innerAbstractClassInnerInterface = new InnerImplementingClass();
            InnerAbstractClass.InnerAbstractClassStruct innerAbstractClassStruct = new InnerAbstractClass.InnerAbstractClassStruct();
            InnerImplementingClass innerImplementingClass = new InnerImplementingClass();
#pragma warning restore 219
        }

        public void ReferenceAllTypesInLocalVariablesWithInitializationWithReferences()
        {
            IInnerInterface innerInterface = new InnerImplementingClass();
            InnerEventArgs innerEventArgs = new InnerEventArgs();
            InnerDelegate innerDelegate = Console.WriteLine;
            InnerEnum innerEnum = InnerEnum.Two;
            InnerStruct innerStruct = new InnerStruct();
            InnerStruct.InnerStructInnerEnum innerStructInnerEnum = InnerStruct.InnerStructInnerEnum.Two;
            InnerStruct.IInnerStructInnerInterface innerStructInnerInterface = new InnerImplementingClass();
            InnerStruct.InnerStructInnerStruct innerStructInnerStruct = new InnerStruct.InnerStructInnerStruct();
            InnerStruct.InnerStructInnerClass innerStructInnerClass = new InnerStruct.InnerStructInnerClass();
            InnerAbstractClass innerAbstractClass = new InnerImplementingClass();
            InnerAbstractClass.InnerAbstractClassInnerEnum innerAbstractClassInnerEnum = InnerAbstractClass.InnerAbstractClassInnerEnum.Two;
            InnerAbstractClass.IInnerAbstractClassInnerInterface innerAbstractClassInnerInterface = new InnerImplementingClass();
            InnerAbstractClass.InnerAbstractClassStruct innerAbstractClassStruct = new InnerAbstractClass.InnerAbstractClassStruct();
            InnerImplementingClass innerImplementingClass = new InnerImplementingClass();

            string toStringHolder;

            toStringHolder = innerInterface.ToString();
            toStringHolder = innerEventArgs.ToString();
            toStringHolder = innerDelegate.ToString();
            toStringHolder = innerEnum.ToString();
            toStringHolder = innerStruct.ToString();
            toStringHolder = innerStructInnerEnum.ToString();
            toStringHolder = innerStructInnerInterface.ToString();
            toStringHolder = innerStructInnerStruct.ToString();
            toStringHolder = innerStructInnerClass.ToString();
            toStringHolder = innerAbstractClass.ToString();
            toStringHolder = innerAbstractClassInnerEnum.ToString();
            toStringHolder = innerAbstractClassInnerInterface.ToString();
            toStringHolder = innerAbstractClassStruct.ToString();
            toStringHolder = innerImplementingClass.ToString();
        }

        public void ReferenceAllTypesInClosedGenericParameters(
            Tuple<IInnerInterface, InnerEventArgs, InnerDelegate> arg0,
            List<InnerEnum> innerEnums,
            InnerStruct[] innerStructs,
            Lazy<InnerStruct.InnerStructInnerEnum> innerStructInnerEnum,
            IEnumerable<InnerStruct.IInnerStructInnerInterface> innerStructInnerInterface,
            Dictionary<InnerStruct.InnerStructInnerStruct, InnerStruct.InnerStructInnerClass> innerStructInnerClassByInnerStructInnerStruct,
            Func<InnerAbstractClass, InnerAbstractClass.InnerAbstractClassInnerEnum, InnerAbstractClass.IInnerAbstractClassInnerInterface> getInnerAbstractClass,
            List<Dictionary<InnerAbstractClass.InnerAbstractClassStruct, IEnumerable<InnerImplementingClass[]>>> stuff) { }

        public void ReferenceAllTypesInClosedGenericParametersWithReferences(
            Tuple<IInnerInterface, InnerEventArgs, InnerDelegate> arg0,
            List<InnerEnum> innerEnums,
            InnerStruct[] innerStructs,
            Lazy<InnerStruct.InnerStructInnerEnum> innerStructInnerEnum,
            IEnumerable<InnerStruct.IInnerStructInnerInterface> innerStructInnerInterface,
            Dictionary<InnerStruct.InnerStructInnerStruct, InnerStruct.InnerStructInnerClass> innerStructInnerClassByInnerStructInnerStruct,
            Func<InnerAbstractClass, InnerAbstractClass.InnerAbstractClassInnerEnum, InnerAbstractClass.IInnerAbstractClassInnerInterface> getInnerAbstractClass,
            List<Dictionary<InnerAbstractClass.InnerAbstractClassStruct, IEnumerable<InnerImplementingClass[]>>> stuff)
        {
            string toStringHolder;

            toStringHolder = arg0.ToString();
            toStringHolder = arg0.Item1.ToString();
            toStringHolder = arg0.Item2.ToString();
            toStringHolder = arg0.Item3.ToString();
            toStringHolder = innerEnums.ToString();
            toStringHolder = innerEnums.First().ToString();
            toStringHolder = innerStructs.ToString();
            toStringHolder = innerStructs[0].ToString();
            toStringHolder = innerStructInnerEnum.ToString();
            toStringHolder = innerStructInnerEnum.Value.ToString();
            toStringHolder = innerStructInnerInterface.ToString();
            toStringHolder = innerStructInnerInterface.ToString();
            toStringHolder = innerStructInnerClassByInnerStructInnerStruct.ToString();
            toStringHolder = innerStructInnerClassByInnerStructInnerStruct.Keys.First().ToString();
            toStringHolder = innerStructInnerClassByInnerStructInnerStruct.Values.First().ToString();
            toStringHolder = getInnerAbstractClass.ToString();
            toStringHolder = stuff.ToString();
            toStringHolder = stuff[0].ToString();
            toStringHolder = stuff[0].Keys.First().ToString();
            toStringHolder = stuff[0].Values.First().ToString();
            toStringHolder = stuff[0].Values.First().First().ToString();
        }
    }
}
