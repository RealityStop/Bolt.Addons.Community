using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMRaycast
    {
        /// <summary>
        /// Begin an operation for when a point from a raycast is something.
        /// </summary>
        public static Data.Is Is(this Vector3 origin)
        {
            return new Data.Is(origin);
        }

        /// <summary>
        /// Begin an operation for when a point from a raycast has found something.
        /// </summary>
        public static Data.Find Find(this Vector3 origin)
        {
            return new Data.Find(origin);
        }
    }
}