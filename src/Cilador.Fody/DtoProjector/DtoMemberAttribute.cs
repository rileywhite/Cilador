using System;

namespace Cilador.Fody.Projector
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class DtoMemberAttribute : Attribute { }
}
