using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath
    {
        /// <summary>
        /// Structs for passing data down a math operation.
        /// </summary>
        public static partial class Data
        {
            /// <summary>
            /// Math operation data for setting a value of a Vector3 directly.
            /// </summary>
            public struct SetVector3
            {
                public Vector3 vector;

                public SetVector3(Vector3 vector)
                {
                    this.vector = vector;
                }
            }

            /// <summary>
            /// Math operation data for setting a value of a Vector2 directly.
            /// </summary>
            public struct SetVector2
            {
                public Vector2 vector;

                public SetVector2(Vector2 vector)
                {
                    this.vector = vector;
                }
            }
        }
    }
}