using System;

namespace Cilador.Fody.Projections
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class DtoMemberAttribute : Attribute { }
}
