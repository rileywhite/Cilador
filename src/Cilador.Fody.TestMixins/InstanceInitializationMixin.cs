/***************************************************************************/
// Copyright 2013-2016 Riley White
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

namespace Cilador.Fody.TestMixins
{
    public class InstanceInitializationMixin : IForTargetWithMultipleConstructors
    {
        public int SomeNumber = 684865;
        public string SomeString = "Tawhlej oisahoeh 8ohf 4ifh8ohe fni dlgj";
        public object SomeObject = new object();
        public InnerType SomeInnerType = new InnerType { SomeInt = 4235, SomeString = "JLKOIN  oin aon oingori d", SomeObject = new object() };
        public Func<int, string, object, Tuple<int, string, object>> SomeFunc = InnerType.SomeMethod;
        public SomeMethodDelegate SomeMethodDelegateInstance = InnerType.SomeMethod;

        public int SomeNumberSetTo395493InConstructor;

        public InstanceInitializationMixin()
        {
            this.SomeNumberSetTo395493InConstructor = 395493;
        }

        public delegate Tuple<int, string, object> SomeMethodDelegate(int i, string j, object k);
        
        public class InnerType
        {
            public static Tuple<int, string, object> SomeMethod(int i, string j, object k)
            {
                return Tuple.Create(i, j, k);
            }

            public int SomeInt { get; set; }
            public string SomeString { get; set; }
            public object SomeObject { get; set; }
        }
    }
}
