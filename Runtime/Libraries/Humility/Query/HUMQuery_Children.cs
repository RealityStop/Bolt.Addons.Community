using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMQuery
    {
        /// <summary>
        /// Performs a foreach loop with an action.
        /// </summary>
        public static IEnumerable<T> Each<T>(this Data.For<T> forData, System.Action<T> action)
        {
            foreach (T obj in forData.collection)
            {
                action(obj);
            }

            return forData.collection;
        }
    }
}