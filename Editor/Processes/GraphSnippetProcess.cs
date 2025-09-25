using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public class GraphSnippetProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            var __instance = FuzzyWindow.instance;
            if (__instance != null)
            {
                var fuzzyWindowType = typeof(FuzzyWindow);
                var queryField = fuzzyWindowType.GetField("query", BindingFlags.Instance | BindingFlags.NonPublic);
                var currentQuery = (string)queryField.GetValue(__instance);
    
                if (@event != null && @event.keyCode == KeyCode.Tab && canvas.connectionSource != null)
                {
                    if (string.IsNullOrEmpty(currentQuery))
                    {
                        return;
                    }
                    __instance.Close();
                    string snippetName = currentQuery.Split(',', StringSplitOptions.RemoveEmptyEntries)[0];
                    string[] argumentValues = currentQuery.Split(',', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
    
                    bool matchFound = false;
    
                    if (canvas.connectionSource is ControlOutput)
                    {
                        var snippets = AssetDatabase.FindAssets($"t:{typeof(ControlGraphSnippet)}")
                            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                            .Select(path => AssetDatabase.LoadAssetAtPath<ControlGraphSnippet>(path)).Where(snippet => snippet.snippetArguments.Count == argumentValues.Length)
                            .Where(snippet =>
                            {
                                for (int i = 0; i < snippet.snippetArguments.Count; i++)
                                {
                                    var argumentType = snippet.snippetArguments[i].argumentType;
                                    var argumentValue = argumentValues[i];
    
                                    if (IsCorrectType(argumentType, argumentValue))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                return true;
                            })
                            .ToList();
    
                        var orderedSnippets = snippets
                            .OrderableSearchFilter(snippetName, snippet => snippet.SnippetName)
                            .ToList();
    
                        matchFound = HandleSnippetSelection<ControlGraphSnippet, SnippetControlSourceUnit>(orderedSnippets, graph, canvas, currentQuery);
                    }
                    else if (canvas.connectionSource is ValueInput)
                    {
                        var snippets = AssetDatabase.FindAssets($"t:{typeof(ValueGraphSnippet)}")
                            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                            .Select(path => AssetDatabase.LoadAssetAtPath<ValueGraphSnippet>(path))
                            .Where(snippet => snippet.snippetArguments.Count == argumentValues.Length)
                                .Where(snippet =>
                                {
                                    for (int i = 0; i < snippet.snippetArguments.Count; i++)
                                    {
                                        var argumentType = snippet.snippetArguments[i].argumentType;
                                        var argumentValue = argumentValues[i];
    
                                        if (IsCorrectType(argumentType, argumentValue))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                })
                            .ToList();
    
                        var orderedSnippets = snippets
                            .Where(snippet =>
                                (canvas.connectionSource as ValueInput).type.IsAssignableFrom(snippet.sourceType.type) ||
                                (canvas.connectionSource as ValueInput).type.IsConvertibleTo(snippet.sourceType.type, true))
                            .OrderableSearchFilter(snippetName, snippet => snippet.SnippetName)
                            .ToList();
                        matchFound = HandleSnippetSelection<ValueGraphSnippet, SnippetValueSourceUnit>(orderedSnippets, graph, canvas, currentQuery);
                    }
    
                    if (!matchFound)
                    {
                        Debug.LogWarning("No matching snippet found for query: " + currentQuery);
                    }
                }
            }
        }
    
        private bool HandleSnippetSelection<TSnippet, TSourceUnit>(List<SearchResult<TSnippet>> orderedSnippets, FlowGraph graph, FlowCanvas canvas, string currentQuery) where TSnippet : GraphSnippet where TSourceUnit : SnippetSourceUnit
        {
            var snippet = orderedSnippets.FirstOrDefault(s => s.result != null);
            if (snippet.result == null) return false;
    
    
            if (snippet.result.graph.units.FirstOrDefault(unit => unit is SnippetControlSourceUnit || unit is SnippetValueSourceUnit) is not Unit sourceUnit)
            {
                Debug.LogWarning("Source unit in snippet is missing: " + snippet.result.name, snippet.result);
                return false;
            }
            else if (sourceUnit is SnippetControlSourceUnit controlSource && !controlSource.source.hasValidConnection)
            {
                Debug.LogWarning("No unit is connected to the source unit in snippet: " + snippet.result.name, snippet.result);
                return false;
            }
            else if (sourceUnit is SnippetValueSourceUnit valueSource && !valueSource.source.hasValidConnection)
            {
                Debug.LogWarning("No unit is connected to the source unit in snippet: " + snippet.result.name, snippet.result);
                return false;
            }
    
            var connectedUnit = (typeof(TSourceUnit) == typeof(SnippetControlSourceUnit) ? (sourceUnit as SnippetControlSourceUnit).source.connection.destination.unit : (sourceUnit as SnippetValueSourceUnit).source.connection.source.unit) as Unit;
            SnippetPreservationContext<TSourceUnit>.AddSnippet(graph, canvas, connectedUnit, snippet.result.SnippetType, snippet.result, currentQuery);
            return true;
        }
    
    
        private bool IsCorrectType(Type type, string input)
        {
            if (type == typeof(bool) && bool.TryParse(input, out bool boolResult))
            {
                return true;
            }

            if (type == typeof(char) && input.Length == 1)
            {
                return true;
            }

            if (type == typeof(byte) && byte.TryParse(input, out byte byteResult))
            {
                return true;
            }

            if (type == typeof(sbyte) && sbyte.TryParse(input, out sbyte sbyteResult))
            {
                return true;
            }

            if (type == typeof(short) && short.TryParse(input, out short shortResult))
            {
                return true;
            }

            if (type == typeof(ushort) && ushort.TryParse(input, out ushort ushortResult))
            {
                return true;
            }

            if (type == typeof(int) && int.TryParse(input, out int intResult))
            {
                return true;
            }

            if (type == typeof(uint) && uint.TryParse(input, out uint uintResult))
            {
                return true;
            }

            if (type == typeof(long) && long.TryParse(input, out long longResult))
            {
                return true;
            }

            if (type == typeof(ulong) && ulong.TryParse(input, out ulong ulongResult))
            {
                return true;
            }

            if (type == typeof(float) && float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatResult))
            {
                return true;
            }

            if (type == typeof(double) && double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
            {
                return true;
            }

            if (type == typeof(decimal) && decimal.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal decimalResult))
            {
                return true;
            }
            if (type == typeof(string))
            {
                return true;
            }
            return false;
        }
    } 
}