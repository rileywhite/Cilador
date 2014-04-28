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
    public class ImplicitExplicitTestingImplicitOnlyMixin : IInterfaceForImplicitExplicitTesting
    {
        public string Method1() { return "Implicit Method 1"; }
        public string Method2() { return "Implicit Method 2"; }
        public string Method3() { return "Implicit Method 3"; }

        public string Property1 { get { return "Implicit Property 1"; } }
        public string Property2 { get { return "Implicit Property 2"; } }
        public string Property3 { get { return "Implicit Property 3"; } }

#pragma warning disable 67
        public event EventHandler Event1;
        public event EventHandler Event2;
        public event EventHandler Event3;
#pragma warning restore 67
    }
}
