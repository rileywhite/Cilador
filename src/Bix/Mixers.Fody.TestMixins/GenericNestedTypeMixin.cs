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

using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class GenericNestedTypeMixin : IEmptyInterface
    {
        public class GenericType<T>
        {
            public T GetThing(T thing) { return thing; }
        }

        public class GenericTypeWithConstraints<TClass, TStruct, TNew, TClassNew, TDisposable, TTClass>
            where TClass : class
            where TStruct : struct
            where TNew : new()
            where TClassNew : class, new()
            where TDisposable : IDisposable
            where TTClass : TClass
        {
            public Tuple<TClass, TStruct, TNew, TClassNew, TDisposable, TTClass> GetThings(
                TClass tClass,
                TStruct tStruct,
                TNew tNew,
                TClassNew tClassNew,
                TDisposable tDisposable,
                TTClass ttClass)
            {
                return Tuple.Create(tClass, tStruct, tNew, tClassNew, tDisposable, ttClass);
            }
        }

        public class GenericTypeWithGenericMethod<TType>
        {
            public Tuple<TType, TMethod> GetThings<TMethod>(TType typeThing, TMethod methodThing) { return Tuple.Create(typeThing, methodThing); }
        }

        public class GenericTypeWithMultipleParameters<T1, T2>
        {
            public T1 Thing1 { get; set; }

            public T2 Thing2 { get; set; }
        }

        public class PartiallyClosedGenericType<T3> : GenericTypeWithMultipleParameters<int, T3>
        {
        }

        public class TypeWithPartiallyClosedGenericMethod
        {
            public GenericTypeWithMultipleParameters<int, T4> GetThing<T4>(T4 innerThing)
            {
                return new GenericTypeWithMultipleParameters<int, T4> { Thing1 = 297387, Thing2 = innerThing };
            }

            public GenericTypeWithMultipleParameters<T5, int> GetOtherThing<T5>(T5 innerThing)
            {
                return new GenericTypeWithMultipleParameters<T5, int> { Thing1 = innerThing, Thing2 = 789437 };
            }
        }
    }
}
