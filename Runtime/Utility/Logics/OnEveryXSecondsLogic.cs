using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class OnEveryXSecondsLogic
    {
        private float elapsedTime;

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
