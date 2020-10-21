using System;
using System.Collections.Generic;
using Ludiq;

namespace Bolt.Addons.Community.Utility
{
    /// <summary>
    /// An AOTDictuonary replacement that can be serialized and saved.
    /// </summary>
    [Serializable][Inspectable][IncludeInSettings(true)]
    [RenamedFrom("Lasm.BoltExtensions.IO.ObjectList")]
    [RenamedFrom("Lasm.BoltExtensions.ObjectList")]
    [RenamedFrom("Lasm.UAlive.ObjectList")]
    public sealed class ObjectList : List<object> { }
}

