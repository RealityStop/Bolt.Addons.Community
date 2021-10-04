using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMTransform
    {
        /// <summary>
        /// Structs for passing data down a transform operation.
        /// </summary>
        public partial class Data
        {
            /// <summary>
            /// Transform operation data for looking at a transform in some way.
            /// </summary>
            public struct Look
            {
                public Transform transform;

                public Look(Transform transform)
                {
                    this.transform = transform;
                }
            }

            /// <summary>
            /// Transform operation data for looking in a transforms direction..
            /// </summary>
            public struct In
            {
                public Look look;

                public In(Look look)
                {
                    this.look = look;
                }
            }

            /// <summary>
            /// Transform operation data for looking at a transform.
            /// </summary>
            public struct At
            {
                public Look look;

                public At(Look look)
                {
                    this.look = look;
                }
            }

            /// <summary>
            /// Transform operation data for moving a transform.
            /// </summary>
            public struct Move
            {
                public Transform transform;

                public Move(Transform transform)
                {
                    this.transform = transform;
                }
            }

            /// <summary>
            /// Transform operation data for moving a transform.
            /// </summary>
            public struct MoveRigidbody
            {
                public Rigidbody rigidbody;

                public MoveRigidbody(Rigidbody rigidbody)
                {
                    this.rigidbody = rigidbody;
                }
            }

            /// <summary>
            /// Transform operation data for straifing left and right with a transform.
            /// </summary>
            public struct Straif
            {
                public Move moveData;

                public Straif(Move moveData)
                {
                    this.moveData = moveData;
                }
            }

            /// <summary>
            /// Transform operation data for transform size operations.
            /// </summary>
            public struct Size
            {
                public Transform transform;

                public Size(Transform transform)
                {
                    this.transform = transform;
                }
            }

            /// <summary>
            /// Transform operation data for snapping a transforms position.
            /// </summary>
            public struct SnapPosition
            {
                public From from;
                public TransformSnapPosition position;
                public Vector3 centerOffset;

                public SnapPosition(From from, TransformSnapPosition position, Vector3 centerOffset)
                {
                    this.from = from;
                    this.position = position;
                    this.centerOffset = centerOffset;
                }
            }

            /// <summary>
            /// Transform operation data for doing a from transform operation.
            /// </summary>
            public struct From
            {
                public Transform transform;

                public From(Transform transform)
                {
                    this.transform = transform;
                }
            }

            /// <summary>
            /// Transform operation data for doing a snap to transform operation.
            /// </summary>
            public struct To
            {
                public SnapPosition positionData;

                public To(SnapPosition positionData)
                {
                    this.positionData = positionData;
                }
            }

            /// <summary>
            /// Transform operation data for snapping to a surface.
            /// </summary>
            public struct ObjectBounds
            {
                public Transform snapTo;
                public To to;
                public Vector3 snapToHalf;
                public Vector3 snapFromHalf;

                public ObjectBounds(Transform snapTo, To to, Vector3 snapFromHalf, Vector3 snapToHalf)
                {
                    this.snapTo = snapTo;
                    this.to = to;
                    this.snapFromHalf = snapFromHalf;
                    this.snapToHalf = snapToHalf;
                }
            }

            public enum TransformSnapPosition
            {
                Center,
                Left,
                Right,
                Top,
                Bottom,
                Front,
                Back
            }

            public enum TransformSnapToPosition
            {
                Left,
                Right,
                Top,
                Bottom,
                Front,
                Back
            }
        }
    }
}