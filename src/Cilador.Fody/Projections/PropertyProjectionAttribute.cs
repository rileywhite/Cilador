using System;

namespace Cilador.Fody.Projections
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class PropertyProjectionAttribute : ProjectionAttributeBase
    {
    }
}
