using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    public static class GraphUtility
    {
        private static Dictionary<FlowGraph, (List<CommentNode> commentNodes, int unitCount)> commentCache = new Dictionary<FlowGraph, (List<CommentNode>, int)>();

        public static List<CommentNode> GetComments(this FlowGraph graph)
        {
            if (!commentCache.TryGetValue(graph, out var cached) || cached.unitCount != graph.units.Count)
            {
                cached.commentNodes = graph.units.Where(u => u is CommentNode).Cast<CommentNode>().ToList();
                cached.unitCount = graph.units.Count;
                commentCache[graph] = cached;
            }
            return cached.commentNodes;
        }

        public static void AddNewUnitContextual(FlowGraph graph, FlowCanvas canvas, System.Action<IGraphElement> added, System.Action canceled = null)
        {
            OverrideContextIfNeeded(async () =>
            {
                UndoUtility.RecordEditedObject("Added new unit");
                canvas.NewUnitContextual();
                var elementResult = await WaitForElementAddedOrCanceled(graph);
                if (elementResult.Canceled)
                {
                    canceled?.Invoke();
                    return;
                }

                added?.Invoke(elementResult.Element);
            });
        }

        /// <summary>
        /// Adds a new unit with its position based on the ports.
        /// </summary>
        public static void AddNewPositionedUnit(FlowGraph graph, FlowCanvas canvas, IUnitPort unitPort, System.Action<Unit> added, System.Action canceled = null)
        {
            OverrideContextIfNeeded(() =>
            {
                AddNewUnitContextual(graph, canvas, (element) =>
                {
                    var unit = element as Unit;
                    if (unit == null) return;

                    UndoUtility.RecordEditedObject("Added new positioned unit");
                    if (unitPort is ValueInput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x - 150, unitPos.y + 150);
                    }
                    else if (unitPort is ValueOutput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x + 150, unitPos.y + 150);
                    }
                    else if (unitPort is ControlOutput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x + 250, unitPos.y);
                    }
                    else if (unitPort is ControlInput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x - 250, unitPos.y);
                    }
                    graph.pan = unit.position;
                    added?.Invoke(unit);
                }, canceled);
            });
        }

        private static List<IGraph> waitingGraphs = new List<IGraph>();
        public static void WaitForNewUnit(FlowGraph graph, System.Action<IGraphElement> added, System.Action canceled = null)
        {
            if (waitingGraphs.Contains(graph)) return;
            OverrideContextIfNeeded(async () =>
            {
                waitingGraphs.Add(graph);

                var elementResult = await WaitForElementAddedOrCanceled(graph);

                waitingGraphs.Remove(graph);

                if (elementResult.Canceled)
                {
                    canceled?.Invoke();
                    return;
                }

                added?.Invoke(elementResult.Element);
            });
        }

        public static void OverrideContextIfNeeded(System.Action action)
        {
            bool manuallySet = false;
            if (LudiqGraphsEditorUtility.editedContext.value == null && GraphWindow.active != null && GraphWindow.active.reference != null)
            {
                var context = GraphWindow.active.reference.Context();
                if (context != null)
                {
                    manuallySet = true;
                    LudiqGraphsEditorUtility.editedContext.BeginOverride(context);
                }
            }
            action?.Invoke();
            if (manuallySet) LudiqGraphsEditorUtility.editedContext.EndOverride();
        }

        public static void DescribeAnalyzeAndDefineFlowGraph(this IGraphContext context)
        {
            if (context?.graph is FlowGraph graph)
            {
                context.DescribeAndAnalyze();
                foreach (var unit in graph.units)
                {
                    unit.Define();
                }
            }
            else
            {
                context?.DescribeAndAnalyze();
            }
        }

        public static void CleanGraphFrom(Unit startUnit)
        {
            var visited = new HashSet<Unit>();
            UndoUtility.RecordEditedObject("Cleaned Graph");
            ArrangeUnitRecursive(startUnit, startUnit.position, visited);
        }

        static void ArrangeUnitRecursive(Unit unit, Vector2 basePosition, HashSet<Unit> visited)
        {
            if (!visited.Add(unit)) return;

            unit.position = basePosition;
            var widget = FindNonOverlappingPosition(unit, 5);
            float spacingY = 150f + widget.position.height;
            float spacingX = 150f + widget.position.width;

            var controlOutputs = unit.controlOutputs.ToList();
            for (int i = 0; i < controlOutputs.Count; i++)
            {
                var port = controlOutputs[i];
                var connections = port.validConnections.ToList();
                for (int j = 0; j < connections.Count; j++)
                {
                    var connectedUnit = connections[j].destination.unit;
                    var offset = new Vector2(spacingX, (i - controlOutputs.Count / 2f) * spacingY);
                    ArrangeUnitRecursive(connectedUnit as Unit, basePosition + offset, visited);
                }
            }

            var valueInputs = unit.valueInputs.ToList();
            for (int i = 0; i < valueInputs.Count; i++)
            {
                var port = valueInputs[i];
                var connection = port.validConnections.FirstOrDefault();
                if (connection != null)
                {
                    var connectedUnit = connection.source.unit;
                    if (connectedUnit.controlInputs.Count == 0 && connectedUnit.controlOutputs.Count > 0) continue; // Most likely a source unit
                    var offset = new Vector2(-spacingX, (i - valueInputs.Count / 2f) * spacingY);
                    ArrangeUnitRecursive(connectedUnit as Unit, basePosition + offset, visited);
                }
            }

            widget?.Reposition();
        }

        static IUnitWidget FindNonOverlappingPosition(Unit unit, float minDistance)
        {
            var canvas = unit.graph.Canvas<FlowCanvas>();
            var unitWidget = canvas.Widget<IUnitWidget>(unit);
            int tries = 0;
            int maxTries = 50;
            while (PositionOverlaps(canvas, unitWidget, 8))
            {
                unitWidget.position = new Rect(unitWidget.position.position + new Vector2(minDistance, minDistance), unitWidget.position.size).PixelPerfect();

                unitWidget.CachePositionFirstPass();
                unitWidget.CachePosition();

                unitWidget.Reposition();
                if (++tries > maxTries)
                {
                    Debug.LogWarning($"Max repositioning attemptes reached! Could not find a non overlapping position for {unitWidget.element}.");
                    return unitWidget;
                }
            }
            return unitWidget;
        }

        public static bool PositionOverlaps(FlowCanvas canvas, IGraphElementWidget widget, float threshold = 3)
        {
            var position = widget.position;

            return canvas.graph.units.Any(otherElement =>
            {
                // Skip itself, which would by definition always overlap
                if (otherElement == widget.element)
                {
                    return false;
                }

                var positionA = canvas.Widget(otherElement).position;
                var positionB = position;

                return Mathf.Abs(positionA.xMin - positionB.xMin) < threshold &&
                Mathf.Abs(positionA.yMin - positionB.yMin) < threshold;
            });
        }

        private static Task<ElementAddResult> WaitForElementAddedOrCanceled(FlowGraph graph)
        {
            var tcs = new TaskCompletionSource<ElementAddResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void AddedHandler(IGraphElement element)
            {
                graph.elements.ItemAdded -= AddedHandler;
                EditorApplication.update -= CheckClosed;
                tcs.TrySetResult(new ElementAddResult { Canceled = false, Element = element });
            }

            void CheckClosed()
            {
                if (FuzzyWindow.instance == null)
                {
                    graph.elements.ItemAdded -= AddedHandler;
                    EditorApplication.update -= CheckClosed;
                    tcs.TrySetResult(new ElementAddResult { Canceled = true, Element = null });
                }
            }

            void WaitForOpen()
            {
                if (FuzzyWindow.instance != null)
                {
                    EditorApplication.update -= WaitForOpen;

                    graph.elements.ItemAdded += AddedHandler;
                    EditorApplication.update += CheckClosed;
                }
            }

            EditorApplication.update += WaitForOpen;

            return tcs.Task;
        }
        private struct ElementAddResult
        {
            public bool Canceled;
            public IGraphElement Element;
        }

        public static List<UnifiedVariableUnit> GetFlowVariablesRenameTargets(Unit currentUnit, string oldName, GraphReference reference = null)
        {
            List<UnifiedVariableUnit> variableUnits = new List<UnifiedVariableUnit>();
            foreach (var source in RuntimeGraphUtility.GetFlowSourceUnits(currentUnit, reference))
            {
                foreach (var unit in RuntimeGraphUtility.GetUnitsOfType<UnifiedVariableUnit>(source, (v) => v.kind == VariableKind.Flow, reference))
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Flow && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            variableUnits.Add(variableUnit);
                        }
                    }
                }
            }

            return variableUnits;
        }

        public static void UpdateAllGraphVariables(FlowGraph graph, string oldName, string newName)
        {
            foreach (var unit in graph.units)
            {
                if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Graph && variableUnit.isDefined)
                {
                    if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                    {
                        variableUnit.name.SetDefaultValue(newName);
                    }
                }
            }
        }

        public static List<UnifiedVariableUnit> GetGraphVariablesRenameTargets(FlowGraph graph, string oldName)
        {
            List<UnifiedVariableUnit> variableUnits = new List<UnifiedVariableUnit>();
            foreach (var unit in graph.units)
            {
                if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Graph && variableUnit.isDefined)
                {
                    if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                    {
                        variableUnits.Add(variableUnit);
                    }
                }
            }
            return variableUnits;
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetObjectVariablesRenameTargets(GameObject gameObject, string oldName, VariableKind kind = VariableKind.Object)
        {
            if (gameObject.scene != null && gameObject.scene.IsValid())
            {
                var results = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
                foreach (var rootGameObject in gameObject.scene.GetRootGameObjects())
                {
                    foreach (var component in rootGameObject.GetComponentsInChildren<IMachine>(true))
                    {
                        results.AddRange(GetFromComponent(component));
                    }
                }
                return results;
            }
            else if (gameObject != null)
            {
                var results = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
                var components = gameObject.GetComponents<IMachine>();

                foreach (var component in components)
                {
                    results.AddRange(GetFromComponent(component));
                }
                return results;
            }
            else
            {
                return new List<(UnifiedVariableUnit, UnityEngine.Object)>();
            }

            List<(UnifiedVariableUnit, UnityEngine.Object)> GetFromComponent(IMachine component)
            {
                if (component == null || component.GetReference() == null || component.nest == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                GraphTraversal.TraverseGraph(component.GetReference().graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == kind && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            if (kind == VariableKind.Object)
                            {
                                var reference = GraphTraversal.GetChildReferenceWithGraph(component.GetReference().AsReference(), variableUnit.graph);
                                if (Flow.CanPredict(variableUnit.@object, reference))
                                {
                                    var value = Flow.Predict(variableUnit.@object, reference);
                                    if (value is GameObject @object)
                                    {
                                        if (reference != null && @object == gameObject)
                                        {
                                            variableUnits.Add((variableUnit, reference.rootObject));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                variableUnits.Add((variableUnit, component as UnityEngine.Object));
                            }
                        }
                    }
                });

                return variableUnits;
            }
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetObjectVariablesRenameTargets(GraphReference reference, GameObject initialValue, string oldName)
        {
            if (reference == null || reference.graph == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            if (initialValue != null) variableUnits.AddRange(GetObjectVariablesRenameTargets(initialValue, oldName));
            else
            {
                GraphTraversal.TraverseGraph(reference.graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Object && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            var graphReference = GraphTraversal.GetChildReferenceWithGraph(reference, variableUnit.graph);
                            if (Flow.CanPredict(variableUnit.@object, graphReference))
                            {
                                var value = Flow.Predict(variableUnit.@object, graphReference);
                                if (value is GameObject @object)
                                {
                                    if (graphReference != null && @object == initialValue)
                                    {
                                        variableUnits.Add((variableUnit, reference.rootObject));
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return variableUnits;
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetSceneVariablesRenameTargets(Scene scene, string oldName,
        VariableKind kind = VariableKind.Scene, Func<IMachine, bool> predicate = null)
        {
            if (scene != null && scene.IsValid())
            {
                var results = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (var component in rootGameObject.GetComponentsInChildren<IMachine>(true))
                    {
                        results.AddRange(GetFromComponent(component));
                    }
                }
                return results;
            }
            else
            {
                return new List<(UnifiedVariableUnit, UnityEngine.Object)>();
            }

            List<(UnifiedVariableUnit, UnityEngine.Object)> GetFromComponent(IMachine component)
            {
                if (component == null || component.GetReference() == null || component.nest == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                if (predicate == null || predicate(component))
                    GraphTraversal.TraverseGraph(component.GetReference().graph, (unit) =>
                    {
                        if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == kind && variableUnit.isDefined)
                        {
                            if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                            {
                                variableUnits.Add((variableUnit, component.GetReference().rootObject));
                            }
                        }
                    });

                return variableUnits;
            }
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetSceneVariablesRenameTargets(GraphReference reference, Scene? scene, string oldName, VariableKind kind = VariableKind.Scene)
        {
            if (reference == null || reference.graph == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            if (scene != null) variableUnits.AddRange(GetSceneVariablesRenameTargets(scene.Value, oldName, kind));
            else
            {
                GraphTraversal.TraverseGraph(reference.graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == kind && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            variableUnits.Add((variableUnit, reference.rootObject));
                        }
                    }
                });
            }

            return variableUnits;
        }

        private static IEnumerable<string> GetAllScenePaths()
        {
            return AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath);
        }

        private static IEnumerable<UnityEngine.Object> GetAllMacros()
        {
            return AssetDatabase.FindAssets("t:LudiqScriptableObject").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadMainAssetAtPath).Where(asset => asset is IMacro);
        }

        private static void UpdateProjectVariables(string oldName, string newName, VariableKind kind)
        {
            if (kind != VariableKind.Application && kind != VariableKind.Saved)
                throw new InvalidOperationException();

            try
            {
                List<string> scenePaths = GetAllScenePaths().ToList();
                int totalScenes = scenePaths.Count;

                float scenePhaseWeight = 0.5f;
                float macroPhaseWeight = 0.3f;
                float prefabPhaseWeight = 0.2f;

                List<string> loadedScenes = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded) continue;
                    loadedScenes.Add(scene.path);
                }

                for (int sceneIndex = 0; sceneIndex < totalScenes; sceneIndex++)
                {
                    string scenePath = scenePaths[sceneIndex];
                    if (!scenePath.StartsWith("Assets/")) continue;

                    float sceneProgress = (float)sceneIndex / totalScenes;
                    float progress = sceneProgress * scenePhaseWeight;

                    EditorUtility.DisplayProgressBar(
                        "Renaming Variables",
                        "Processing Scene: " + scenePath,
                        progress
                    );

                    Scene scene = SceneManager.GetSceneByPath(scenePath);

                    try
                    {
                        if (!loadedScenes.Contains(scenePath))
                            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    }
                    catch
                    {
                        continue;
                    }

                    if (!scene.IsValid())
                        continue;

                    bool sceneModified = false;

                    List<(UnifiedVariableUnit, UnityEngine.Object)> targets =
                        GetSceneVariablesRenameTargets(scene, oldName, kind, m => m.nest != null && m.nest.source == GraphSource.Embed).ToList();

                    int unitCount = targets.Count;
                    for (int i = 0; i < unitCount; i++)
                    {
                        float unitProgress = unitCount == 0 ? 0f : (float)i / unitCount;
                        float combinedProgress = progress + (unitProgress * scenePhaseWeight / totalScenes);

                        EditorUtility.DisplayProgressBar(
                            "Renaming Variables",
                            "Processing Scene: " + scenePath,
                            combinedProgress
                        );

                        if (UpdateVariable(targets[i].Item1, newName))
                            sceneModified = true;
                    }

                    if (sceneModified)
                        EditorSceneManager.SaveScene(scene);

                    if (!loadedScenes.Contains(scenePath))
                        EditorSceneManager.CloseScene(scene, true);
                }

                float macroBase = scenePhaseWeight;
                List<UnityEngine.Object> macros = GetAllMacros().ToList();
                int macroCount = macros.Count;

                for (int i = 0; i < macroCount; i++)
                {
                    float progress = macroBase + ((float)i / macroCount) * macroPhaseWeight;

                    EditorUtility.DisplayProgressBar(
                        "Renaming Variables",
                        "Processing Macro: " + macros[i].name,
                        progress
                    );

                    UnityEngine.Object macroObject = macros[i];
                    if (macroObject is IMacro macro && macro.graph != null)
                    {
                        bool modified = false;

                        GraphTraversal.TraverseGraph(macro.graph, unit =>
                        {
                            if (unit is UnifiedVariableUnit variableUnit &&
                                variableUnit.kind == kind &&
                                variableUnit.isDefined &&
                                !variableUnit.name.hasValidConnection &&
                                (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                            {
                                if (UpdateVariable(variableUnit, newName))
                                    modified = true;
                            }
                        });

                        if (modified)
                        {
                            EditorUtility.SetDirty(macroObject);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }

                float prefabBase = scenePhaseWeight + macroPhaseWeight;
                List<GameObject> prefabs = AssetUtility.GetAllAssetsOfType<GameObject>().ToList();
                int prefabCount = prefabs.Count;

                for (int i = 0; i < prefabCount; i++)
                {
                    float progress = prefabBase + ((float)i / prefabCount) * prefabPhaseWeight;

                    EditorUtility.DisplayProgressBar(
                        "Renaming Variables",
                        "Processing Prefab: " + prefabs[i].name,
                        progress
                    );

                    UpdateAllObjectVariables(prefabs[i], oldName, newName, kind);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }


        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetCurrentlyAccessibleProjectUnits(string oldName, VariableKind kind = VariableKind.Application)
        {
            if (kind != VariableKind.Application && kind != VariableKind.Saved)
                throw new InvalidOperationException();

            List<(UnifiedVariableUnit, UnityEngine.Object)> targets = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
            var scene = SceneManager.GetActiveScene();
            if (scene != null && scene.IsValid())
            {
                targets.AddRange(GetSceneVariablesRenameTargets(scene, oldName, kind));
            }

            var activeReference = GraphWindow.activeReference;

            if (activeReference != null)
            {
                var refGraph = activeReference.graph;
                if (refGraph is FlowGraph graph)
                {
                    foreach (var unit in graph.units)
                    {
                        if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == kind && variableUnit.isDefined)
                        {
                            if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                            {
                                targets.Add((variableUnit, null));
                            }
                        }
                    }
                }
            }
            return targets;
        }

        public static void RenameApplicationVariables(string oldName, string newName)
        {
            EditorApplication.delayCall += () =>
            UpdateProjectVariables(oldName, newName, VariableKind.Application);
        }

        public static void RenameSavedVariables(string oldName, string newName)
        {
            EditorApplication.delayCall += () =>
            UpdateProjectVariables(oldName, newName, VariableKind.Saved);
        }

        private static bool UpdateVariable(UnifiedVariableUnit target, string newName)
        {
            if (target.name.hasValidConnection)
                return false;

            string oldValue = (string)target.defaultValues[target.name.key];

            if (oldValue == newName)
                return false;

            target.name.SetDefaultValue(newName);
            return true;
        }

        public static void UpdateAllObjectVariables(GameObject gameObject, string oldName, string newName, VariableKind kind = VariableKind.Object)
        {
            var group = Undo.GetCurrentGroup();
            foreach (var target in GetObjectVariablesRenameTargets(gameObject, oldName, kind))
            {
                if (target.Item1.name.hasValidConnection) continue;

                Undo.RecordObject(target.Item2, $"Renamed '{oldName}' variable to '{newName}'");

                target.Item1.name.SetDefaultValue(newName);
            }
            Undo.CollapseUndoOperations(group);
        }

        public static void UpdateAllSceneVariables(Scene scene, string oldName, string newName)
        {
            var group = Undo.GetCurrentGroup();
            foreach (var target in GetSceneVariablesRenameTargets(scene, oldName))
            {
                if (target.Item1.name.hasValidConnection) continue;

                Undo.RecordObject(target.Item2, $"Renamed '{oldName}' variable to '{newName}'");

                target.Item1.name.SetDefaultValue(newName);
            }
            Undo.CollapseUndoOperations(group);
        }
    }
}