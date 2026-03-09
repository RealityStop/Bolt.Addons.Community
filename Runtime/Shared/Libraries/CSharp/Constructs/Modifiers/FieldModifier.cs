using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.FieldModifier")]
    [Flags]
    public enum FieldModifier
    {
        None = 0,
        Constant = 1 << 0,
        Static = 1 << 1,
        Unsafe = 1 << 2,
        Volatile = 1 << 3,
        Readonly = 1 << 4,
        New = 1 << 5,
    }
}
