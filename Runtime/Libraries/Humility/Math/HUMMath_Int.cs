using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath
    {
        /// <summary>
        /// Reverses a float value. If negative, it'll come back positive. If positive, it'll come back negative.
        /// </summary>
        public static int Negate(this int value)
        {
            return value *= -1;
        }

        /// <summary>
        /// Ensures the value returns positive or 0.
        /// </summary>
        public static int Positive(this int value)
        {
            if (value < 0) return value.Negate();
            return value;
        }

        /// <summary>
        /// Ensures the value returns negative or 0.
        /// </summary>
        public static int Negative(this int value)
        {
            if (value > 0) return value.Negate();
            return value;
        }

        public static int ToInt(this string str)
        {
            return int.Parse(str);
        }
        /// <summary>
        /// Turns a range from one minimum and maximum, to another. The current value in the range is mapped accordingly.
        /// </summary>
        public static int Remap(this int from, int fromMin, int fromMax, int toMin, int toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }
    }
}