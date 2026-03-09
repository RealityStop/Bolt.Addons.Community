using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.PropertyModifier")]
    [Flags]
    public enum PropertyModifier
    {
        None = 0,
        Abstract = 1 << 0,
        Override = 1 << 1,
        Sealed = 1 << 2,
        Static = 1 << 3,
        Unsafe = 1 << 4,
        Volatile = 1 << 5,
        New = 1 << 6,
    }
}
