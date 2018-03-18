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

using Cilador.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cilador.Fody.TestMixins
{
    public class SelfReferencingMembersMixin : IEmptyInterface
    {
        private int field;
        public int Property { get; set; }
        public void Method() { Console.WriteLine(this.field); }
        public event EventHandler<InnerEventArgs> EventHappened;
        protected virtual void OnEventHappened(InnerEventArgs e)
        {
            var eventHandler = this.EventHappened;
            eventHandler?.Invoke(this, e);
        }

        private static int staticField;
        public static int StaticProperty { get; set; }
        public static void StaticMethod()
        {
            Console.WriteLine(SelfReferencingMembersMixin.staticField);
        }
        public static event EventHandler<InnerEventArgs> StaticEventHappened;
        protected static void OnStaticEventHappened(InnerEventArgs e)
        {
            var eventHandler = SelfReferencingMembersMixin.StaticEventHappened;
            eventHandler?.Invoke(typeof(SelfReferencingMembersMixin), e);
        }

        public enum InnerEnum { One, Two }

        public interface IInnerInterface
        {
            int InnerInterfaceProperty { get; set; }
            void InnerInterfaceMethod();
            event EventHandler<InnerEventArgs> InnerInterfaceEventHappened;
        }

        public class InnerEventArgs : EventArgs { }

        public delegate void InnerDelegate(string arg0);

        public struct InnerStruct
        {
            private int innerStructField;
            public int InnerStructProperty { get; set; }
            public void InnerStructMethod() { }
            public event EventHandler<InnerEventArgs> InnerStructEventHappened;
            private void OnInnerStructEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerStructEventHappened;
                eventHandler?.Invoke(this, e);
            }

            private static int staticInnerStructField;
            public static int StaticInnerStructProperty { get; set; }
            public static void StaticInnerStructMethod() { }
            public static event EventHandler<InnerEventArgs> StaticInnerStructEventHappened;
            private static void OnStaticInnerStructEventHappened(InnerEventArgs e)
            {
                var eventHandler = InnerStruct.StaticInnerStructEventHappened;
                eventHandler?.Invoke(typeof(InnerStruct), e);
            }

            public enum InnerStructInnerEnum { One, Two }
            public struct InnerStructInnerStruct { }
            public interface IInnerStructInnerInterface
            {
                int InnerStructInnerInterfaceProperty { get; set; }
                void InnerStructInnerInterfaceMethod();
                event EventHandler<InnerEventArgs> InnerStructInnerInterfaceEventHappened;
            }
            public class InnerStructInnerClass { }

            public void InnerStructReferenceAllMembers()
            {
                var events = 0;
                EventHandler<InnerEventArgs> eventDelegate = (object sender, InnerEventArgs e) => { ++events; };
                int i;

                var selfReferencingMembersMixin = new SelfReferencingMembersMixin
                {
                    Property = 4
                };
                selfReferencingMembersMixin.field = selfReferencingMembersMixin.Property;
                selfReferencingMembersMixin.Method();
                selfReferencingMembersMixin.EventHappened += eventDelegate;
                selfReferencingMembersMixin.OnEventHappened(new InnerEventArgs());
                selfReferencingMembersMixin.EventHappened -= eventDelegate;

                SelfReferencingMembersMixin.StaticProperty = 9;
                SelfReferencingMembersMixin.staticField = SelfReferencingMembersMixin.StaticProperty;
                SelfReferencingMembersMixin.StaticMethod();
                SelfReferencingMembersMixin.StaticEventHappened += eventDelegate;
                SelfReferencingMembersMixin.OnStaticEventHappened(new InnerEventArgs());
                SelfReferencingMembersMixin.StaticEventHappened -= eventDelegate;

                this.InnerStructProperty = 54;
                this.innerStructField = this.InnerStructProperty;
                this.InnerStructMethod();
                this.InnerStructEventHappened += eventDelegate;
                this.OnInnerStructEventHappened(new InnerEventArgs());
                this.InnerStructEventHappened -= eventDelegate;
                InnerStruct.StaticInnerStructProperty = 58;
                InnerStruct.staticInnerStructField = InnerStruct.StaticInnerStructProperty;
                InnerStruct.StaticInnerStructMethod();
                InnerStruct.StaticInnerStructEventHappened += eventDelegate;
                InnerStruct.OnStaticInnerStructEventHappened(new InnerEventArgs());
                InnerStruct.StaticInnerStructEventHappened -= eventDelegate;

                InnerAbstractClass.StaticInnerAbstractClassProperty = 32;
                i = InnerAbstractClass.StaticInnerAbstractClassProperty;
                InnerAbstractClass.StaticInnerAbstractClassMethod();
                InnerAbstractClass.StaticInnerAbstractClassEventHappened += eventDelegate;
                InnerAbstractClass.StaticInnerAbstractClassEventHappened -= eventDelegate;

                var innerImplementingClass = new InnerImplementingClass
                {
                    InnerAbstractClassProperty = 85
                };

                i = innerImplementingClass.InnerAbstractClassProperty;
                innerImplementingClass.InnerAbstractClassMethod();
                innerImplementingClass.InnerAbstractClassEventHappened += eventDelegate;
                innerImplementingClass.InnerAbstractClassEventHappened -= eventDelegate;

                innerImplementingClass.InnerAbstractClassAbstractProperty = 781;
                i = innerImplementingClass.InnerAbstractClassAbstractProperty;
                innerImplementingClass.InnerAbstractClassAbstractMethod();
                innerImplementingClass.InnerAbstractClassAbstractEventHappened += eventDelegate;
                innerImplementingClass.InnerAbstractClassAbstractEventHappened -= eventDelegate;

                innerImplementingClass.InnerAbstractClassInnerInterfaceProperty = 85;
                i = innerImplementingClass.InnerAbstractClassInnerInterfaceProperty;
                innerImplementingClass.InnerAbstractClassInnerInterfaceMethod();
                innerImplementingClass.InnerAbstractClassInnerInterfaceEventHappened += eventDelegate;
                innerImplementingClass.InnerAbstractClassInnerInterfaceEventHappened -= eventDelegate;

                innerImplementingClass.InnerInterfaceProperty = 69;
                i = innerImplementingClass.InnerInterfaceProperty;
                innerImplementingClass.InnerInterfaceMethod();
                innerImplementingClass.InnerInterfaceEventHappened += eventDelegate;
                innerImplementingClass.InnerInterfaceEventHappened -= eventDelegate;

                innerImplementingClass.InnerStructInnerInterfaceProperty = 15;
                i = innerImplementingClass.InnerStructInnerInterfaceProperty;
                innerImplementingClass.InnerStructInnerInterfaceMethod();
                innerImplementingClass.InnerStructInnerInterfaceEventHappened += eventDelegate;
                innerImplementingClass.InnerStructInnerInterfaceEventHappened -= eventDelegate;

                innerImplementingClass.InnerImplementingClassProperty = 48;
                i = innerImplementingClass.InnerImplementingClassProperty;
                innerImplementingClass.InnerImplementingClassMethod();
                innerImplementingClass.InnerImplementingClassEventHappened += eventDelegate;
                innerImplementingClass.InnerImplementingClassEventHappened -= eventDelegate;

                InnerImplementingClass.StaticInnerImplementingClassProperty = 13;
                i = InnerImplementingClass.StaticInnerImplementingClassProperty;
                InnerImplementingClass.StaticInnerImplementingClassMethod();
                InnerImplementingClass.StaticInnerImplementingClassEventHappened += eventDelegate;
                InnerImplementingClass.StaticInnerImplementingClassEventHappened -= eventDelegate;
            }
        }

        public abstract class InnerAbstractClass
        {
            private int innerAbstractClassField;

            public int InnerAbstractClassProperty { get; set; }
            public abstract int InnerAbstractClassAbstractProperty { get; set; }
            public void InnerAbstractClassMethod() { }
            public abstract void InnerAbstractClassAbstractMethod();
            public abstract event EventHandler<InnerEventArgs> InnerAbstractClassAbstractEventHappened;
            public event EventHandler<InnerEventArgs> InnerAbstractClassEventHappened;
            protected virtual void OnInnerAbstractClassEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerAbstractClassEventHappened;
                eventHandler?.Invoke(this, e);
            }

            private static int staticInnerAbstractClassField;
            public static int StaticInnerAbstractClassProperty { get; set; }
            public static void StaticInnerAbstractClassMethod() { }
            public static event EventHandler<InnerEventArgs> StaticInnerAbstractClassEventHappened;
            protected static void OnStaticInnerAbstractClassEventHappened(InnerEventArgs e)
            {
                var eventHandler = StaticInnerAbstractClassEventHappened;
                if (eventHandler != null) { eventHandler(typeof(InnerAbstractClass), e); }
            }

            public enum InnerAbstractClassInnerEnum { One, Two }
            public struct InnerAbstractClassStruct { }
            public interface IInnerAbstractClassInnerInterface
            {
                int InnerAbstractClassInnerInterfaceProperty { get; set; }
                void InnerAbstractClassInnerInterfaceMethod();
                event EventHandler<InnerEventArgs> InnerAbstractClassInnerInterfaceEventHappened;
            }

            public void InnerAbstractClassReferenceAllMembers()
            {
                var events = 0;
                EventHandler<InnerEventArgs> eventDelegate = (object sender, InnerEventArgs e) => { ++events; };
                int i;

                var selfReferencingMembersMixin = new SelfReferencingMembersMixin
                {
                    Property = 4
                };
                selfReferencingMembersMixin.field = selfReferencingMembersMixin.Property;
                selfReferencingMembersMixin.Method();
                selfReferencingMembersMixin.EventHappened += eventDelegate;
                selfReferencingMembersMixin.OnEventHappened(new InnerEventArgs());
                selfReferencingMembersMixin.EventHappened -= eventDelegate;

                SelfReferencingMembersMixin.StaticProperty = 9;
                SelfReferencingMembersMixin.staticField = StaticProperty;
                SelfReferencingMembersMixin.StaticMethod();
                SelfReferencingMembersMixin.StaticEventHappened += eventDelegate;
                SelfReferencingMembersMixin.OnStaticEventHappened(new InnerEventArgs());
                SelfReferencingMembersMixin.StaticEventHappened -= eventDelegate;

                InnerStruct innerStruct = new InnerStruct();
                innerStruct.InnerStructProperty = 54;
                i = innerStruct.InnerStructProperty;
                innerStruct.InnerStructMethod();
                innerStruct.InnerStructEventHappened += eventDelegate;
                innerStruct.InnerStructEventHappened -= eventDelegate;
                InnerStruct.StaticInnerStructProperty = 58;
                i = InnerStruct.StaticInnerStructProperty;
                InnerStruct.StaticInnerStructMethod();
                InnerStruct.StaticInnerStructEventHappened += eventDelegate;
                InnerStruct.StaticInnerStructEventHappened -= eventDelegate;

                this.InnerAbstractClassProperty = 85;
                this.innerAbstractClassField = this.InnerAbstractClassProperty;
                this.InnerAbstractClassMethod();
                this.InnerAbstractClassEventHappened += eventDelegate;
                this.OnInnerAbstractClassEventHappened(new InnerEventArgs());
                this.InnerAbstractClassEventHappened -= eventDelegate;

                InnerAbstractClass.StaticInnerAbstractClassProperty = 32;
                InnerAbstractClass.staticInnerAbstractClassField = InnerAbstractClass.StaticInnerAbstractClassProperty;
                InnerAbstractClass.StaticInnerAbstractClassMethod();
                InnerAbstractClass.StaticInnerAbstractClassEventHappened += eventDelegate;
                InnerAbstractClass.OnStaticInnerAbstractClassEventHappened(new InnerEventArgs());
                InnerAbstractClass.StaticInnerAbstractClassEventHappened -= eventDelegate;

                var innerImplementingClass = new InnerImplementingClass
                {
                    InnerAbstractClassProperty = 85
                };

                innerImplementingClass.innerAbstractClassField = innerImplementingClass.InnerAbstractClassProperty;
                innerImplementingClass.InnerAbstractClassMethod();
                innerImplementingClass.InnerAbstractClassEventHappened += eventDelegate;
                innerImplementingClass.InnerAbstractClassEventHappened -= eventDelegate;

                innerImplementingClass.InnerAbstractClassAbstractProperty = 781;
                i = innerImplementingClass.InnerAbstractClassAbstractProperty;
                innerImplementingClass.InnerAbstractClassAbstractMethod();
                innerImplementingClass.InnerAbstractClassAbstractEventHappened += eventDelegate;
                innerImplementingClass.InnerAbstractClassAbstractEventHappened -= eventDelegate;

                innerImplementingClass.InnerAbstractClassInnerInterfaceProperty = 85;
                i = innerImplementingClass.InnerAbstractClassInnerInterfaceProperty;
                innerImplementingClass.InnerAbstractClassInnerInterfaceMethod();
                innerImplementingClass.InnerAbstractClassInnerInterfaceEventHappened += eventDelegate;
                innerImplementingClass.InnerAbstractClassInnerInterfaceEventHappened -= eventDelegate;

                innerImplementingClass.InnerInterfaceProperty = 69;
                i = innerImplementingClass.InnerInterfaceProperty;
                innerImplementingClass.InnerInterfaceMethod();
                innerImplementingClass.InnerInterfaceEventHappened += eventDelegate;
                innerImplementingClass.InnerInterfaceEventHappened -= eventDelegate;

                innerImplementingClass.InnerStructInnerInterfaceProperty = 15;
                i = innerImplementingClass.InnerStructInnerInterfaceProperty;
                innerImplementingClass.InnerStructInnerInterfaceMethod();
                innerImplementingClass.InnerStructInnerInterfaceEventHappened += eventDelegate;
                innerImplementingClass.InnerStructInnerInterfaceEventHappened -= eventDelegate;

                innerImplementingClass.InnerImplementingClassProperty = 48;
                i = innerImplementingClass.InnerImplementingClassProperty;
                innerImplementingClass.InnerImplementingClassMethod();
                innerImplementingClass.InnerImplementingClassEventHappened += eventDelegate;
                innerImplementingClass.InnerImplementingClassEventHappened -= eventDelegate;

                InnerImplementingClass.StaticInnerImplementingClassProperty = 13;
                i = InnerImplementingClass.StaticInnerImplementingClassProperty;
                InnerImplementingClass.StaticInnerImplementingClassMethod();
                InnerImplementingClass.StaticInnerImplementingClassEventHappened += eventDelegate;
                InnerImplementingClass.StaticInnerImplementingClassEventHappened -= eventDelegate;
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
            public override event EventHandler<InnerEventArgs> InnerAbstractClassAbstractEventHappened;
            protected virtual void OnInnerAbstractClassAbstractEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerAbstractClassAbstractEventHappened;
                eventHandler?.Invoke(this, e);
            }
            public event EventHandler<InnerEventArgs> InnerInterfaceEventHappened;
            protected virtual void OnInnerInterfaceEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerInterfaceEventHappened;
                eventHandler?.Invoke(this, e);
            }
            public event EventHandler<InnerEventArgs> InnerStructInnerInterfaceEventHappened;
            protected virtual void OnInnerStructInnerInterfaceEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerStructInnerInterfaceEventHappened;
                eventHandler?.Invoke(this, e);
            }
            public event EventHandler<InnerEventArgs> InnerAbstractClassInnerInterfaceEventHappened;
            protected virtual void OnInnerAbstractClassInnerInterfaceEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerAbstractClassInnerInterfaceEventHappened;
                if (eventHandler != null) { eventHandler(this, e); }
            }

            private int innerImplementingClassField;
            public int InnerImplementingClassProperty { get; set; }
            public void InnerImplementingClassMethod() { }
            public event EventHandler<InnerEventArgs> InnerImplementingClassEventHappened;
            protected virtual void OnInnerImplementingClassEventHappened(InnerEventArgs e)
            {
                var eventHandler = this.InnerImplementingClassEventHappened;
                eventHandler?.Invoke(this, e);
            }

            private static int staticInnerImplementingClassField;
            public static int StaticInnerImplementingClassProperty { get; set; }
            public static void StaticInnerImplementingClassMethod() { }
            public static event EventHandler<InnerEventArgs> StaticInnerImplementingClassEventHappened;
            protected static void OnStaticInnerImplementingClassEventHappened(InnerEventArgs e)
            {
                var eventHandler = StaticInnerImplementingClassEventHappened;
                if (eventHandler != null) { eventHandler(typeof(InnerImplementingClass), e); }
            }

            public void InnerImplementingClassReferenceAllMembers()
            {
                var events = 0;
                EventHandler<InnerEventArgs> eventDelegate = (object sender, InnerEventArgs e) => { ++events; };
                int i;

                SelfReferencingMembersMixin selfReferencingMembersMixin = new SelfReferencingMembersMixin();
                selfReferencingMembersMixin.Property = 4;
                selfReferencingMembersMixin.field = selfReferencingMembersMixin.Property;
                selfReferencingMembersMixin.Method();
                selfReferencingMembersMixin.EventHappened += eventDelegate;
                selfReferencingMembersMixin.OnEventHappened(new InnerEventArgs());
                selfReferencingMembersMixin.EventHappened -= eventDelegate;

                SelfReferencingMembersMixin.StaticProperty = 9;
                SelfReferencingMembersMixin.staticField = StaticProperty;
                SelfReferencingMembersMixin.StaticMethod();
                SelfReferencingMembersMixin.StaticEventHappened += eventDelegate;
                SelfReferencingMembersMixin.OnStaticEventHappened(new InnerEventArgs());
                SelfReferencingMembersMixin.StaticEventHappened -= eventDelegate;

                InnerStruct innerStruct = new InnerStruct();
                innerStruct.InnerStructProperty = 54;
                i = innerStruct.InnerStructProperty;
                innerStruct.InnerStructMethod();
                innerStruct.InnerStructEventHappened += eventDelegate;
                innerStruct.InnerStructEventHappened -= eventDelegate;
                InnerStruct.StaticInnerStructProperty = 58;
                i = InnerStruct.StaticInnerStructProperty;
                InnerStruct.StaticInnerStructMethod();
                InnerStruct.StaticInnerStructEventHappened += eventDelegate;
                InnerStruct.StaticInnerStructEventHappened -= eventDelegate;

                InnerAbstractClass.StaticInnerAbstractClassProperty = 32;
                InnerAbstractClass.StaticInnerAbstractClassMethod();
                InnerAbstractClass.StaticInnerAbstractClassEventHappened += eventDelegate;
                InnerAbstractClass.StaticInnerAbstractClassEventHappened -= eventDelegate;

                this.InnerAbstractClassProperty = 85;
                i = this.InnerAbstractClassProperty;
                this.InnerAbstractClassMethod();
                this.InnerAbstractClassEventHappened += eventDelegate;
                this.OnInnerAbstractClassEventHappened(new InnerEventArgs());
                this.InnerAbstractClassEventHappened -= eventDelegate;

                this.InnerAbstractClassAbstractProperty = 781;
                i = this.InnerAbstractClassAbstractProperty;
                this.InnerAbstractClassAbstractMethod();
                this.InnerAbstractClassAbstractEventHappened += eventDelegate;
                this.OnInnerAbstractClassAbstractEventHappened(new InnerEventArgs());
                this.InnerAbstractClassAbstractEventHappened -= eventDelegate;

                this.InnerAbstractClassInnerInterfaceProperty = 85;
                i = this.InnerAbstractClassInnerInterfaceProperty;
                this.InnerAbstractClassInnerInterfaceMethod();
                this.InnerAbstractClassInnerInterfaceEventHappened += eventDelegate;
                this.OnInnerAbstractClassInnerInterfaceEventHappened(new InnerEventArgs());
                this.InnerAbstractClassInnerInterfaceEventHappened -= eventDelegate;

                this.InnerInterfaceProperty = 69;
                i = this.InnerInterfaceProperty;
                this.InnerInterfaceMethod();
                this.InnerInterfaceEventHappened += eventDelegate;
                this.OnInnerInterfaceEventHappened(new InnerEventArgs());
                this.InnerInterfaceEventHappened -= eventDelegate;

                this.InnerStructInnerInterfaceProperty = 15;
                i = this.InnerStructInnerInterfaceProperty;
                this.InnerStructInnerInterfaceMethod();
                this.InnerStructInnerInterfaceEventHappened += eventDelegate;
                this.OnInnerStructInnerInterfaceEventHappened(new InnerEventArgs());
                this.InnerStructInnerInterfaceEventHappened -= eventDelegate;

                this.InnerImplementingClassProperty = 48;
                this.innerImplementingClassField = this.InnerImplementingClassProperty;
                this.InnerImplementingClassMethod();
                this.InnerImplementingClassEventHappened += eventDelegate;
                this.OnInnerImplementingClassEventHappened(new InnerEventArgs());
                this.InnerImplementingClassEventHappened -= eventDelegate;

                InnerImplementingClass.StaticInnerImplementingClassProperty = 13;
                InnerImplementingClass.staticInnerImplementingClassField = InnerImplementingClass.StaticInnerImplementingClassProperty;
                InnerImplementingClass.StaticInnerImplementingClassMethod();
                InnerImplementingClass.StaticInnerImplementingClassEventHappened += eventDelegate;
                InnerImplementingClass.OnStaticInnerImplementingClassEventHappened(new InnerEventArgs());
                InnerImplementingClass.StaticInnerImplementingClassEventHappened -= eventDelegate;
            }
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

        public void ReferenceAllTypesInClosedGenericVariables()
        {
#pragma warning disable 219
            Tuple<IInnerInterface, InnerEventArgs, InnerDelegate> tuple =
                new Tuple<IInnerInterface, InnerEventArgs, InnerDelegate>(new InnerImplementingClass(), new InnerEventArgs(), Console.WriteLine);
            List<InnerEnum> innerEnums = new List<InnerEnum>();
            InnerStruct[] innerStructs = new InnerStruct[10];
            Lazy<InnerStruct.InnerStructInnerEnum> innerStructInnerEnum = new Lazy<InnerStruct.InnerStructInnerEnum>();
            IEnumerable<InnerStruct.IInnerStructInnerInterface> innerStructInnerInterface = new List<InnerStruct.IInnerStructInnerInterface>();
            Dictionary<InnerStruct.InnerStructInnerStruct, InnerStruct.InnerStructInnerClass> innerStructInnerClassByInnerStructInnerStruct =
                new Dictionary<InnerStruct.InnerStructInnerStruct, InnerStruct.InnerStructInnerClass>();
            Func<InnerAbstractClass, InnerAbstractClass.InnerAbstractClassInnerEnum, InnerAbstractClass.IInnerAbstractClassInnerInterface> getInnerAbstractClass =
                (InnerAbstractClass @class, InnerAbstractClass.InnerAbstractClassInnerEnum @enum) => new InnerImplementingClass();
            List<Dictionary<InnerAbstractClass.InnerAbstractClassStruct, IEnumerable<InnerImplementingClass[]>>> stuff =
                new List<Dictionary<InnerAbstractClass.InnerAbstractClassStruct, IEnumerable<InnerImplementingClass[]>>>();
#pragma warning restore 219
        }

        public void ReferenceAllTypesInClosedGenericVariablesWithReferences()
        {
            Tuple<IInnerInterface, InnerEventArgs, InnerDelegate> tuple =
                new Tuple<IInnerInterface, InnerEventArgs, InnerDelegate>(new InnerImplementingClass(), new InnerEventArgs(), Console.WriteLine);
            List<InnerEnum> innerEnums = new List<InnerEnum>();
            InnerStruct[] innerStructs = new InnerStruct[10];
            Lazy<InnerStruct.InnerStructInnerEnum> innerStructInnerEnum = new Lazy<InnerStruct.InnerStructInnerEnum>();
            IEnumerable<InnerStruct.IInnerStructInnerInterface> innerStructInnerInterface = new List<InnerStruct.IInnerStructInnerInterface>();
            Dictionary<InnerStruct.InnerStructInnerStruct, InnerStruct.InnerStructInnerClass> innerStructInnerClassByInnerStructInnerStruct =
                new Dictionary<InnerStruct.InnerStructInnerStruct,InnerStruct.InnerStructInnerClass>();
            Func<InnerAbstractClass, InnerAbstractClass.InnerAbstractClassInnerEnum, InnerAbstractClass.IInnerAbstractClassInnerInterface> getInnerAbstractClass =
                (InnerAbstractClass @class, InnerAbstractClass.InnerAbstractClassInnerEnum @enum) => new InnerImplementingClass();
            List<Dictionary<InnerAbstractClass.InnerAbstractClassStruct, IEnumerable<InnerImplementingClass[]>>> stuff =
                new List<Dictionary<InnerAbstractClass.InnerAbstractClassStruct,IEnumerable<InnerImplementingClass[]>>>();

            string toStringHolder;

            toStringHolder = tuple.ToString();
            toStringHolder = tuple.Item1.ToString();
            toStringHolder = tuple.Item2.ToString();
            toStringHolder = tuple.Item3.ToString();
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

        public void CallGenericMethodsOnGenericClosedType(List<int> ints, List<IInnerInterface> innerInterfaces)
        {
            ints.ConvertAll<InnerStruct>(new Converter<int,InnerStruct>(i => new InnerStruct()));
            innerInterfaces.ConvertAll<InnerImplementingClass>(new Converter<IInnerInterface,InnerImplementingClass>(x => new InnerImplementingClass()));
        }

        public void ReferenceAllMembers()
        {
            var events = 0;
            EventHandler<InnerEventArgs> eventDelegate = (object sender, InnerEventArgs e) => { ++events; };
            int i;

            this.Property = 4;
            this.field = this.Property;
            this.Method();
            this.EventHappened += eventDelegate;
            this.OnEventHappened(new InnerEventArgs());
            this.EventHappened -= eventDelegate;

            StaticProperty = 9;
            staticField = StaticProperty;
            StaticMethod();
            StaticEventHappened += eventDelegate;
            OnStaticEventHappened(new InnerEventArgs());
            StaticEventHappened -= eventDelegate;

            InnerStruct innerStruct = new InnerStruct();
            innerStruct.InnerStructProperty = 54;
            i = innerStruct.InnerStructProperty;
            innerStruct.InnerStructMethod();
            innerStruct.InnerStructEventHappened += eventDelegate;
            innerStruct.InnerStructEventHappened -= eventDelegate;
            InnerStruct.StaticInnerStructProperty = 58;
            i = InnerStruct.StaticInnerStructProperty;
            InnerStruct.StaticInnerStructMethod();
            InnerStruct.StaticInnerStructEventHappened += eventDelegate;
            InnerStruct.StaticInnerStructEventHappened -= eventDelegate;

            InnerAbstractClass.StaticInnerAbstractClassProperty = 32;
            i = InnerAbstractClass.StaticInnerAbstractClassProperty;
            InnerAbstractClass.StaticInnerAbstractClassMethod();
            InnerAbstractClass.StaticInnerAbstractClassEventHappened += eventDelegate;
            InnerAbstractClass.StaticInnerAbstractClassEventHappened -= eventDelegate;

            InnerImplementingClass innerImplementingClass = new InnerImplementingClass();

            innerImplementingClass.InnerAbstractClassProperty = 85;
            i = innerImplementingClass.InnerAbstractClassProperty;
            innerImplementingClass.InnerAbstractClassMethod();
            innerImplementingClass.InnerAbstractClassEventHappened += eventDelegate;
            innerImplementingClass.InnerAbstractClassEventHappened -= eventDelegate;

            innerImplementingClass.InnerAbstractClassAbstractProperty = 781;
            i = innerImplementingClass.InnerAbstractClassAbstractProperty;
            innerImplementingClass.InnerAbstractClassAbstractMethod();
            innerImplementingClass.InnerAbstractClassAbstractEventHappened += eventDelegate;
            innerImplementingClass.InnerAbstractClassAbstractEventHappened -= eventDelegate;

            innerImplementingClass.InnerAbstractClassInnerInterfaceProperty = 85;
            i = innerImplementingClass.InnerAbstractClassInnerInterfaceProperty;
            innerImplementingClass.InnerAbstractClassInnerInterfaceMethod();
            innerImplementingClass.InnerAbstractClassInnerInterfaceEventHappened += eventDelegate;
            innerImplementingClass.InnerAbstractClassInnerInterfaceEventHappened -= eventDelegate;
            
            innerImplementingClass.InnerInterfaceProperty = 69;
            i = innerImplementingClass.InnerInterfaceProperty;
            innerImplementingClass.InnerInterfaceMethod();
            innerImplementingClass.InnerInterfaceEventHappened += eventDelegate;
            innerImplementingClass.InnerInterfaceEventHappened -= eventDelegate;
            
            innerImplementingClass.InnerStructInnerInterfaceProperty = 15;
            i = innerImplementingClass.InnerStructInnerInterfaceProperty;
            innerImplementingClass.InnerStructInnerInterfaceMethod();
            innerImplementingClass.InnerStructInnerInterfaceEventHappened += eventDelegate;
            innerImplementingClass.InnerStructInnerInterfaceEventHappened -= eventDelegate;

            innerImplementingClass.InnerImplementingClassProperty = 48;
            i = innerImplementingClass.InnerImplementingClassProperty;
            innerImplementingClass.InnerImplementingClassMethod();
            innerImplementingClass.InnerImplementingClassEventHappened += eventDelegate;
            innerImplementingClass.InnerImplementingClassEventHappened -= eventDelegate;

            InnerImplementingClass.StaticInnerImplementingClassProperty = 13;
            i = InnerImplementingClass.StaticInnerImplementingClassProperty;
            InnerImplementingClass.StaticInnerImplementingClassMethod();
            InnerImplementingClass.StaticInnerImplementingClassEventHappened += eventDelegate;
            InnerImplementingClass.StaticInnerImplementingClassEventHappened -= eventDelegate;
        }
    }
}
