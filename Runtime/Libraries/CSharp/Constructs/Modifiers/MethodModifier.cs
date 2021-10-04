using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.MethodModifier")]
    public enum MethodModifier
    {
        None,
        Abstract,
        Async,
        Extern,
        Override,
        Sealed,
        Static,
        Unsafe,
        Virtual
    }
}
