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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Description("This is an assembly attribute")]
[module: Description("This is a module attribute")]

namespace Bix.Mixers.Fody.TestMixinTargets
{
    [Description("NonTargetType")]
    public class NonTargetType
    {
        [Description(".cctor")]
        static NonTargetType() { }

        [Description(".ctor")]
        public NonTargetType() { }

        [Description(".ctor")]
        public NonTargetType([Description("arg0")] int arg0) { }

        [Description("Field1")]
        public int Field1;

        [Description("Field2")]
        public int Field2;

        [Description("Field3")]
        public int Field3;

        [Description("Method1")]
        [return: Description("return")]
        public int Method1([Description("arg0")] int arg0) { return 0; }

        [Description("Method2")]
        [return: Description("return")]
        public int Method2([Description("arg0")] int arg0) { return 0; }

        [Description("Method3")]
        [return: Description("return")]
        public int Method3([Description("arg0")] int arg0) { return 0; }

        [Description("Property1")]
        public int Property1 { [Description("get_Property1")] [return: Description("return")] get { return 0; } [Description("set_Property1")] [return: Description("return")] [param: Description("value")] set { } }

        [Description("Property2")]
        public int Property2 { [Description("get_Property2")] [return: Description("return")] get { return 0; } [Description("set_Property2")] [return: Description("return")] [param: Description("value")] set { } }

        [Description("Property3")]
        public int Property3 { [Description("get_Property3")] [return: Description("return")] get { return 0; } [Description("set_Property3")] [return: Description("return")] [param: Description("value")] set { } }

        [Description("Event1")]
        public event EventHandler Event1 { [Description("add_Event1")] [return: Description("return")] [param: Description("value")] add { } [Description("remove_Event1")] [return: Description("return")] [param: Description("value")] remove { } }

        [Description("Event2")]
        public event EventHandler Event2 { [Description("add_Event2")] [return: Description("return")] [param: Description("value")] add { } [Description("remove_Event2")] [return: Description("return")] [param: Description("value")] remove { } }

        [Description("Event3")]
        public event EventHandler Event3 { [Description("add_Event3")] [return: Description("return")] [param: Description("value")] add { } [Description("remove_Event3")] [return: Description("return")] [param: Description("value")] remove { } }

        [Description("InnerClass1")]
        public class InnerClass1 { }

        [Description("InnerClass2")]
        public class InnerClass2 { }

        [Description("InnerClass3")]
        public class InnerClass3 { }

        [Description("InnerInterface1")]
        public interface InnerInterface1 { }

        [Description("InnerInterface2")]
        public interface InnerInterface2 { }

        [Description("InnerInterface3")]
        public interface InnerInterface3 { }

        [Description("InnerStruct1")]
        public struct InnerStruct1 { }

        [Description("InnerStruct2")]
        public struct InnerStruct2 { }

        [Description("InnerStruct3")]
        public struct InnerStruct3 { }

        [Description("Delegate1")]
        public delegate int Delegate1();

        [Description("Delegate2")]
        public delegate int Delegate2();

        [Description("Delegate3")]
        public delegate int Delegate3();

        [Description("Enum1")]
        public enum Enum1 { }

        [Description("Enum2")]
        public enum Enum2 { }

        [Description("Enum3")]
        public enum Enum3 { }
    }
}
