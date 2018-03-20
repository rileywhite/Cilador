using System;

namespace Cilador.Aop.Advisors.WrapMethod
{
    internal static class Extensions
    {
        public static string ToCecilTypeName(this string source)
        {
            if(source == null) { return source; }
            return source.Replace('+', '/');
        }
    }
}
