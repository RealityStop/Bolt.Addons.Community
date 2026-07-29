using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class UnifiedVariableUnitWidget : UnitWidget<UnifiedVariableUnit>
    {
        #region Reflection Caching

        private static readonly FieldInfo CollectionField =
            typeof(VariableDeclarations).GetField("collection", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo SetNameMethod =
            typeof(VariableDeclaration).GetProperty("name", BindingFlags.Instance | BindingFlags.Public)?.GetSetMethod(true);

        #endregion

        private static readonly List<UnifiedVariableUnit> ActiveRenameTargets = new List<UnifiedVariableUnit>();
        private static UnifiedVariableUnit closestToMouse;

        private readonly List<(UnifiedVariableUnit, UnityEngine.Object)> renameTargets = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
        private readonly Func<Metadata, VariableNameInspector> nameInspectorConstructor;
        private readonly string controlName;

        private VariableDeclarationCollection collection;
        private VariableDeclarationCollection savedCollection;
        private GameObject storedObject;
        private VariableKind previousKind;

        private VariableNameInspector nameInspector;
        private string newProjectName;
        private string oldProjectName;
        private bool isRenaming;

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;

        public UnifiedVariableUnitWidget(FlowCanvas canvas, UnifiedVariableUnit unit) : base(canvas, unit)
        {
            controlName = $"{unit}_VariableNameInspector";

            nameInspectorConstructor = metadata => new VariableNameInspector(metadata, GetNameSuggestions, OnVariableRenamed, controlName);
        }

        public override void CachePosition()
        {
            base.CachePosition();

            if (unit.kind != previousKind)
            {
                previousKind = unit.kind;
                collection = null;
                savedCollection = null;
                storedObject = null;
            }

            if (collection == null)
            {
                ResolveVariableCollections();
            }
        }

        public override void DrawForeground()
        {
            base.DrawForeground();

            if (ActiveRenameTargets.Contains(unit))
            {
                GraphGUI.DrawDragAndDropPreviewLabel(new Vector2(edgePosition.x, outerPosition.yMax), "Renaming", typeof(string).Icon());
            }
        }

        public override void HandleInput()
        {
            if (ShouldStartRename())
            {
                ExecuteStartRename();
            }
            else if (ShouldEndRename())
            {
                ExecuteEndRename();
            }
            else if (!selection.Contains(unit))
            {
                isRenaming = false;
            }

            base.HandleInput();
        }

        public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            if (port == unit.name)
            {
                InspectorProvider.instance.Renew(ref nameInspector, metadata, nameInspectorConstructor);
                return nameInspector;
            }

            return base.GetPortInspector(port, metadata);
        }

        protected override IEnumerable<DropdownOption> contextOptions
        {
            get
            {
                foreach (var option in base.contextOptions)
                {
                    yield return option;
                }

                if (!unit.name.hasValidConnection && Flow.CanPredict(unit.name, reference))
                {
                    yield return new DropdownOption((Action)FindAll, "Find/All");
                    yield return new DropdownOption((Action)FindSetters, "Find/Setters");
                    yield return new DropdownOption((Action)FindGetters, "Find/Getters");
                }
            }
        }

        #region Variable Collection Handling

        private void ResolveVariableCollections()
        {
            switch (unit.kind)
            {
                case VariableKind.Graph:
                    collection = GetCollection(VisualScripting.Variables.Graph(reference));
                    break;

                case VariableKind.Object:
                    if (Flow.CanPredict(unit.@object, reference) && Flow.Predict(unit.@object, reference) is GameObject go)
                    {
                        if (go != null && storedObject != go)
                        {
                            storedObject = go;
                            collection = GetCollection(VisualScripting.Variables.Object(go));
                        }
                    }
                    break;

                case VariableKind.Scene:
                    if (reference.scene != null)
                        collection = GetCollection(VisualScripting.Variables.Scene(reference.scene));
                    break;

                case VariableKind.Application:
                    collection = GetCollection(VisualScripting.Variables.Application);
                    break;

                case VariableKind.Saved:
                    collection = GetCollection(VisualScripting.Variables.Saved);
                    savedCollection = GetCollection(SavedVariables.saved);
                    break;
            }
        }

        private static VariableDeclarationCollection GetCollection(VariableDeclarations declarations)
        {
            return declarations != null ? (VariableDeclarationCollection)CollectionField?.GetValue(declarations) : null;
        }

        #endregion

        #region Renaming Logic

        private void OnVariableRenamed(string oldName, string newName)
        {
            if (!isRenaming) return;

            switch (unit.kind)
            {
                case VariableKind.Graph:
                    RenameVariable(oldName, newName, VisualScripting.Variables.Graph(reference));
                    break;

                case VariableKind.Object:
                    if (storedObject != null)
                        RenameVariable(oldName, newName, VisualScripting.Variables.Object(storedObject));
                    break;

                case VariableKind.Scene:
                    if (reference.scene != null)
                        RenameVariable(oldName, newName, VisualScripting.Variables.Scene(reference.scene));
                    break;

                case VariableKind.Application:
                    newName = RenameVariable(oldName, newName, VisualScripting.Variables.Application);
                    newProjectName = newName;
                    break;

                case VariableKind.Saved:
                    newName = RenameVariable(oldName, newName, VisualScripting.Variables.Saved);
                    if (!Application.isPlaying)
                    {
                        newName = RenameVariable(oldName, newName, SavedVariables.saved);
                    }
                    newProjectName = newName;
                    break;
            }

            int group = Undo.GetCurrentGroup();
            foreach (var (targetUnit, targetObject) in renameTargets)
            {
                if (targetUnit.name.hasValidConnection) continue;

                if (targetObject != null)
                {
                    Undo.RecordObject(targetObject, $"Renamed '{oldName}' variable to '{newName}'");
                }

                targetUnit.name.SetDefaultValue(newName);
            }
            Undo.CollapseUndoOperations(group);

            if (GUI.GetNameOfFocusedControl() != controlName)
            {
                isRenaming = false;
                ActiveRenameTargets.Clear();
            }
        }

        private string RenameVariable(string oldName, string newName, VariableDeclarations declarations)
        {
            if (declarations == null || !declarations.IsDefined(oldName))
                return newName;

            var declaration = declarations.GetDeclaration(oldName);
            newName = EnsureUniqueName(declarations, newName);

            collection?.EditorRename(declaration, newName);
            SetNameMethod?.Invoke(declaration, new object[] { newName });

            return newName;
        }

        private static string EnsureUniqueName(VariableDeclarations declarations, string candidateName)
        {
            string baseName = string.IsNullOrEmpty(candidateName) ? "Unnamed Variable" : candidateName;
            string finalName = baseName;
            int counter = 1;

            while (declarations.IsDefined(finalName))
            {
                finalName = $"{baseName} ({counter++})";
            }

            return finalName;
        }

        private bool ShouldStartRename()
        {
            return !unit.name.hasValidConnection && e != null && e.keyCode == KeyCode.F2 && selection.Contains(unit);
        }

        private void ExecuteStartRename()
        {
            if (selection.Count(s => s is UnifiedVariableUnit) > 1)
            {
                if (closestToMouse == null || Vector2.Distance(unit.position, e.mousePosition) < Vector2.Distance(closestToMouse.position, e.mousePosition))
                {
                    closestToMouse = unit;
                }
            }
            else
            {
                closestToMouse = unit;
            }

            if (closestToMouse != null && closestToMouse != unit) return;

            if (IsSceneRequired() && reference.gameObject == null)
            {
                Debug.LogWarning(
                    $"[Rename Variables] The selected variable is an {unit.kind} variable inside an Asset. " +
                    $"{reference.rootObject.GetType().DisplayName()} does not have access to the scene this graph is used in."
                );
                return;
            }

            EditorGUI.FocusTextInControl(controlName);
            string currentName = unit.defaultValues[unit.name.key] as string;

            switch (unit.kind)
            {
                case VariableKind.Flow:
                    ActiveRenameTargets.Clear();
                    ActiveRenameTargets.AddRange(GraphUtility.GetFlowVariablesRenameTargets(unit, currentName, reference));
                    renameTargets.Clear();
                    renameTargets.AddRange(ActiveRenameTargets.Select<UnifiedVariableUnit, (UnifiedVariableUnit, UnityEngine.Object)>(t => (t, null)));
                    isRenaming = true;
                    break;

                case VariableKind.Graph:
                    ActiveRenameTargets.Clear();
                    ActiveRenameTargets.AddRange(GraphUtility.GetGraphVariablesRenameTargets(graph as FlowGraph, currentName));
                    renameTargets.Clear();
                    renameTargets.AddRange(ActiveRenameTargets.Select<UnifiedVariableUnit, (UnifiedVariableUnit, UnityEngine.Object)>(t => (t, null)));
                    isRenaming = true;
                    break;

                case VariableKind.Object:
                    if (Flow.CanPredict(unit.@object, reference) && Flow.Predict(unit.@object, reference) is GameObject go)
                    {
                        renameTargets.Clear();
                        renameTargets.AddRange(GraphUtility.GetObjectVariablesRenameTargets(reference, go, currentName));
                        ActiveRenameTargets.Clear();
                        ActiveRenameTargets.AddRange(renameTargets.Select(t => t.Item1));
                        isRenaming = true;
                    }
                    break;

                case VariableKind.Scene:
                    if (reference.scene != null && SceneVariables.InstantiatedIn(reference.scene.Value))
                    {
                        renameTargets.Clear();
                        renameTargets.AddRange(GraphUtility.GetSceneVariablesRenameTargets(reference, reference.scene, currentName));
                        ActiveRenameTargets.Clear();
                        ActiveRenameTargets.AddRange(renameTargets.Select(t => t.Item1));
                        isRenaming = true;
                    }
                    break;

                default:
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning($"[Rename Variables] Cannot rename all {unit.kind} variables while in play mode!");
                        break;
                    }
                    isRenaming = true;
                    renameTargets.Clear();
                    renameTargets.AddRange(GraphUtility.GetCurrentlyAccessibleProjectUnits(currentName, unit.kind));
                    ActiveRenameTargets.Clear();
                    ActiveRenameTargets.AddRange(renameTargets.Select(t => t.Item1));
                    oldProjectName = currentName;
                    break;
            }
        }

        private bool ShouldEndRename()
        {
            return isRenaming && (!selection.Contains(unit) ||
                   GUI.GetNameOfFocusedControl() != controlName ||
                   e.keyCode == KeyCode.Return ||
                   e.keyCode == KeyCode.Escape ||
                   !canvas.isMouseOver);
        }

        private void ExecuteEndRename()
        {
            isRenaming = false;
            ActiveRenameTargets.Clear();

            if (oldProjectName != null && newProjectName != null && oldProjectName != newProjectName)
            {
                if (unit.kind == VariableKind.Application || unit.kind == VariableKind.Saved)
                {
                    bool confirm = EditorUtility.DisplayDialog(
                        $"Update ALL {unit.kind} Variables?",
                        $"This will search ALL scenes and macros to update '{oldProjectName}' to '{newProjectName}'.\n\nThis operation is FINAL and cannot be undone!",
                        "Update All",
                        "Rename Only"
                    );

                    if (confirm)
                    {
                        if (unit.kind == VariableKind.Application)
                            GraphUtility.RenameApplicationVariables(oldProjectName, newProjectName);
                        else
                            GraphUtility.RenameSavedVariables(oldProjectName, newProjectName);
                    }
                }
            }

            oldProjectName = null;
            newProjectName = null;
        }

        private bool IsSceneRequired() => unit.kind == VariableKind.Object || unit.kind == VariableKind.Scene;

        #endregion

        #region Search Helpers

        private void FindAll() => OpenNodeFinder($"{{0}} [SetVariable: {unit.kind}] | {{0}} [GetVariable: {unit.kind}]");
        private void FindSetters() => OpenNodeFinder($"{{0}} [SetVariable: {unit.kind}]");
        private void FindGetters() => OpenNodeFinder($"{{0}} [GetVariable: {unit.kind}]");

        private void OpenNodeFinder(string querySuffix)
        {
            if (Flow.Predict(unit.name, reference) is string varName)
            {
                NodeFinderWindow.Open(string.Format(querySuffix, varName));
            }
        }

        private IEnumerable<string> GetNameSuggestions()
        {
            return EditorVariablesUtility.GetVariableNameSuggestions(unit.kind, reference);
        }

        #endregion
    }
}