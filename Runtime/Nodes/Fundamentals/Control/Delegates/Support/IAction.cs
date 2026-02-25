using System;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Utility.IAction")]
    public interface IAction : IDelegate
    {
        void Invoke(params object[] parameters);
        void Initialize(Flow flow, ActionNode unit, Action flowAction);
        void SetInstance(Flow flow, ActionNode unit, Action flowAction);
    }
}
