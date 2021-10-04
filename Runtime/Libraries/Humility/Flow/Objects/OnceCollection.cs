using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public sealed class AfterTicksCollection<TKey>
    {
        private Dictionary<TKey, int> counts = new Dictionary<TKey, int>();
        private Dictionary<TKey, object> values = new Dictionary<TKey, object>();

        public bool TryAdd(TKey key)
        {
            if (counts.ContainsKey(key))
            {
                return false;
            }

            counts.Add(key, 0);
            values.Add(key, null);
            return true;
        }

        public void OnValueChanged(TKey key, int ticks, bool reset, object value, Action action)
        {
            var count = counts[key];
            HUMFlow.AfterTicks(ref count, ticks, reset, () =>
            {
                if (values[key] != value)
                {
                    values[key] = value;
                    action?.Invoke();
                }
            });
            counts[key] = count;
        }
    }
}