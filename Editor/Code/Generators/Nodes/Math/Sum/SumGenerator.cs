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

namespace Unity.VisualScripting.Community.CSharp
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

            return TypeConversionUtility.CastTo(
                MakeClickableForThisUnit("(") +
                string.Join(MakeClickableForThisUnit(" + "), values) +
                MakeClickableForThisUnit(")"),
                inferredType,
                data.GetExpectedType(),
                Unit
            );
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
            List<Type> list = types.Where(t => t != null).ToList();
            if (list.Count == 0)
                return null;

            if (list.Count == 1)
                return list[0];

            Type first = list[0];
            
            if (list.All(t => t == first))
                return first;

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i; j < list.Count; j++)
                {
                    Type result;
                    if (HasAdditionOperator(list[i], list[j], out result))
                        return result ?? list[i];
                }
            }

            Type[] order = new Type[]
            {
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal)
            };

            Type best = list.First();
            foreach (Type t in list)
            {
                int bestIndex = Array.IndexOf(order, best);
                int tIndex = Array.IndexOf(order, t);
                if (tIndex > bestIndex && tIndex != -1)
                    best = t;
            }

            return best;
        }

        private static readonly Dictionary<(Type, Type), (bool, Type)> additionOperatorCache =
            new Dictionary<(Type, Type), (bool, Type)>();

        private static readonly Dictionary<Type, MethodInfo[]> methodCache =
            new Dictionary<Type, MethodInfo[]>();

        private static readonly HashSet<(Type, Type)> knownAddPairs =
            new HashSet<(Type, Type)>
            {
                (typeof(int), typeof(int)),
                (typeof(long), typeof(long)),
                (typeof(float), typeof(float)),
                (typeof(double), typeof(double)),
                (typeof(decimal), typeof(decimal))
            };

        private static MethodInfo[] GetCachedMethods(Type type)
        {
            MethodInfo[] methods;
            if (!methodCache.TryGetValue(type, out methods))
            {
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                methodCache[type] = methods;
            }
            return methods;
        }

        private static bool HasAdditionOperator(Type left, Type right, out Type returnType)
        {
            returnType = null;

            if (left == null || right == null)
                return false;

            if (knownAddPairs.Contains((left, right)))
            {
                returnType = left;
                return true;
            }

            (bool, Type) cached;
            if (additionOperatorCache.TryGetValue((left, right), out cached))
            {
                returnType = cached.Item2;
                return cached.Item1;
            }

            MethodInfo[] methods = GetCachedMethods(left);
            MethodInfo op = null;

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (m.Name == "op_Addition")
                {
                    ParameterInfo[] p = m.GetParameters();
                    if (p.Length == 2 && p[0].ParameterType == left && p[1].ParameterType == right)
                    {
                        op = m;
                        break;
                    }
                }
            }

            bool hasOperator = op != null;
            returnType = hasOperator ? op.ReturnType : null;

            additionOperatorCache[(left, right)] = (hasOperator, returnType);
            return hasOperator;
        }
    }
}