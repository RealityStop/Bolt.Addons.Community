using System;

namespace Unity.VisualScripting.Community.Utility
{
    [RenamedFrom("Bolt.Addons.Community.Utility.Disposable")]
    public class Disposable : IDisposable
    {
        public static IDisposable Create(Action action)
        {
            return new Disposable(action);
        }

        public static IDisposable Empty = new Disposable(null);

        Disposable(Action action)
        {
            _action = action;
        }

        void IDisposable.Dispose()
        {
            if (_action != null)
            {
                _action();
                _action = null;
            }
        }

        private Action _action;
    }
}