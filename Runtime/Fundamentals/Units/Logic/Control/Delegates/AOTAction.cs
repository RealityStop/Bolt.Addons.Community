using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;

namespace Bolt.Addons.Community.Utility
{
    [IncludeInSettings(true)]
    public sealed class AOTAction : IAction
    {
        public Action action;
        public object GetAction() => action;
        public void SetAction(object action) => this.action = action as Action;
        public Type GetActionType() => typeof(Action);
        public TypeParam[] parameters => new TypeParam[] { };

        public void Invoke(params object[] parameters)
        {
            action.Invoke();
        }

        public void Initialize(Action flow)
        {
            action = () => { flow.Invoke(); };
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTAction<T> : IAction
    {
        public Action<T> action;
        public object GetAction() => action;
        public void SetAction(object action) => this.action = action as Action<T>;
        public Type GetActionType() => typeof(Action<T>);

        public abstract TypeParam[] parameters { get; }

        public void Invoke(params object[] parameters)
        {
            action.Invoke((T)parameters[0]);
        }

        public void Initialize(Action flow)
        {
            action = (t) => { flow.Invoke(); };
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTAction<T1, T2> : IAction
    {
        internal Action<T1, T2> action;
        public object GetAction() => action;
        public void SetAction(object action) => this.action = action as Action<T1, T2>;
        public Type GetActionType() => typeof(Action<T1, T2>);

        public abstract TypeParam[] parameters { get; }

        public void Invoke(params object[] parameters)
        {
            action.Invoke((T1)parameters[0], (T2)parameters[1]);
        }

        public void Initialize(Action flow)
        {
            action = (t1, t2) => { flow.Invoke(); };
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTAction<T1, T2, T3> : IAction
    {
        public Action<T1, T2, T3> action;
        public object GetAction() => action;
        public void SetAction(object action) => this.action = action as Action<T1, T2, T3>;
        public Type GetActionType() => typeof(Action<T1, T2, T3>);

        public abstract TypeParam[] parameters { get; }

        public void Invoke(params object[] parameters)
        {
            action.Invoke((T1)parameters[0], (T2)parameters[1], (T3)parameters[3]);
        }

        public void Initialize(Action flow)
        {
            action = (t1, t2, t3) => { flow.Invoke(); };
        }
    }

    [IncludeInSettings(true)]
    public abstract class AOTAction<T1, T2, T3, T4> : IAction
    {
        public Action<T1, T2, T3, T4> action;
        public object GetAction() => action;
        public void SetAction(object action) => this.action = action as Action<T1, T2, T3, T4>;
        public Type GetActionType() => typeof(Action<T1, T2, T3, T4>);

        public abstract TypeParam[] parameters { get; }

        public void Invoke(params object[] parameters)
        {
            action.Invoke((T1)parameters[0], (T2)parameters[1], (T3)parameters[3], (T4)parameters[4]);
        }

        public void Initialize(Action flow)
        {
            action = (t1, t2, t3, t4) => { flow.Invoke(); };
        }
    }
}
