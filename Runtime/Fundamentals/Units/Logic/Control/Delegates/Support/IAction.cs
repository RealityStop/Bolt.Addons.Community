using Bolt.Addons.Community.Fundamentals;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    public interface IAction : IDelegate
    {
        void Invoke(params object[] parameters);
        void Initialize(Flow flow, ActionUnit unit, Action flowAction);
    }
}
