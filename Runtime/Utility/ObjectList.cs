using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Utility
{
    /// <summary>
    /// An AOTDictuonary replacement that can be serialized and saved.
    /// </summary>
    [Serializable][Inspectable][IncludeInSettings(true)]
    [RenamedFrom("Bolt.Addons.Community.Utility.ObjectList")]
    public sealed class ObjectList : List<object> { }
}

