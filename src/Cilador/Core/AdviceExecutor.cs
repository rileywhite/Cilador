using System;

namespace Cilador.Core
{
    internal class AdviceExecutor
    {
        public void Execute<T>(ActionAdvice<T> advice, Action<T> action, T arg)
        {
            advice.Invoke(action, arg);
        }
    }
}
