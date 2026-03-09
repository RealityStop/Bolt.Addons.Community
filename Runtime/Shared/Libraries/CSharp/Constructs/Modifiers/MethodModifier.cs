using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [System.Flags]
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.MethodModifier")]
    public enum MethodModifier
    {
        None = 0,
        Abstract = 1 << 0,
        Async = 1 << 1,
        Extern = 1 << 2,
        Override = 1 << 3,
        Sealed = 1 << 4,
        Static = 1 << 5,
        Unsafe = 1 << 6,
        Virtual = 1 << 7
    }
}
