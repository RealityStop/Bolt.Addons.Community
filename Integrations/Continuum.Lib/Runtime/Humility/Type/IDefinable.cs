using System;

namespace Bolt.Addons.Integrations.Continuum
{
    public interface IDefinable
    {
        bool changed { get; }
        event Action definitionChanged;
        void Define();
    }
}