using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class GenericsMixin : IEmptyInterface
    {
        [Skip]
        public GenericsMixin() { }

        public void VoidGenericMethodWithNoArguments<T>() { }
        public void VoidGenericMethodWithGenericArgument<T>(T arg0) { }
        public void VoidGenericMethodWithGenericAndNonGenericArguments<T>(T arg0, int arg1, object arg2, T arg3) { }
        public T NonVoidGenericMethodWithNoArguments<T>() { return default(T); }
        public T NonVoidGenericMethodWithGenericArgument<T>(T arg0) { return arg0; }
        public T NonVoidGenericMethodWithGenericAndNonGenericArguments<T>(T arg0, int arg1, object arg2, T arg3) { return arg3; }

        public void VoidMultipleGenericMethodWithNoArguments<T0, T1, T2, T3, T4>() { }
        public void VoidGenericMethodWithGenericArguments<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) { }
        public T3 NonVoidMultipleGenericMethodWithNoArguments<T0, T1, T2, T3, T4>() { return default(T3); }
        public T3 NonVoidMultipleGenericMethodWithGenericArguments<T0, T1, T2, T3, T4>() { return default(T3); }
        public T3 NonVoidMultipleGenericMethodWithGenericAndNonGenericArguments<T0, T1, T2, T3, T4>(T0 arg0, int arg1, object arg2, T3 arg3) { return arg3; }

        public void GenericWithConstraintsMethod<T>(T arg0)
            where T : class, IDisposable, IList<DictionaryEntry>, new() { }

        public T3 MultipleGenericWithConstraintsMethod<T0, T1, T2, T3, T4>(T0 arg0, int arg1, object arg2, T3 arg3)
            where T0 : class
            where T2 : struct
            where T3 : List<int>, IDisposable, new()
            where T4 : new() { return arg3; }

        public Tuple<T0, List<KeyValuePair<string, T2>>, T4, T3, T1[], T4, T4> MultipleGenericWithSelfReferencingConstraintsMethod<T0, T1, T2, T3, T4>(T0 arg0, int arg1, object arg2, T3 arg3, T4 arg4)
            where T0 : class
            where T1 : T2
            where T2 : IEnumerable<byte>
            where T3 : List<T1>, IDisposable, new()
            where T4 : T3, new() { return Tuple.Create(arg0, new List<KeyValuePair<string, T2>>(), arg4, arg3, new T1[0], arg4, arg4); }
    }
}
