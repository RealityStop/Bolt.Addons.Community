using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMRaycast
    {
        /// <summary>
        /// Structs for passing data down a Raycasting operation.
        /// </summary>
        public static partial class Data
        {
            /// <summary>
            /// Raycast operation data for finding.
            /// </summary>
            public struct Find
            {
                public Vector3 origin;

                public Find(Vector3 origin)
                {
                    this.origin = origin;
                }
            }

            /// <summary>
            /// Raycast operation data for when its with something.
            /// </summary>
            public struct With
            {
                public Vector3 origin;

                public With(Vector3 origin)
                {
                    this.origin = origin;
                }
            }

            /// <summary>
            /// Raycast operation data for when there is multiple rays doing something.
            /// </summary>
            public struct Multiple
            {
                public Vector3 origin;

                public Multiple(Vector3 origin)
                {
                    this.origin = origin;
                }
            }

            /// <summary>
            /// Raycast operation data for when a ray is doing something.
            /// </summary>
            public struct Is
            {
                public Vector3 origin;

                public Is(Vector3 origin)
                {
                    this.origin = origin;
                }
            }

            /// <summary>
            /// Raycast operation data for when a ray is touching something.
            /// </summary>
            public struct Touching
            {
                public Vector3 origin;

                public Touching(Vector3 origin)
                {
                    this.origin = origin;
                }
            }
        }
    }
}