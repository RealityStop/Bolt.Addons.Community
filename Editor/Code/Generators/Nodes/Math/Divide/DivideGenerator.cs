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
    public abstract class DivideGenerator<T> : NodeGenerator<Divide<T>>
    {
        public DivideGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var inferredType = InferType(GetSourceType(Unit.dividend, data, writer, false), GetSourceType(Unit.divisor, data, writer, false)) ?? typeof(object);
            if (data.GetExpectedType() != null && data.GetExpectedType().IsStrictlyAssignableFrom(inferredType))
            {
                data.MarkExpectedTypeMet(inferredType);
            }
            data.CreateSymbol(Unit, inferredType);
            
            writer.Write("(");
            using (data.Expect(inferredType))
            {
                GenerateValue(Unit.dividend, data, writer);
            }
            writer.Write(" / ");
            using (data.Expect(inferredType))
            {
                GenerateValue(Unit.divisor, data, writer);
            }
            writer.Write(")");
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                GenerateConnectedValue(input, data, writer, false);
            }
            else if (input.hasDefaultValue)
            {
                var expectedType = data.GetExpectedType();
                var val = unit.defaultValues[input.key];

                if (expectedType == typeof(int))
                    writer.Write($"{val}".NumericHighlight());
                else if (expectedType == typeof(float))
                    writer.Write($"{val}f".Replace(",", ".").NumericHighlight());
                else if (expectedType == typeof(double))
                    writer.Write($"{val}d".Replace(",", ".").NumericHighlight());
                else if (expectedType == typeof(long))
                    writer.Write($"{val}L".Replace(",", ".").NumericHighlight());
                else
                    writer.Object(val, true, true, true, true, "", false);
            }
            else
            {
                writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
            }
        }

        private static Type InferType(Type left, Type right)
        {
            if (left == null || right == null) return null;

            if (HasDivideOperator(left, right, out var result))
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

        private static bool HasDivideOperator(Type left, Type right, out Type returnType)
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
                .FirstOrDefault(m => m.Name == "op_Division" && m.GetParameters().Length == 2 &&
                          m.GetParameters()[0].ParameterType == left &&
                          m.GetParameters()[1].ParameterType == right);

            bool hasOperator = op != null;

            returnType = hasOperator ? op.ReturnType : null;

            hasOperatorCache[(left, right)] = (hasOperator, returnType);

            return hasOperator;
        }
    }
}
