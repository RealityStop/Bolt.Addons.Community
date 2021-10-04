using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath
    {
        /// <summary>
        /// Adds X, Y, and Z of a vector by the same float value and returns the sum.
        /// </summary>
        public static Vector3 Add(this Vector3 multiplyValue, float value)
        {
            return new Vector3(multiplyValue.x + value, multiplyValue.y + value, multiplyValue.z + value);
        }

        /// <summary>
        /// Adds two vectors together and returns the sum.
        /// </summary>
        public static Vector3 Add(this Vector3 multiplyValue, UnityEngine.Vector3 value)
        {
            return new Vector3(multiplyValue.x + value.x, multiplyValue.y + value.y, multiplyValue.z + value.z);
        }

        /// <summary>
        /// Subtracts X, Y, and Z of a vector by the same float value.
        /// </summary>
        public static Vector3 Subtract(this Vector3 multiplyValue, float value)
        {
            return new Vector3(multiplyValue.x - value, multiplyValue.y - value, multiplyValue.z - value);
        }

        /// <summary>
        /// Subtracts two Vector2s from each other and returns the difference.
        /// </summary>
        public static Vector3 Subtract(this Vector3 multiplyValue, UnityEngine.Vector3 value)
        {
            return new Vector3(multiplyValue.x - value.x, multiplyValue.y - value.y, multiplyValue.z - value.z);
        }

        /// <summary>
        /// Multiplies X, Y, and Z of a vector by the same float value and returns the result.
        /// </summary>
        public static Vector3 Multiply(this Vector3 multiplyValue, float value)
        {
            return new Vector3(multiplyValue.x * value, multiplyValue.y * value, multiplyValue.z * value);
        }

        /// <summary>
        /// Multiplies one Vector by another, and returns the result.
        /// </summary>
        public static Vector3 Multiply(this Vector3 multiplyValue, UnityEngine.Vector3 value)
        {
            return new Vector3(multiplyValue.x * value.x, multiplyValue.y * value.y, multiplyValue.z * value.z);
        }

        /// <summary>
        /// Divides X, Y, and Z of a vector by the same float value, and returns the result.
        /// </summary>
        public static Vector3 Divide(this Vector3 multiplyValue, float value)
        {
            return new Vector3(multiplyValue.x / value, multiplyValue.y / value, multiplyValue.z / value);
        }

        /// <summary>
        /// Divides one Vector by another, and returns the result.
        /// </summary>
        public static Vector3 Divide(this Vector3 multiplyValue, UnityEngine.Vector3 value)
        {
            return new Vector3(multiplyValue.x / value.x, multiplyValue.y / value.y, multiplyValue.z / value.z);
        }

        /// <summary>
        /// Divides a vector in half and returns the result.
        /// </summary>
        public static Vector3 Half(this Vector3 vector)
        {
            return vector / 2;
        }

        public static HUMMath.Data.SetVector3 Set(this Vector3 vector)
        {
            return new Data.SetVector3(vector);
        }
    }
}