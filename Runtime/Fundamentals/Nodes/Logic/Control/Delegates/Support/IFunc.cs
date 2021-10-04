using System;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Utility.IFunc")]
    public interface IFunc : IDelegate
    {
        Type ReturnType { get; }
        object DynamicInvoke(params object[] parameters);
        void Initialize(Flow flow, FuncNode unit, Func<object> flowAction);
        void SetInstance(Flow flow, FuncNode unit, Func<object> flowAction);
    }
}
