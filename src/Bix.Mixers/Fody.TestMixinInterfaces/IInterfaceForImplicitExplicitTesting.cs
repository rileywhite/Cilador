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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixinInterfaces
{
    public interface IInterfaceForImplicitExplicitTesting
    {
        string Method1();
        string Method2();
        string Method3();

        string Property1 { get; }
        string Property2 { get; }
        string Property3 { get; }

        event EventHandler Event1;
        event EventHandler Event2;
        event EventHandler Event3;
    }
}
