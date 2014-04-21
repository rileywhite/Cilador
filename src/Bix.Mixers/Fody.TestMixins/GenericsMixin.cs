using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
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

        public void VoidGenericWithConstraintsMethodWithNoArguments<T>()
            where T : class, IDisposable, new() { }
    }
}
