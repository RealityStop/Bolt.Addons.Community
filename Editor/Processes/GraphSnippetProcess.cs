using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

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
                __instance.Close();
                if (canvas.connectionSource is ControlOutput)
                {
                    var orderedSnippets = AssetDatabase.FindAssets($"t:{typeof(ControlGraphSnippet)}").Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Select(path => AssetDatabase.LoadAssetAtPath<ControlGraphSnippet>(path)).OrderableSearchFilter(currentQuery, (snippet) => snippet.SnippetName);
                    var snippet = orderedSnippets.FirstOrDefault(s => s.result != null);
                    if (snippet.result != null)
                    {
                        var sourceUnit = snippet.result.graph.units.First(unit => unit is SnippetControlSourceUnit) as SnippetControlSourceUnit;
                        if (!sourceUnit.source.hasValidConnection)
                        {
                            Debug.Log("No unit is connected to source unit in : " + snippet.result.name);
                            return;
                        }
                        var connectedUnit = (Unit)sourceUnit.source.connection.destination.unit;
                        var preservation = new SnippetHandler(snippet.result.SnippetType, connectedUnit);
                        preservation.AddSnippet(graph, canvas);
                    }
                }
                else if (canvas.connectionSource is ValueInput)
                {
                    var orderedSnippets = AssetDatabase.FindAssets($"t:{typeof(ValueGraphSnippet)}").Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Select(path => AssetDatabase.LoadAssetAtPath<ValueGraphSnippet>(path)).Where(snippet => (canvas.connectionSource as ValueInput).type.IsAssignableFrom(snippet.sourceType.type)).OrderableSearchFilter(currentQuery, (snippet) => snippet.SnippetName);
                    var snippet = orderedSnippets.FirstOrDefault(s => s.result != null);
                    if (snippet.result != null)
                    {
                        var sourceUnit = snippet.result.graph.units.First(unit => unit is SnippetValueSourceUnit) as SnippetValueSourceUnit;
                        if (!sourceUnit.source.hasValidConnection)
                        {
                            Debug.Log("No unit is connected to source unit in : " + snippet.result.name);
                            return;
                        }
                        var connectedUnit = (Unit)sourceUnit.source.connection.source.unit;
                        var preservation = new SnippetHandler(snippet.result.SnippetType, connectedUnit);
                        preservation.AddSnippet(graph, canvas);
                    }
                }
            }
        }
        else return;
    }
}
