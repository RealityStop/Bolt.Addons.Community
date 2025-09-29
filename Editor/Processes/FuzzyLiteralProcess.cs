using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Variables.Editor;

namespace Unity.VisualScripting.Community
{
    public class FuzzyLiteralProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        private string query = "Fuzzy Literal";
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            var __instance = FuzzyWindow.instance;
            if (__instance == null) return;
            var fuzzyWindowType = typeof(FuzzyWindow);
            var queryField = fuzzyWindowType.GetField("query", BindingFlags.Instance | BindingFlags.NonPublic);
            var currentQuery = (string)queryField.GetValue(__instance);
            query = currentQuery;

            Options.dynamicLiteralOptions[typeof(string)].unit.value = query;
            Options.dynamicLiteralOptions[typeof(string)].Update(query);

            if (TryParseValue(query, out var value))
            {
                foreach (var optionType in Options.dynamicLiteralOptions.Keys)
                {
                    if (optionType.IsInstanceOfType(value) || value.IsConvertibleTo(optionType, true))
                    {
                        try
                        {
                            var option = Options.dynamicLiteralOptions[optionType];
                            option.Update(query);
                            option.unit.value = value.ConvertTo(optionType);
                        }
                        catch
                        {
                            //silently catch conversion errors
                        }
                    }
                }
            }

            if (IsExpression(query))
            {
                string[] tokens = TokenizeExpression(query);
                Options.fuzzyExpressionOption.unit.tokens.Clear();

                foreach (string token in tokens)
                {
                    Options.fuzzyExpressionOption.unit.tokens.Add(token);
                }
                Options.fuzzyExpressionOption.Update(query);
            }
            else
            {
                return;
            }
        }

        private static bool TryParseValue(string target, out object value)
        {
            if (int.TryParse(target, out var intValue))
            {
                value = intValue;
                return true;
            }
            else if (float.TryParse(target, out var floatValue))
            {
                value = floatValue;
                return true;
            }
            else if (long.TryParse(target, out var longValue))
            {
                value = longValue;
                return true;
            }
            else if (double.TryParse(target, out var doubleValue))
            {
                value = doubleValue;
                return true;
            }
            else if (ulong.TryParse(target, out var ulongValue))
            {
                value = ulongValue;
                return true;
            }
            else if (short.TryParse(target, out var shortValue))
            {
                value = shortValue;
                return true;
            }
            else if (ushort.TryParse(target, out var ushortValue))
            {
                value = ushortValue;
                return true;
            }
            else if (bool.TryParse(target, out var boolValue))
            {
                value = boolValue;
                return true;
            }
            else if (DateTime.TryParse(target, out var dateTimeValue))
            {
                value = dateTimeValue;
                return true;
            }
            else if (decimal.TryParse(target, out var decimalValue))
            {
                value = decimalValue;
                return true;
            }
            else if (Guid.TryParse(target, out var guidValue))
            {
                value = guidValue;
                return true;
            }
            else if (TimeSpan.TryParse(target, out var timeSpanValue))
            {
                value = timeSpanValue;
                return true;
            }
            value = target;
            return false;
        }

        private static string[] TokenizeExpression(string expression)
        {
            return Regex.Split(expression, @"(\+|\-|\*|\/|\(|\)|\b\d+(\.\d+)?\b)")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();
        }

        private static bool IsExpression(string query)
        {
            if (string.IsNullOrEmpty(query) || !(query.Contains("+") || query.Contains("-") || query.Contains("*") || query.Contains("/")))
            {
                return false;
            }
            string pattern = @"^\s*\(*\s*\d+(\s*[+\-*\/]\s*\d+\s*)*\)*\s*$";

            return Regex.IsMatch(query, pattern);
        }
    }
}
