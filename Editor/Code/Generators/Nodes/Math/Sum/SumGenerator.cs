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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var connectedTypes = Unit.multiInputs
                .Select(i => GetSourceType(i, data, writer, false))
                .ToList();

            var inferredType = InferType(connectedTypes) ?? typeof(object);
            if (data.GetExpectedType() != null && data.GetExpectedType().IsStrictlyAssignableFrom(inferredType))
            {
                data.MarkExpectedTypeMet(inferredType);
            }
            data.CreateSymbol(Unit, inferredType);

            writer.Write("(");
            bool first = true;
            foreach (var item in Unit.multiInputs)
            {
                if (!first)
                    writer.Write(" + ");
                first = false;

                using (data.Expect(inferredType))
                {
                    GenerateValue(item, data, writer);
                }
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
                    writer.Write(val.As().Code(true, true, true, "", false));
            }
            else
            {
                writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
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