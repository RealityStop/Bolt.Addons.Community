using Bolt.Addons.Community.Fundamentals;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    public interface IFunc : IDelegate
    {
        Type ReturnType { get; }
        object Invoke(params object[] parameters);
        void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction);
    }
}
