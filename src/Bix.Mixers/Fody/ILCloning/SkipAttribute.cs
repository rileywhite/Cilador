using System;

namespace Bix.Mixers.Fody.ILCloning
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Interface |
        AttributeTargets.Enum |
        AttributeTargets.Constructor |
        AttributeTargets.Method |
        AttributeTargets.Property |
        AttributeTargets.Field |
        AttributeTargets.Delegate |
        AttributeTargets.Event,
        AllowMultiple = false,
        Inherited = false)]
    internal sealed class SkipAttribute : Attribute
    {
    }
}
