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
        private bool isRenaming;
        private List<(UnifiedVariableUnit, UnityEngine.Object)> renameTargets = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
        private static List<UnifiedVariableUnit> targets = new List<UnifiedVariableUnit>();

        private static FieldInfo collectionField = typeof(VariableDeclarations).GetField("collection", BindingFlags.Instance | BindingFlags.NonPublic);
        private MethodInfo setNameMethod = typeof(VariableDeclaration).GetProperty("name", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);

        private VariableDeclarationCollection collection;
        private VariableDeclarationCollection savedCollection;
        private GameObject storedObject;

        private static UnifiedVariableUnit closestToMouse;

        private readonly string controlName;
        public override void CachePosition()
        {
            base.CachePosition();
            switch (unit.kind)
            {
                case VariableKind.Graph:
                    if (collection == null)
                        collection = (VariableDeclarationCollection)collectionField.GetValue(VisualScripting.Variables.Graph(reference));
                    break;
                case VariableKind.Object:
                    if (Flow.CanPredict(unit.@object, reference))
                    {
                        var value = Flow.Predict(unit.@object, reference);
                        if (value is GameObject @object)
                        {
                            if (@object != null && storedObject != @object)
                            {
                                storedObject = @object;
                                collection = (VariableDeclarationCollection)collectionField.GetValue(VisualScripting.Variables.Object(@object));
                            }
                        }
                    }
                    break;
                case VariableKind.Scene:
                    if (collection == null && reference.scene != null)
                        collection = (VariableDeclarationCollection)collectionField.GetValue(VisualScripting.Variables.Scene(reference.scene));
                    break;
                case VariableKind.Application:
                    if (collection == null)
                        collection = (VariableDeclarationCollection)collectionField.GetValue(VisualScripting.Variables.Application);
                    break;
                case VariableKind.Saved:
                    if (collection == null)
                        collection = (VariableDeclarationCollection)collectionField.GetValue(VisualScripting.Variables.Saved);
                    if (savedCollection == null)
                        savedCollection = (VariableDeclarationCollection)collectionField.GetValue(SavedVariables.saved);
                    break;
            }
        }

        private string newProjectName;
        private string oldProjectName;
        public UnifiedVariableUnitWidget(FlowCanvas canvas, UnifiedVariableUnit unit) : base(canvas, unit)
        {
            controlName = unit.ToString() + "_VariableNameInspector";
            nameInspectorConstructor = (metadata) => new VariableNameInspector(metadata, GetNameSuggestions, (oldName, newName) =>
            {
                if (isRenaming)
                {
                    switch (unit.kind)
                    {
                        case VariableKind.Graph:
                            {
                                var declarations = VisualScripting.Variables.Graph(reference);

                                if (declarations.IsDefined(oldName))
                                {
                                    var declaration = declarations.GetDeclaration(oldName);

                                    newName = OperateOnString(declarations, newName);

                                    collection.EditorRename(declaration, newName);
                                    setNameMethod.Invoke(declaration, new object[] { newName });
                                }
                            }
                            break;
                        case VariableKind.Object:
                            {
                                if (storedObject == null) break;

                                var declarations = VisualScripting.Variables.Object(storedObject);

                                if (declarations.IsDefined(oldName))
                                {
                                    var declaration = declarations.GetDeclaration(oldName);

                                    newName = OperateOnString(declarations, newName);

                                    collection.EditorRename(declaration, newName);
                                    setNameMethod.Invoke(declaration, new object[] { newName });
                                }
                            }
                            break;
                        case VariableKind.Scene:
                            {
                                if (reference.scene == null) break;

                                var declarations = VisualScripting.Variables.Scene(reference.scene);

                                if (declarations.IsDefined(oldName))
                                {
                                    var declaration = declarations.GetDeclaration(oldName);

                                    newName = OperateOnString(declarations, newName);

                                    collection.EditorRename(declaration, newName);
                                    setNameMethod.Invoke(declaration, new object[] { newName });
                                }
                            }
                            break;
                        case VariableKind.Application:
                            {
                                var declarations = VisualScripting.Variables.Application;

                                if (declarations.IsDefined(oldName))
                                {
                                    var declaration = declarations.GetDeclaration(oldName);

                                    newName = OperateOnString(declarations, newName);

                                    collection.EditorRename(declaration, newName);
                                    setNameMethod.Invoke(declaration, new object[] { newName });
                                }

                                newProjectName = newName;
                            }
                            break;
                        case VariableKind.Saved:
                            {
                                var mainDeclarations = VisualScripting.Variables.Saved;

                                if (mainDeclarations.IsDefined(oldName))
                                {
                                    var declaration = mainDeclarations.GetDeclaration(oldName);

                                    newName = OperateOnString(mainDeclarations, newName);

                                    collection.EditorRename(declaration, newName);
                                    setNameMethod.Invoke(declaration, new object[] { newName });
                                }

                                if (!Application.isPlaying)
                                {
                                    var saved = SavedVariables.saved;
                                    if (saved.IsDefined(oldName))
                                    {
                                        var declaration = saved.GetDeclaration(oldName);

                                        newName = OperateOnString(saved, newName);

                                        savedCollection.EditorRename(declaration, newName);
                                        setNameMethod.Invoke(declaration, new object[] { newName });
                                    }
                                }

                                newProjectName = newName;
                            }
                            break;
                    }

                    var group = Undo.GetCurrentGroup();
                    foreach (var target in renameTargets)
                    {
                        if (target.Item1.name.hasValidConnection) continue;

                        if (target.Item2 != null)
                            Undo.RecordObject(target.Item2, $"Renamed '{oldName}' variable to '{newName}'");

                        target.Item1.name.SetDefaultValue(newName);
                    }
                    Undo.CollapseUndoOperations(group);

                    if (GUI.GetNameOfFocusedControl() != controlName)
                    {
                        isRenaming = false;
                        targets.Clear();
                    }
                }
            }, controlName);
        }

        private string OperateOnString(VariableDeclarations declarations, string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                int counter = 1;

                var baseName = "Unnamed Variable";
                newName = baseName;
                while (declarations.IsDefined(newName))
                {
                    newName = $"{baseName} ({counter++})";
                }
            }
            else if (declarations.IsDefined(newName))
            {
                int counter = 1;

                var baseName = newName;
                newName = baseName;
                while (declarations.IsDefined(newName))
                {
                    newName = $"{baseName} ({counter++})";
                }
            }
            return newName;
        }

        public override void DrawForeground()
        {
            base.DrawForeground();
            if (targets.Contains(unit))
                GraphGUI.DrawDragAndDropPreviewLabel(new Vector2(edgePosition.x, outerPosition.yMax), "Renaming", typeof(string).Icon());
        }

        public override void HandleInput()
        {
            if (!unit.name.hasValidConnection && e != null && e.keyCode == KeyCode.F2 && selection.Contains(unit))
            {
                if (selection.Count(e => e is UnifiedVariableUnit) > 1)
                {
                    if (closestToMouse == null) closestToMouse = unit;
                    else if (Vector2.Distance(unit.position, e.mousePosition) < Vector2.Distance(closestToMouse.position, e.mousePosition))
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
                        $"{reference.rootObject.GetType().DisplayName()}'s do not have access to the scene this graph is used in. Rename the variable directly from the GameObject or Scene itself."
                    );
                    return;
                }

                EditorGUI.FocusTextInControl(controlName);
                switch (unit.kind)
                {
                    case VariableKind.Flow:
                        targets = GraphUtility.GetFlowVariablesRenameTargets(unit, unit.defaultValues[unit.name.key] as string, reference);
                        renameTargets = targets.Select<UnifiedVariableUnit, (UnifiedVariableUnit, UnityEngine.Object)>(t => (t, null)).ToList();
                        isRenaming = true;
                        break;
                    case VariableKind.Graph:
                        targets = GraphUtility.GetGraphVariablesRenameTargets(graph as FlowGraph, unit.defaultValues[unit.name.key] as string);
                        renameTargets = targets.Select<UnifiedVariableUnit, (UnifiedVariableUnit, UnityEngine.Object)>(t => (t, null)).ToList();
                        isRenaming = true;
                        break;
                    case VariableKind.Object:
                        if (Flow.CanPredict(unit.@object, reference))
                        {
                            var value = Flow.Predict(unit.@object, reference);
                            if (value is GameObject @object)
                            {
                                renameTargets = GraphUtility.GetObjectVariablesRenameTargets(reference, @object, unit.defaultValues[unit.name.key] as string);
                                targets = renameTargets.Select(t => t.Item1).ToList();
                                isRenaming = true;
                            }
                        }
                        break;
                    case VariableKind.Scene:
                        if (reference.scene != null && SceneVariables.InstantiatedIn(reference.scene.Value))
                        {
                            renameTargets = GraphUtility.GetSceneVariablesRenameTargets(reference, reference.scene, unit.defaultValues[unit.name.key] as string);
                            targets = renameTargets.Select(t => t.Item1).ToList();
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
                        renameTargets = GraphUtility.GetCurrentlyAccessibleProjectUnits(unit.defaultValues[unit.name.key] as string, unit.kind);
                        targets = renameTargets.Select(t => t.Item1).ToList();
                        oldProjectName = unit.defaultValues[unit.name.key] as string;
                        break;
                }
            }
            else if (isRenaming && (!selection.Contains(unit) ||
            GUI.GetNameOfFocusedControl() != controlName ||
            e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape))
            {
                isRenaming = false;
                targets.Clear();
                switch (unit.kind)
                {
                    case VariableKind.Application:
                        {
                            bool choice = oldProjectName != newProjectName && EditorUtility.DisplayDialog(
                                "Update ALL Application Variables?",
                                "This will go through ALL scenes and macros to find every Variable Unit "
                                + $"using {oldProjectName} and update it to {newProjectName}.\n\n"
                                + "This operation is FINAL and cannot be undone!",
                                "Update All",
                                "Rename Only"
                            );

                            if (choice)
                            {
                                GraphUtility.RenameApplicationVariables(oldProjectName, newProjectName);
                            }
                        }
                        break;
                    case VariableKind.Saved:
                        {
                            bool choice = oldProjectName != newProjectName && EditorUtility.DisplayDialog(
                                "Update ALL Saved Variables?",
                                "This will go through ALL scenes and macros to find every Variable Unit "
                                + $"using {oldProjectName} and update it to {newProjectName}.\n\n"
                                + "This operation is FINAL and cannot be undone!",
                                "Update All",
                                "Rename Only"
                            );

                            if (choice)
                            {
                                GraphUtility.RenameSavedVariables(oldProjectName, newProjectName);
                            }
                        }
                        break;
                }
                oldProjectName = null;
                newProjectName = null;
            }
            else if (!selection.Contains(unit))
            {
                isRenaming = false;
            }
            base.HandleInput();
        }

        private bool IsSceneRequired()
        {
            return unit.kind == VariableKind.Object || unit.kind == VariableKind.Scene;
        }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;

        private VariableNameInspector nameInspector;
        private Func<Metadata, VariableNameInspector> nameInspectorConstructor;

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

                if (!unit.name.hasValidConnection && !Flow.CanPredict(unit.name, reference))
                    yield break;

                yield return new DropdownOption((Action)FindAll, "Find/All");
                yield return new DropdownOption((Action)FindSetters, "Find/Setters");
                yield return new DropdownOption((Action)FindGetters, "Find/Getters");
            }
        }

        private void FindAll()
        {
            var value = Flow.Predict(unit.name, reference);
            if (value is string name)
                NodeFinderWindow.Open($"{name} [SetVariable: {unit.kind}] | {name} [GetVariable: {unit.kind}]");
        }

        private void FindSetters()
        {
            var value = Flow.Predict(unit.name, reference);
            if (value is string name)
                NodeFinderWindow.Open($"{name} [SetVariable: {unit.kind}]");
        }

        private void FindGetters()
        {
            var value = Flow.Predict(unit.name, reference);
            if (value is string name)
                NodeFinderWindow.Open($"{name} [GetVariable: {unit.kind}]");
        }

        private IEnumerable<string> GetNameSuggestions()
        {
            return EditorVariablesUtility.GetVariableNameSuggestions(unit.kind, reference);
        }
    }
}
