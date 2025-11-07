using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class MultiplyGenerator<T> : NodeGenerator<Multiply<T>>
    {
        public MultiplyGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var inferredType = InferType(GetSourceType(Unit.a, data), GetSourceType(Unit.b, data)) ?? typeof(object);
            data.SetExpectedType(inferredType);
            var a = GenerateValue(Unit.a, data);
            data.RemoveExpectedType();
            data.SetExpectedType(inferredType);
            var b = GenerateValue(Unit.b, data);
            data.RemoveExpectedType();
            data.CreateSymbol(Unit, inferredType);
            return TypeConversionUtility.CastTo(MakeClickableForThisUnit("(") + a + MakeClickableForThisUnit(" * ") + b + MakeClickableForThisUnit(")"), inferredType, data.GetExpectedType(), Unit);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input, data);
            }
            else if (input.hasDefaultValue)
            {
                var expectedType = data.GetExpectedType();
                var val = unit.defaultValues[input.key];

                if (expectedType == typeof(int))
                    return MakeClickableForThisUnit($"{val}".NumericHighlight());
                if (expectedType == typeof(float))
                    return MakeClickableForThisUnit($"{val}f".Replace(",", ".").NumericHighlight());
                if (expectedType == typeof(double))
                    return MakeClickableForThisUnit($"{val}d".Replace(",", ".").NumericHighlight());
                if (expectedType == typeof(long))
                    return MakeClickableForThisUnit($"{val}L".Replace(",", ".").NumericHighlight());

                return val.As().Code(true, Unit, true, true, "", false);
            }
            else
            {
                return $"/* \"{input.key} Requires Input\" */".WarningHighlight();
            }
        }

        private static Type InferType(Type left, Type right)
        {
            if (left == null || right == null) return null;

            if (HasMultiplyOperator(left, right, out var result))
                return result;

            Type[] order = { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(T) };
            var best = left;
            foreach (var t in left.Yield().Append(right))
            {
                if (order.Contains(t) && order.ToList().IndexOf(t) > order.ToList().IndexOf(best))
                    best = t;
            }

            return best;
        }
        private static readonly Dictionary<(Type, Type), (bool, Type)> hasOperatorCache = new Dictionary<(Type, Type), (bool, Type)>();

        private static bool HasMultiplyOperator(Type left, Type right, out Type returnType)
        {
            if (left == null)
            {
                returnType = null;
                return false;
            }

            if (right == null)
            {
                returnType = null;
                return false;
            }

            if (hasOperatorCache.TryGetValue((left, right), out var cachedResult))
            {
                returnType = cachedResult.Item2;
                return cachedResult.Item1;
            }
            var op = left.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Multiply" && m.GetParameters().Length == 2 &&
                          m.GetParameters()[0].ParameterType == left &&
                          m.GetParameters()[1].ParameterType == right);

            bool hasOperator = op != null;

            returnType = hasOperator ? op.ReturnType : null;

            hasOperatorCache[(left, right)] = (hasOperator, returnType);

            return hasOperator;
        }
    }
}
