using System;

namespace Unity.VisualScripting.Community 
{
    [Flags]
    public enum TypeParameterConstraints
    {
        None = 0,
        Class = 1 << 0,
        Struct = 1 << 1,
        New = 1 << 2
    }
}