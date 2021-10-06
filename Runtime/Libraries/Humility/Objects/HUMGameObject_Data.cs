using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMGameObject
    {
        public static partial class Data
        {
            public struct Remove
            {
                public GameObject target;

                public Remove(GameObject target)
                {
                    this.target = target;
                }
            }
        }
    }
}