using Bolt.Addons.Community.Fundamentals.Units.logic;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    public interface IFunc : IDelegate
    {
        Type ReturnType { get; }
        object Invoke(params object[] parameters);
        void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction);
        void Bind(object a, object b);
        void Unbind(object a, object b);
    }
}
