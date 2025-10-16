using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class SumGenerator<T> : NodeGenerator<T> where T : Unit, IMultiInputUnit
    {
        public SumGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> values = new List<string>();

            var connectedTypes = Unit.multiInputs
                .Select(i => GetSourceType(i, data))
                .ToList();

            var inferredType = InferType(connectedTypes) ?? typeof(object);

            foreach (var item in Unit.multiInputs)
            {
                data.SetExpectedType(inferredType);
                var code = GenerateValue(item, data);
                data.RemoveExpectedType();
                values.Add(code);
            }

            return (MakeClickableForThisUnit("(")
                 + string.Join(MakeClickableForThisUnit(" + "), values)
                 + MakeClickableForThisUnit(")")).CastTo(data.GetExpectedType(), Unit, data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType().IsNumeric());
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                var expected = data.GetExpectedType();
                var connectedCode = GetNextValueUnit(input, data);
                var inputType = GetSourceType(input, data);

                if (expected != null && expected != inputType && !expected.IsAssignableFrom(inputType))
                {
                    connectedCode = connectedCode.CastTo(expected, Unit);
                }

                return connectedCode;
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
                return MakeClickableForThisUnit($"/* \"{input.key} Requires Input\" */".WarningHighlight());
            }
        }

        private static Type InferType(IEnumerable<Type> types)
        {
            var list = types.Where(t => t != null).ToList();
            if (list.Count == 0) return null;

            foreach (var t in list)
            {
                if (HasAdditionOperator(t))
                    return t;
            }

            Type[] order = { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal), typeof(T) };
            var best = list.FirstOrDefault();
            foreach (var t in list)
            {
                if (order.Contains(t) && order.ToList().IndexOf(t) > order.ToList().IndexOf(best))
                    best = t;
            }

            return best;
        }
        private static readonly Dictionary<Type, bool> hasOperatorCache = new Dictionary<Type, bool>();

        private static bool HasAdditionOperator(Type type)
        {
            if (type == null)
                return false;

            if (hasOperatorCache.TryGetValue(type, out var cachedResult))
                return cachedResult;

            bool hasOperator = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Any(m => m.Name == "op_Addition" && m.GetParameters().Length == 2 &&
                          m.GetParameters()[0].ParameterType == type &&
                          m.GetParameters()[1].ParameterType == type);

            hasOperatorCache[type] = hasOperator;

            return hasOperator;
        }
    }
}