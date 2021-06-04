using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    public interface IAction : IDelegate
    {
        void Invoke(params object[] parameters);
        void Initialize(Flow flow, ActionUnit unit, Action flowAction);
    }
}
