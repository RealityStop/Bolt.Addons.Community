using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using System;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Utility
{
    public interface IAction : IDelegate
    {
        void Invoke(params object[] parameters);
        void Initialize(Flow flow, ActionUnit unit, Action flowAction);
        void Bind(object a, object b);
        void Unbind(object a, object b);
    }
}
