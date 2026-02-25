using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public partial class HUMTransform
    {
        /// <summary>
        /// Get the type of time based on an enum.
        /// </summary>
        internal static float GetTime(this TimeType timeType)
        {
            switch (timeType)
            {
                case TimeType.Time:
                    return Time.time;
                case TimeType.Delta:
                    return Time.deltaTime;
                case TimeType.Fixed:
                    return Time.fixedDeltaTime;
                case TimeType.UnscaledDelta:
                    return Time.unscaledDeltaTime;
                case TimeType.UnscaledFixed:
                    return Time.fixedUnscaledDeltaTime;
                case TimeType.UnscaledTime:
                    return Time.unscaledTime;
                case TimeType.UnscaledFixedTime:
                    return Time.fixedUnscaledTime;
                default:
                    return Time.deltaTime;
            }
        }

        /// <summary>
        /// Begins a movement operation on a transform.
        /// </summary>
        public static HUMTransform.Data.Move Move(this Transform transform)
        {
            return new HUMTransform.Data.Move(transform);
        }

        /// <summary>
        /// Begins a movement operation on a transform.
        /// </summary>
        public static HUMTransform.Data.MoveRigidbody Move(this Rigidbody rigidbody)
        {
            return new HUMTransform.Data.MoveRigidbody(rigidbody);
        }

        /// <summary>
        /// Begins a transform looking operation.
        /// </summary>
        public static Data.Look Look(this Transform transform)
        {
            return new Data.Look(transform);
        }

        /// <summary>
        /// Begins a transform snapping operation.
        /// </summary>
        public static Data.From Snap(this Transform transform)
        {
            return new Data.From(transform);
        }

        /// <summary>
        /// Begins a transform sizing operation.
        /// </summary>
        public static Data.Size Size(this Transform transform)
        {
            return new Data.Size(transform);
        }
    }
}