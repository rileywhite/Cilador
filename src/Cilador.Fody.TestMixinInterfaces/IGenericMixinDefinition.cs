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

using System;

namespace Cilador.Fody.TestMixinInterfaces
{
    public interface IGenericMixinDefinition<T1, T2, T3, T4>
        where T1 : class, new()
        where T2 : struct, IGenericMixinDefinition<T1, T2, T3, T4>
        where T3 : T1, IDisposable
        where T4 : EventArgs
    {
        IGenericMixinDefinition<T1, T2, T3, T4> AsInterface(T2 arg0);
        void GenericMethod<T5>(T1 arg0, T4 arg1, T5 arg2);
        T3 SomeProperty { get; set; }
        int IntProperty { get; set; }
        event EventHandler<T4> Thinged;
    }
}
