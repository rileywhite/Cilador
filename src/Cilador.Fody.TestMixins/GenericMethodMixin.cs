/***************************************************************************/
// Copyright 2013-2017 Riley White
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
    public class GenericMethodMixin : IEmptyInterface
    {
        public T GenericMethod<T>(T arg) { return arg; }

        public Tuple<TClass, TStruct, TNew, TClassNew, TDisposable, TTClass> GenericMethodWithConstraints<TClass, TStruct, TNew, TClassNew, TDisposable, TTClass>(
            TClass tClass,
            TStruct tStruct,
            TNew tNew,
            TClassNew tClassNew,
            TDisposable tDisposable,
            TTClass ttClass)
            where TClass : class
            where TStruct : struct
            where TNew : new()
            where TClassNew : class, new()
            where TDisposable : IDisposable
            where TTClass : TClass
        {
            return Tuple.Create(tClass, tStruct, tNew, tClassNew, tDisposable, ttClass);
        }
    }
}
