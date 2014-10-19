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

using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class StaticInitializationMixin : IEmptyInterface
    {
        public static int StaticNumberSetTo832778InInitializer;
        public static int StaticNumberInitilizedTo7279848InDeclaration = 7279848;
        public static Tuple<int, string> StaticTupleInitilizedTo485AndBlahInDeclaration = Tuple.Create(485, "Blah");
        public static InnerType InnerTypeSetTo49874AndBlah2AndNewObjectWithObjectInitilizerInDelcaration =
            new InnerType { SomeInt = 49874, SomeString = "Blah2", SomeObject = new object() };

        //static StaticInitializationMixin()
        //{
        //    StaticNumberSetTo832778InInitializer = 832778;
        //}

        public class InnerType
        {
            public int SomeInt { get; set; }
            public string SomeString { get; set; }
            public object SomeObject { get; set; }
        }
    }
}
