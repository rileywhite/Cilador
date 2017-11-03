using Cilador.Core;
using System;

namespace Cilador.TestAopAspects
{
    internal class MethodAdvice : MethodAdviceBase
    {
        protected override void OnBefore()
        {
            Console.WriteLine("Before...");
        }

        protected override void OnAfter()
        {
            Console.WriteLine("...After");
        }
    }
}
