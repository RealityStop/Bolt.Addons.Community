using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class MathLibrary
    {
        const float Eulers = 2.71828182845904523536028747135266249f;

        public static float LinearFunction(float input, float min, float max)
        {
            return Mathf.Lerp(min, max, input);
        }

        public static float ReverseLinearFunction(float input, float min, float max)
        {
            float clampedInput = Mathf.Clamp(input, min, max);
            return Mathf.InverseLerp(min, max, input);
        }

        public static float LinearFunctionOfRange(float input, float minValue, float maxValue, float minimum, float maximum)
        {
            float interpolated = ReverseLinearFunction(input, minValue, maxValue);
            return LinearFunction(interpolated, minValue, maxValue);
        }


        public static float ExponentialFunction(float value, float minWeight, float exponent, float scale)
        {
            return scale * Mathf.Pow(value, exponent) + minWeight;
        }

        public static float ExponentialFunctionOfRange(float value, float minValue, float maxValue, float minWeight, float exponent, float scale)
        {
            exponent = Mathf.Max(1, exponent);

            float y = ReverseLinearFunction(value, minValue, maxValue);
            return ExponentialFunction(y, minWeight, exponent, scale);
        }

        public static float LogarithmicFunction(float value, float minimum, float exponent, float scale)
        {
            exponent = Mathf.Clamp01(exponent);
            return scale * Mathf.Pow(value, exponent) + minimum;
        }


        public static float LogarithmicFunctionOfRange(float value, float minValue, float maxValue, float minWeight, float exponent, float scale)
        {
            float y = ReverseLinearFunction(value, minValue, maxValue);
            return LogarithmicFunction(y, minWeight, exponent, scale);
        }



        public static float DecayFunction(float value, float minWeight, float decayFactor, float scale)
        {
            value = Mathf.Clamp01(value);
            return scale * Mathf.Pow(decayFactor, value) + minWeight;
        }


        public static float DecayFunctionOfRange(float value, float minValue, float maxValue, float minWeight, float decayFactor, float scale)
        {

            float y = ReverseLinearFunction(value, minValue, maxValue);
            return DecayFunction(y, minWeight, decayFactor, scale);
        }

        public static float DecayingSigmoid(float value, float inflectionPoint, float minWeight, float decayFactor, float scale)
        {
            value = Mathf.Clamp01(value);
            inflectionPoint = Mathf.Clamp01(inflectionPoint);

            float y = 1 / (1 + Mathf.Pow(Eulers, (decayFactor * (value - inflectionPoint))));
            y = scale * y + minWeight;
            return y;
        }


        public static float DecayingSigmoidOfRange(float value, float minValue, float maxValue, float inflectionPoint, float minWeight, float decayFactor, float scale)
        {
            float y = ReverseLinearFunction(value, minValue, maxValue);
            float normalizedInflection = ReverseLinearFunction(inflectionPoint, minValue, maxValue);
            return DecayingSigmoid(y, normalizedInflection, minWeight, decayFactor, scale);
        }
    }
}