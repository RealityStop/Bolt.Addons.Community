using System;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMFlow
    {
        /// <summary>
        /// Performs an action after this method has occurred the correct amount of times.
        /// </summary>
        /// <param name="counter">A field reference to an integer that acts as the counters current value.</param>
        /// <param name="requiredTicks">The amount the counter must reach to start triggerin the action.</param>
        /// <param name="autoReset">After the first time the action successfully occurs, should we reset back to zero?</param>
        /// <param name="afterTicks">The assigned action to invoke once the counter has reached its required ticks.</param>
        public static void AfterTicks(ref int counter, int requiredTicks, bool autoReset = false, Action afterTicks = null)
        {
            if (counter >= requiredTicks)
            {
                afterTicks?.Invoke();
                if (autoReset) counter = -1;
            }

            counter++;
        }
    }
}