using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    public interface IDefinable
    {
        bool changed { get; }
        event Action definitionChanged;
        void Define();
    }
}