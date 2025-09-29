using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ParameterModifier")]
    public enum ParameterModifier
    {
        None = 0,
        In = 1 << 0,
        Out = 1 << 1,
        Ref = 1 << 2,
        Params = 1 << 3,
        This = 1 << 4
    }

}
