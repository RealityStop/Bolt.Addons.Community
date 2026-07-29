using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class BinaryComparisonGenerator<TUnit> : NodeGenerator<TUnit> where TUnit : Unit
    {
        protected BinaryComparisonGenerator(TUnit unit) : base(unit)
        {
        }

        protected abstract ValueInput LeftInput { get; }
        protected abstract ValueInput RightInput { get; }
        protected abstract ValueOutput OutputPort { get; }
        protected abstract string OperatorToken { get; }
        protected abstract string OperatorMethodName { get; }

        private static readonly List<Type> NumericOrder = new List<Type>
        {
            typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)
        };

        #region Custom Operator Lookup Caching
        private static readonly HashSet<(Type Left, Type Right, string Method)> KnownOperators = new HashSet<(Type Left, Type Right, string Method)>();
        private static readonly HashSet<(Type Left, Type Right, string Method)> MissingOperators = new HashSet<(Type Left, Type Right, string Method)>();
        #endregion

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == OutputPort)
            {
                var leftSourceType = GetSourceType(LeftInput, data, writer, false);
                var rightSourceType = GetSourceType(RightInput, data, writer, false);

                var comparisonType = InferComparisonType(leftSourceType, rightSourceType);

                writer.Write("(");

                using (data.Expect(comparisonType))
                {
                    GenerateValue(LeftInput, data, writer);
                }

                writer.Write(OperatorToken);

                using (data.Expect(comparisonType))
                {
                    GenerateValue(RightInput, data, writer);
                }

                writer.Write(")");
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == LeftInput || input == RightInput)
            {
                if (input.hasValidConnection)
                {
                    if (input.type != typeof(object))
                    {
                        Type actualSourceType = GetSourceType(input, data, writer, false);
                        Type expectedType = data.GetExpectedType();

                        if (expectedType != null && actualSourceType != null)
                        {
                            bool isStandardCsharpAssignment = expectedType.IsAssignableFrom(actualSourceType);

                            ConversionUtility.ConversionType conversionType = ConversionUtility.GetRequiredConversion(actualSourceType, expectedType);

                            bool isCsharpExpressionSafe = conversionType == ConversionUtility.ConversionType.Identity ||
                                                        conversionType == ConversionUtility.ConversionType.Upcast ||
                                                        conversionType == ConversionUtility.ConversionType.NumericImplicit ||
                                                        conversionType == ConversionUtility.ConversionType.UserDefinedImplicit;

                            bool naturallyCompatible = isStandardCsharpAssignment || isCsharpExpressionSafe;

                            if (!naturallyCompatible)
                            {
                                writer.Write($"({writer.GetTypeNameHighlighted(expectedType)})");
                                GenerateConnectedValue(input, data, writer, false);
                                return;
                            }
                        }
                    }

                    GenerateConnectedValue(input, data, writer);
                    return;
                }

                using (writer.BeginNode(input.unit as Unit))
                {
                    if (input.hasDefaultValue)
                    {
                        WriteDefaultValue(input, data, writer);
                        return;
                    }

                    writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
                }
            }
        }

        private Type InferComparisonType(Type left, Type right)
        {
            if (left == null && right == null) return typeof(float);
            if (left == null) left = typeof(object);
            if (right == null) right = typeof(object);

            if (HasCustomOperator(left, right, OperatorMethodName)) return left;
            if (HasCustomOperator(right, left, OperatorMethodName)) return right;

            if (left == typeof(object) && right != typeof(object)) return right;
            if (right == typeof(object) && left != typeof(object)) return left;
            if (left == typeof(object) && right == typeof(object)) return typeof(float);

            int leftIndex = NumericOrder.IndexOf(left);
            int rightIndex = NumericOrder.IndexOf(right);

            if (leftIndex >= 0 && rightIndex >= 0)
            {
                return leftIndex >= rightIndex ? left : right;
            }

            return left;
        }

        private static bool HasCustomOperator(Type left, Type right, string methodName)
        {
            if (left == null || right == null || string.IsNullOrEmpty(methodName)) return false;

            var lookupKey = (left, right, methodName);
            if (KnownOperators.Contains(lookupKey)) return true;
            if (MissingOperators.Contains(lookupKey)) return false;

            var op = left.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2 &&
                                    m.GetParameters()[0].ParameterType == left &&
                                    m.GetParameters()[1].ParameterType == right);

            if (op != null)
            {
                KnownOperators.Add(lookupKey);
                return true;
            }

            MissingOperators.Add(lookupKey);
            return false;
        }
    }
}