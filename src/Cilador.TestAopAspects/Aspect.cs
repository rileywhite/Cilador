using Cilador.Core;
using System;

namespace Cilador.TestAopAspects
{
    internal class Aspect : AspectBase
    {
        public Aspect() : base(new PointCut(), new MethodAdvice()) { }
    }
}
