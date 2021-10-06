using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMGameObject_Children
    {
        /// <summary>
        /// Removes the '(Clone)' text in the name that happens when cloned or instantiated.
        /// </summary>
        public static GameObject Clone(this HUMGameObject.Data.Remove remove)
        {
            remove.target.name = remove.target.name.Replace("(Clone)", string.Empty);
            return remove.target;
        }
    }
}