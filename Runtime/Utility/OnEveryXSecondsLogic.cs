using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class OnEveryXSecondsLogic
    {
        private float elapsedTime;

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="threshold">The number of seconds required before triggering.</param>
        /// <param name="unscaled">Whether to ignore the time scale.</param>
        /// <returns>True if the threshold was reached this frame, false otherwise.</returns>
        public bool Update(float threshold, bool unscaled = false)
        {
            elapsedTime += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

            if (elapsedTime >= threshold)
            {
                elapsedTime = 0f;
                return true;
            }

            return false;
        }
    }
}
