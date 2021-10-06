using System;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public sealed class Once
    {
        private bool canDo = true;
        public void Do(Action action)
        {
            if (canDo)
            {
                action?.Invoke();
                canDo = false;
            }
        }

        public void Reset()
        {
            canDo = true;
        }
    }
}