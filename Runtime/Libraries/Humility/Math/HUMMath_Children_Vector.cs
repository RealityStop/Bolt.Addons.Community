using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath_Children
    {
        /// <summary>
        /// Add to X of this Vector3.
        /// </summary>
        public static Vector3 X(this HUMMath.Data.SetVector3 set, float value)
        {
            return set.vector.Add(new Vector3(value, 0, 0));
        }

        /// <summary>
        /// Add to Y of this Vector3.
        /// </summary>
        public static Vector3 Y(this HUMMath.Data.SetVector3 set, float value)
        {
            return set.vector.Add(new Vector3(0, value, 0));
        }

        /// <summary>
        /// Add to Y of this Vector3.
        /// </summary>
        public static Vector3 Z(this HUMMath.Data.SetVector3 set, float value)
        {
            return set.vector.Add(new Vector3(0, 0, value));
        }

        /// <summary>
        /// Add to X of this Vector2.
        /// </summary>
        public static Vector2 X(this HUMMath.Data.SetVector2 set, float value)
        {
            return set.vector.Add(new Vector2(value, 0));
        }

        /// <summary>
        /// Add to Y of this Vector2.
        /// </summary>
        public static Vector2 Y(this HUMMath.Data.SetVector2 set, float value)
        {
            return set.vector.Add(new Vector2(0, value));
        }
    }
}