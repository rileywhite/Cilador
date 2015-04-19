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

using Cilador.Fody.InterfaceMixins;
using Cilador.Fody.TestMixinInterfaces;
using System;

namespace Cilador.Fody.TestMixinTargets
{
    using Cilador.Fody.InterfaceMixins;
    using Cilador.Fody.TestMixinInterfaces;

    public class StaticConstructorsTargetBase
    {
        public static int SomeNumberSetTo56561InStaticConstructor;
        public static int SomeNumberInitializedTo8188 = 8188;

        static StaticConstructorsTargetBase()
        {
            SomeNumberSetTo56561InStaticConstructor = 56561;
        }
    }

    [InterfaceMixin(typeof(IForTargetWithStaticConstructors))]
    public class StaticConstructorsTarget : StaticConstructorsTargetBase
    {
        static StaticConstructorsTarget()
        {
            OriginalUninitializedInt = 9834897;
            OriginalUninitializedString = "QWEhinIOnsonf nui uif";
            OriginalUninitializedObject = new UriBuilder { Host = "d.e.f" };
        }

        public static int OriginalInitializedInt = 35881;
        public static string OriginalInitializedString = "Elak Ion fiugrnn";
        public static UriBuilder OriginalInitializedObject = new UriBuilder { Host = "a.b.c" };

        public static int OriginalUninitializedInt;
        public static string OriginalUninitializedString;
        public static UriBuilder OriginalUninitializedObject;
    }
}
