using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEvents
    {
        /// <summary>
        /// The amount of clicks in a row the mouse accumulated.
        /// </summary>
        internal static int clickCount = 0;

        /// <summary>
        /// Starts a mouse operation.
        /// </summary>
        public static Data.Mouse Mouse(this Event e)
        {
            return new Data.Mouse(e);
        }

        public static Vector2 mouseDelta;
        public static Vector2 mousePosition;
    }
}
