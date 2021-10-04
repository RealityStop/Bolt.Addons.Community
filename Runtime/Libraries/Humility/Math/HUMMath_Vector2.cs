using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath
    {
        /// <summary>
        /// Adds both X and Y of a vector by the same float value and returns the sum.
        /// </summary>
        public static Vector2 Add(this Vector2 multiplyValue, float value)
        {
            return new Vector2(multiplyValue.x + value, multiplyValue.y + value);
        }

        /// <summary>
        /// Adds two vectors together and returns the sum.
        /// </summary>
        public static Vector2 Add(this Vector2 multiplyValue, UnityEngine.Vector2 value)
        {
            return new Vector2(multiplyValue.x + value.x, multiplyValue.y + value.y);
        }

        /// <summary>
        /// Subtracts both X and Y of a vector by the same float value.
        /// </summary>
        public static Vector2 Subtract(this Vector2 multiplyValue, float value)
        {
            return new Vector2(multiplyValue.x - value, multiplyValue.y - value);
        }

        /// <summary>
        /// Subtracts two Vector2s from each other and returns the difference.
        /// </summary>
        public static Vector2 Subtract(this Vector2 multiplyValue, UnityEngine.Vector2 value)
        {
            return new Vector2(multiplyValue.x - value.x, multiplyValue.y - value.y);
        }

        /// <summary>
        /// Multiplies both X and Y of a vector by the same float value and returns the result.
        /// </summary>
        public static Vector2 Multiply(this Vector2 multiplyValue, float value)
        {
            return new Vector2(multiplyValue.x * value, multiplyValue.y * value);
        }

        /// <summary>
        /// Multiplies one Vector2 by another, and returns the result.
        /// </summary>
        public static Vector2 Multiply(this Vector2 multiplyValue, UnityEngine.Vector2 value)
        {
            return new Vector2(multiplyValue.x * value.x, multiplyValue.y * value.y);
        }

        /// <summary>
        /// Divides both X and Y of a vector by the same float value, and returns the result.
        /// </summary>
        public static Vector2 Divide(this Vector2 multiplyValue, float value)
        {
            return new Vector2(multiplyValue.x / value, multiplyValue.y / value);
        }

        /// <summary>
        /// Divides one Vector2 by another, and returns the result.
        /// </summary>
        public static Vector2 Divide(this Vector2 multiplyValue, UnityEngine.Vector2 value)
        {
            return new Vector2(multiplyValue.x / value.x, multiplyValue.y / value.y);
        }

        /// <summary>
        /// Divides a vector in half and returns the result.
        /// </summary>
        public static Vector2 Half(this Vector2 vector)
        {
            return vector / 2;
        }

        /// <summary>
        /// Divides a vector into a quarter of the original, and returns the result.
        /// </summary>
        public static Vector2 Quarter(this Vector2 vector)
        {
            return vector / 4;
        }
    }
}