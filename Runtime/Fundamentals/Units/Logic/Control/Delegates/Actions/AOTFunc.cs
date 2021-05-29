using System;
using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;

namespace Bolt.Addons.Community.Utility
{
    [IncludeInSettings(true)]
    public sealed class AOTFunc : AOTFunc<object>
    {

    }

    [IncludeInSettings(true)]
    public abstract class AOTFunc<T> : IFunc
    {
        public Func<T> func;
        public object GetDelegate() => func;
        public Type GetDelegateType() => typeof(Func<T>);

        public TypeParam[] parameters => new TypeParam[] { };

        public Type ReturnType => typeof(T);

        public object Invoke(params object[] parameters)
        {
            return func.Invoke();
        }

        public void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction)
        {
            func = () =>
            {
                unit.AssignParameters(flow);
                return (T)flowAction.Invoke();
            };
        }

        public void Bind(object a, object b)
        {
            var _action = a as Func<T>;
            _action += ((Func<T>)b);
        }

        public void Unbind(object a, object b)
        {
            var _action = a as Func<T>;
            _action -= ((Func<T>)b);
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTFunc<T, T1> : IFunc
    {
        public Func<T, T1> func;
        public object GetDelegate() => func;
        public Type GetDelegateType() => typeof(Func<T, T1>);

        public abstract TypeParam[] parameters { get; }

        public Type ReturnType => typeof(T1);

        public object Invoke(params object[] parameters)
        {
            return func.Invoke((T)parameters[0]);
        }

        public void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction)
        {
            func = (t) =>
            {
                unit.AssignParameters(flow, t);
                return (T1)flowAction.Invoke();
            };
        }

        public void Bind(object a, object b)
        {
            var _action = a as Func<T, T1>;
            _action += ((Func<T, T1>)b);
        }

        public void Unbind(object a, object b)
        {
            var _action = a as Func<T, T1>;
            _action -= ((Func<T, T1>)b);
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTFunc<T, T1, T2> : IFunc
    {
        public Func<T, T1> func;
        public object GetDelegate() => func;
        public Type GetDelegateType() => typeof(Func<T, T1>);

        public abstract TypeParam[] parameters { get; }

        public Type ReturnType => typeof(T1);

        public object Invoke(params object[] parameters)
        {
            return func.Invoke((T)parameters[0]);
        }

        public void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction)
        {
            func = (t) =>
            {
                unit.AssignParameters(flow, t);
                return (T1)flowAction.Invoke();
            };
        }

        public void Bind(object a, object b)
        {
            var _action = a as Func<T, T1, T2>;
            _action += ((Func<T, T1, T2>)b);
        }

        public void Unbind(object a, object b)
        {
            var _action = a as Func<T, T1, T2>;
            _action -= ((Func<T, T1, T2>)b);
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTFunc<T, T1, T2, T3> : IFunc
    {
        public Func<T, T1, T2, T3> func;
        public object GetDelegate() => func;
        public Type GetDelegateType() => typeof(Func<T, T1, T2, T3>);

        public abstract TypeParam[] parameters { get; }

        public Type ReturnType => typeof(T3);

        public object Invoke(params object[] parameters)
        {
            return func.Invoke((T)parameters[0], (T1)parameters[1], (T2)parameters[2]);
        }

        public void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction)
        {
            func = (t, t1, t2) =>
            {
                unit.AssignParameters(flow, t);
                return (T3)flowAction.Invoke();
            };
        }

        public void Bind(object a, object b)
        {
            var _action = a as Func<T, T1, T2, T3>;
            _action += ((Func<T, T1, T2, T3>)b);
        }

        public void Unbind(object a, object b)
        {
            var _action = a as Func<T, T1, T2, T3>;
            _action -= ((Func<T, T1, T2, T3>)b);
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTFunc<T, T1, T2, T3, T4> : IFunc
    {
        public Func<T, T1, T2, T3, T4> func;
        public object GetDelegate() => func;
        public Type GetDelegateType() => typeof(Func<T, T1, T2, T3, T4>);

        public abstract TypeParam[] parameters { get; }

        public Type ReturnType => typeof(T4);

        public object Invoke(params object[] parameters)
        {
            return func.Invoke((T)parameters[0], (T1)parameters[1], (T2)parameters[2], (T3)parameters[3]);
        }

        public void Initialize(Flow flow, FuncUnit unit, Func<object> flowAction)
        {
            func = (t, t1, t2, t3) =>
            {
                unit.AssignParameters(flow, t);
                return (T4)flowAction.Invoke();
            };
        }

        public void Bind(object a, object b)
        {
            var _action = a as Func<T, T1, T2, T3, T4>;
            _action += ((Func<T, T1, T2, T3, T4>)b);
        }

        public void Unbind(object a, object b)
        {
            var _action = a as Func<T, T1, T2, T3, T4>;
            _action -= ((Func<T, T1, T2, T3, T4>)b);
        }
    }
}
