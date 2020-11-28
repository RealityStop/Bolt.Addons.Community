using Bolt.Addons.Community.Utility;
using System;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Utility
{
    public interface IAction
    {
        void Invoke(params object[] parameters);
        TypeParam[] parameters { get; }
        object GetAction();
        Type GetActionType();
        void Initialize(Action flow);
        void SetAction(object action);
    }
}
