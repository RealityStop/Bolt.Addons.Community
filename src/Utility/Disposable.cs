using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolt.Addons.Community.Utility
{
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