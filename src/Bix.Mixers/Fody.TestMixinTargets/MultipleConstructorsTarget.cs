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

using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;

namespace Bix.Mixers.Fody.TestMixinTargets
{
    public class MultipleConstructorsTargetBase
    {
        public MultipleConstructorsTargetBase()
        {
            this.WhichConstructor = "First";
        }

        public MultipleConstructorsTargetBase(int i)
        {
            this.WhichConstructor = "Second";
        }

        public string WhichConstructor;
    }

    [InterfaceMixin(typeof(IForTargetWithMultipleConstructors))]
    public class MultipleConstructorsTarget : MultipleConstructorsTargetBase
    {
        public MultipleConstructorsTarget()
        {
            this.OriginalUninitializedInt = 783535;
            this.OriginalUninitializedString = "KNion wineofn oianweiof nqiognui ndf";
            this.OriginalUninitializedObject =  new OriginalInnerType { SomeNumber = 68485, SomeString = "AkolenaAeoi akefnoi ne" };
        }

        public MultipleConstructorsTarget(int i) : this(i, "A iuohiogfniouhe uihui iu.", new OriginalInnerType { SomeNumber = 89743, SomeString = "Alkewlkn io" }) { }

        public MultipleConstructorsTarget(int i, string j) : this(i, j, new OriginalInnerType { SomeNumber = 787465, SomeString = "LKEnoisnf sdsd" }) { }

        public MultipleConstructorsTarget(int i, string j, OriginalInnerType k)
            : base(i)
        {
            this.OriginalUninitializedInt = i;
            this.OriginalUninitializedString = j;
            this.OriginalUninitializedObject = k;
        }

        public class OriginalInnerType
        {
            public int SomeNumber { get; set; }
            public string SomeString { get; set; }
        }

        public int OriginalInitializedInt = 48685;
        public string OriginalInitializedString = "Tion3lao ehiuawh iuh buib ld";
        public OriginalInnerType OriginalInitializedObject = new OriginalInnerType { SomeNumber = 7834, SomeString = "aHWOoi Ooih noing" };

        public int OriginalUninitializedInt;
        public string OriginalUninitializedString;
        public OriginalInnerType OriginalUninitializedObject;
    }
}
