using System;

namespace Bolt.Addons.Libraries.CSharp
{
    public interface IDefinable
    {
        bool changed { get; }
        event Action definitionChanged;
        void Define();
    }
}