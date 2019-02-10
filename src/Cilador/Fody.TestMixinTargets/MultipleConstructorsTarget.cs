/***************************************************************************/
// Copyright 2013-2019 Riley White
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

using Cilador.Fody.InterfaceMixins;
using Cilador.Fody.TestMixinInterfaces;
using System;

namespace Cilador.Fody.TestMixinTargets
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
            var values = Tuple.Create(783535, "KNion wineofn oianweiof nqiognui ndf", new UriBuilder { Host = "j.k.l" });

            this.OriginalUninitializedInt = values.Item1;
            this.OriginalUninitializedString = values.Item2;
            this.OriginalUninitializedObject =  values.Item3;
        }

        public MultipleConstructorsTarget(int i) : this(i, "A iuohiogfniouhe uihui iu.", new UriBuilder { Host = "g.h.i" }) { }

        public MultipleConstructorsTarget(int i, string j) : this(i, j, new UriBuilder { Host = "d.e.f" }) { }

        public MultipleConstructorsTarget(int i, string j, UriBuilder k)
            : base(i)
        {
            this.OriginalUninitializedInt = i;
            this.OriginalUninitializedString = j;
            this.OriginalUninitializedObject = k;
        }

        public int OriginalInitializedInt = 48685;
        public string OriginalInitializedString = "Tion3lao ehiuawh iuh buib ld";
        public UriBuilder OriginalInitializedObject = new UriBuilder { Host = "a.b.c" };

        public int OriginalUninitializedInt;
        public string OriginalUninitializedString;
        public UriBuilder OriginalUninitializedObject;
    }
}
