using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreviewWindow : EditorWindow
    {
        public static CSharpPreviewWindow instance;
        private Slider zoomSlider;
        private float zoomFactor = 1.0f;
        private bool showCodeWindow = true;
        private Label zoomLabel;
        private List<Label> labels = new List<Label>();

        public static Object asset;

        [MenuItem("Window/Community Addons/C# Preview")]
        public static void Open()
        {
            CSharpPreviewWindow window = GetWindow<CSharpPreviewWindow>();
            window.titleContent = new GUIContent("C# Preview");
            instance = window;
        }

        public void CreateGUI()
        {
            Selection.selectionChanged += ChangeSelection;
            var toolbar = new Toolbar();

            // Create the Zoom section
            var zoomContainer = new VisualElement
            {
                style =
        {
            flexDirection = FlexDirection.Row,
            alignItems = Align.Center,
            paddingLeft = 10
        }
            };
            var zoomTextLabel = new Label("Zoom:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginRight = 5 } };
            var zoomSlider = new Slider(0.5f, 2.0f)
            {
                value = zoomFactor,
                style = { width = Length.Percent(20), alignSelf = Align.Center }
            };
            var zoomLabel = new Label($"{zoomFactor:0.#}x") { style = { marginLeft = 5, alignSelf = Align.Center } };

            // Register a callback for when the slider value changes
            zoomSlider.RegisterValueChangedCallback(evt =>
            {
                zoomFactor = evt.newValue; // Update the zoom factor
                zoomLabel.text = $"{zoomFactor:0.#}x"; // Update the zoom label to reflect the new value
                foreach (var label in labels)
                {
                    label.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);
                }
            });

            zoomContainer.Add(zoomTextLabel);
            zoomContainer.Add(zoomSlider);
            zoomContainer.Add(zoomLabel);

            toolbar.Add(zoomContainer);

            // Toolbar Buttons with toolbar-like styling
            var compileButton = CreateToolbarButton("Compile", () => CompileCode());
            toolbar.Add(compileButton);

            var copyButton = CreateToolbarButton("Copy to Clipboard", CopyToClipboard);
            toolbar.Add(copyButton);

            var utilityButton = CreateToolbarButton("Utility Window", OpenUtilityWindow);
            toolbar.Add(utilityButton);

            var refreshButton = CreateToolbarButton("Refresh", () => UpdateCodeDisplay());
            toolbar.Add(refreshButton);

            var toggleButton = CreateToolbarButton(showCodeWindow ? "Settings" : "Preview", ToggleWindowMode);
            toolbar.Add(toggleButton);

            rootVisualElement.Add(toolbar);

            // Scroll view for code display
            var scrollView = new ScrollView
            {
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                style = { flexGrow = 1, paddingLeft = 10, paddingTop = 10, backgroundColor = new Color(0.15f, 0.15f, 0.15f) }
            };
            rootVisualElement.Add(scrollView);
            ChangeSelection();
        }

        private Button CreateToolbarButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.style.paddingLeft = 6;
            button.style.paddingRight = 6;
            button.style.marginLeft = 0;
            button.style.marginRight = 0;
            button.style.paddingTop = 4;
            button.style.paddingBottom = 4;
            button.style.backgroundColor = new Color(0, 0, 0, 0); // Default background color
            button.style.borderTopColor = new Color(0, 0, 0, 0);
            button.style.borderBottomColor = new Color(0, 0, 0, 0);
            button.style.borderLeftColor = new Color(0.1f, 0.1f, 0.1f);
            button.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f);
            button.style.borderTopWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 0;
            button.style.color = Color.white;
            button.style.borderTopLeftRadius = 0;
            button.style.borderBottomLeftRadius = 0;
            button.style.borderTopRightRadius = 0;
            button.style.borderBottomRightRadius = 0;

            // Hover effects
            var defaultBackgroundColor = button.style.backgroundColor.value;
            var hoverBackgroundColor = new Color(0.15f, 0.15f, 0.15f); // Darker color on hover

            button.RegisterCallback<MouseEnterEvent>(evt =>
            {
                button.style.backgroundColor = hoverBackgroundColor;
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.backgroundColor = defaultBackgroundColor;
            });

            return button;
        }

        private void CompileCode()
        {
            if (asset != null)
                AssetCompiler.CompileAsset(asset);
        }

        // Copy selected text to clipboard
        private void CopyToClipboard()
        {
            string outputToCopy;
            if (selectedLabels.Count > 0)
            {
                // Join text from selected labels
                outputToCopy = string.Join("\n", selectedLabels.Select(label => CodeUtility.RemoveAllSelectableTags(CodeUtility.RemoveCustomHighlights(RemoveColorTags(label.text)))));
            }
            else
            {
                // No selection: copy the entire code
                outputToCopy = CodeUtility.RemoveAllSelectableTags(CodeUtility.RemoveCustomHighlights(RemoveColorTags(LoadYourCode())));
            }
            EditorGUIUtility.systemCopyBuffer = outputToCopy;
        }


        private string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=#[0-9a-fA-F]{6,8}>|</color>", string.Empty, RegexOptions.Compiled);
        }

        private void OpenUtilityWindow()
        {
            UtilityWindow.Open();
        }

        private void ToggleWindowMode()
        {
            showCodeWindow = !showCodeWindow;
            UpdateCodeDisplay();
        }

        private void ChangeSelection()
        {
            if (Selection.activeObject is CodeAsset _asset)
            {
                asset = _asset;
                UpdateCodeDisplay();
            }
            else if (Selection.activeObject is ScriptGraphAsset _graphAsset)
            {
                asset = _graphAsset;
                UpdateCodeDisplay();
            }
        }

        public void UpdateCodeDisplay()
        {
            if (asset != null)
            {
                var code = LoadYourCode(); // Load your code based on the selected asset
                var scrollView = rootVisualElement.Q<ScrollView>(); // Get the existing scroll view
                DisplayCode(scrollView, code); // Update the displayed code
            }
        }

        private string ProcessOutput(string output)
        {
            if (output.Length > 0)
            {
                output = output.RemoveMarkdown();
                output = Regex.Replace(output, @"/\*(?!.*\(Recommendation\))", "<color=#CC3333>/*", RegexOptions.Compiled);
                output = output.Replace("*/", "*/</color>");
            }
            return output;
        }

        private void DisplayCode(ScrollView scrollView, string code)
        {
            scrollView.Clear();
            labels.Clear();

            var clickableRegions = CodeUtility.ExtractClickableRegions(code);
            var regionsByLine = clickableRegions
                .GroupBy(region => region.startLine)
                .ToDictionary(g => g.Key, g => g.ToList());

            var lines = CodeUtility.RemoveAllSelectableTags(ProcessOutput(code)).Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                // Line container with fixed flex layout
                var lineContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                // Line number label container
                // Line number label container
                var lineNumberContainer = new Label($"{i + 1}");
                lineNumberContainer.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);
                lineNumberContainer.style.width = 25; // Adjust width if necessary
                lineNumberContainer.style.unityTextAlign = TextAnchor.MiddleLeft;
                lineNumberContainer.style.color = Color.gray;
                lineNumberContainer.style.marginLeft = 0; // Remove any left margin
                lineNumberContainer.style.paddingLeft = 0; // Remove any left padding
                lineContainer.Add(lineNumberContainer);
                labels.Add(lineNumberContainer);


                // Code container for the actual code
                var codeContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1, marginLeft = 10 } };

                // Check for clickable regions on the line
                if (regionsByLine.TryGetValue(i, out var regions))
                {
                    AdjustLeadingWhitespacesForFirstRegion(lines[i], regions[0]);
                    foreach (var region in regions)
                    {
                        var label = CreateCodeLabel(region, i);
                        labels.Add(label);
                        codeContainer.Add(label);
                    }
                }
                else
                {
                    var label = CreateNonClickableLabel(lines[i]);
                    labels.Add(label);
                    codeContainer.Add(label);
                }

                // Add the code container to the line container and then to ScrollView
                lineContainer.Add(codeContainer);
                scrollView.Add(lineContainer);
            }
        }

        private void AdjustLeadingWhitespacesForFirstRegion(string line, ClickableRegion firstRegion)
        {
            int leadingWhitespaceLength = 0;

            while (leadingWhitespaceLength < line.Length && char.IsWhiteSpace(line[leadingWhitespaceLength]))
            {
                leadingWhitespaceLength++;
            }

            if (!firstRegion.code.StartsWith(line.AsSpan(0, leadingWhitespaceLength).ToString()))
            {
                firstRegion.code = line.Substring(0, leadingWhitespaceLength) + firstRegion.code;
            }
        }

        private void OnCodeRegionClicked(ClickableRegion region, int currentLine)
        {
            HandleClickableRegionClick(region.unitId, currentLine);
        }

        private List<Label> selectedLabels = new List<Label>();
        private Label lastSelectedLabel = null;

        // Create a non-clickable label
        private Label CreateNonClickableLabel(string text)
        {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            label.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);
            label.style.color = Color.white;
            label.style.backgroundColor = new Color(1, 1, 1, 0);
            RemovePaddingAndMargin(label);
            label.RegisterCallback<ClickEvent>(evt => SelectLabel(label, evt));
            return label;
        }

        // Create a clickable code label
        private Label CreateCodeLabel(ClickableRegion region, int currentLine)
        {
            var label = new Label(CodeUtility.RemoveAllSelectableTags(region.code));
            label.style.fontSize = Mathf.RoundToInt(14 * zoomFactor);
            label.style.unityFontStyleAndWeight = FontStyle.Normal;
            label.style.color = Color.white;
            label.style.backgroundColor = new Color(1, 1, 1, 0);
            RemovePaddingAndMargin(label);

            label.RegisterCallback<ClickEvent>(evt =>
            {
                SelectLabel(label, evt);
                OnCodeRegionClicked(region, currentLine);
            });

            return label;
        }

        private void RemovePaddingAndMargin(Label label)
        {
            label.style.paddingLeft = 0;
            label.style.paddingRight = 0;
            label.style.paddingTop = 0;
            label.style.paddingBottom = 0;
            label.style.marginLeft = 0;
            label.style.marginRight = 0;
            label.style.marginTop = 0;
            label.style.marginBottom = 0;
        }

        // Handle label selection with Ctrl and Shift keys
        private void SelectLabel(Label label, ClickEvent evt)
        {
            if (evt.ctrlKey)
            {
                // Ctrl + Click: Add/remove label from selection
                if (selectedLabels.Contains(label))
                {
                    DeselectLabel(label);
                }
                else
                {
                    AddLabelToSelection(label);
                }
            }
            else if (evt.shiftKey && lastSelectedLabel != null)
            {
                // Shift + Click: Select a range
                SelectRange(label);
            }
            else
            {
                // No modifier: clear selection and select only the clicked label
                ClearSelection();
                AddLabelToSelection(label);
            }

            lastSelectedLabel = label;
        }

        // Add label to the selection
        private void AddLabelToSelection(Label label)
        {
            label.style.backgroundColor = new Color(0.25f, 0.5f, 0.8f, 0.3f); // Highlight color

            selectedLabels.Add(label);
        }

        // Deselect a label
        private void DeselectLabel(Label label)
        {
            label.style.backgroundColor = new Color(1, 1, 1, 0); // Default background
            selectedLabels.Remove(label);
        }

        // Clear all selected labels
        private void ClearSelection()
        {
            foreach (var selectedLabel in selectedLabels)
            {
                selectedLabel.style.backgroundColor = new Color(1, 1, 1, 0);
            }
            selectedLabels.Clear();
        }

        // Select a range between lastSelectedLabel and current label
        private void SelectRange(Label label)
        {
            int startIndex = labels.IndexOf(lastSelectedLabel);
            int endIndex = labels.IndexOf(label);

            if (startIndex > endIndex)
            {
                int temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                var labelInRange = labels[i];
                if (!selectedLabels.Contains(labelInRange))
                {
                    AddLabelToSelection(labelInRange);
                }
            }
        }

        private string LoadYourCode()
        {
            return CodeGenerator.GetSingleDecorator(asset).Generate(0).RemoveMarkdown();
        }

        private void HandleClickableRegionClick(string unitId, int line)
        {
            var code = CodeGenerator.GetSingleDecorator(asset);
            if (GraphWindow.active?.reference != null && GraphWindow.active.context.graph is FlowGraph)
            {
                var reference = GraphWindow.active.reference.isRoot ? GraphWindow.active.reference : GraphWindow.active.reference.root.GetReference() as GraphReference;
                GraphWindow.active.Focus();

                List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                if (reference.macro != null && reference.macro is MethodDeclaration or ConstructorDeclaration or FieldDeclaration)
                {
                    if (reference.macro is MethodDeclaration methodDeclaration)
                    {
                        ProcessMethodDeclaration(methodDeclaration, units);
                    }
                    else if (reference.macro is ConstructorDeclaration constructorDeclaration)
                    {
                        ProcessConstructorDeclaration(constructorDeclaration, units);
                    }
                    else if (reference.macro is FieldDeclaration fieldDeclaration)
                    {
                        ProcessFieldDeclaration(fieldDeclaration, units);
                    }
                }
                else
                {
                    units = TraverseFlowGraph(reference).ToList();
                }
                var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                if (ordered.Count() > 0 && ordered.Any(selectable => selectable.result.Item2 != null))
                {
                    ordered = ordered.Where(selectable => selectable.result.Item2 != null);
                }
                if (units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString()).Count() > 0)
                {
                    if (!GraphWindow.active.reference.isRoot && reference != ordered.First().result.Item1)
                    {
                        GraphWindow.active.reference = GraphWindow.active.reference.root.GetReference() as GraphReference;
                    }
                    var path = GetUnitPath(ordered.First().result.Item1);
                    if (GraphWindow.active.reference != ordered.First().result.Item1)
                    {
                        foreach (var item in path)
                        {
                            if (item.Item2 != null)
                            {
                                GraphWindow.active.reference = GraphWindow.active.reference.ChildReference(item.Item2, false);
                            }
                            else if (item.Item1.isRoot)
                            {
                                GraphWindow.active.reference = item.Item1;
                            }
                        }
                    }
                    if (ordered.First().result.Item2 != null)
                    {
                        var canvas = GraphWindow.active.context.canvas as FlowCanvas;
                        GraphWindow.active.context.BeginEdit();
                        canvas.ViewElements(new List<Unit>() { ordered.First().result.Item2 });
                        GraphWindow.active.context.EndEdit();
                    }
                }
            }
            else
            {
                if (code is ClassAssetGenerator classAssetGenerator)
                {
                    if (classAssetGenerator.Data != null)
                    {
                        List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                        foreach (var constructorDeclaration in classAssetGenerator.Data.constructors)
                        {
                            ProcessConstructorDeclaration(constructorDeclaration, units);
                        }

                        foreach (var fieldDeclaration in classAssetGenerator.Data.variables)
                        {
                            ProcessFieldDeclaration(fieldDeclaration, units);
                        }

                        foreach (var methodDeclaration in classAssetGenerator.Data.methods)
                        {
                            ProcessMethodDeclaration(methodDeclaration, units);
                        }
                        var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                        GraphWindow.OpenActive(ordered.First().result.Item1);
                        HandleClickableRegionClick(unitId, line);
                    }
                }
                else if (code is StructAssetGenerator structAssetGenerator)
                {
                    if (structAssetGenerator.Data != null)
                    {
                        List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                        foreach (var constructorDeclaration in structAssetGenerator.Data.constructors)
                        {
                            ProcessConstructorDeclaration(constructorDeclaration, units);
                        }

                        foreach (var fieldDeclaration in structAssetGenerator.Data.variables)
                        {
                            ProcessFieldDeclaration(fieldDeclaration, units);
                        }

                        foreach (var methodDeclaration in structAssetGenerator.Data.methods)
                        {
                            ProcessMethodDeclaration(methodDeclaration, units);
                        }
                        var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                        GraphWindow.OpenActive(ordered.First().result.Item1);
                        HandleClickableRegionClick(unitId, line);
                    }
                }
                else if (code is ScriptGraphAssetGenerator graphAssetGenerator)
                {
                    if (graphAssetGenerator.Data != null)
                    {
                        List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                        units = TraverseFlowGraph(graphAssetGenerator.Data.GetReference() as GraphReference).ToList();
                        var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                        GraphWindow.OpenActive(ordered.First().result.Item1);
                    }
                }
            }
        }

        private void ProcessMethodDeclaration(MethodDeclaration methodDeclaration, List<(GraphReference, Unit)> units)
        {
            if (methodDeclaration.classAsset != null)
            {
                AddUnitsFromClassAsset(methodDeclaration.classAsset, units);
            }
            else if (methodDeclaration.structAsset != null)
            {
                AddUnitsFromStructAsset(methodDeclaration.structAsset, units);
            }
        }

        private void ProcessConstructorDeclaration(ConstructorDeclaration constructorDeclaration, List<(GraphReference, Unit)> units)
        {
            if (constructorDeclaration.classAsset != null)
            {
                AddUnitsFromClassAsset(constructorDeclaration.classAsset, units);
            }
            else if (constructorDeclaration.structAsset != null)
            {
                AddUnitsFromStructAsset(constructorDeclaration.structAsset, units);
            }
        }

        private void ProcessFieldDeclaration(FieldDeclaration fieldDeclaration, List<(GraphReference, Unit)> units)
        {
            if (fieldDeclaration.classAsset != null)
            {
                AddUnitsFromClassAsset(fieldDeclaration.classAsset, units);
            }
            else if (fieldDeclaration.structAsset != null)
            {
                AddUnitsFromStructAsset(fieldDeclaration.structAsset, units);
            }
        }

        private void AddUnitsFromClassAsset(ClassAsset classAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in classAsset.methods)
                units.AddRange(TraverseFlowGraph(method.GetReference() as GraphReference));

            foreach (var constructor in classAsset.constructors)
                units.AddRange(TraverseFlowGraph(constructor.GetReference() as GraphReference));

            foreach (var variable in classAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(TraverseFlowGraph(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(TraverseFlowGraph(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        private void AddUnitsFromStructAsset(StructAsset structAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in structAsset.methods)
                units.AddRange(TraverseFlowGraph(method.GetReference() as GraphReference));

            foreach (var constructor in structAsset.constructors)
                units.AddRange(TraverseFlowGraph(constructor.GetReference() as GraphReference));

            foreach (var variable in structAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(TraverseFlowGraph(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(TraverseFlowGraph(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        List<(GraphReference, SubgraphUnit)> GetUnitPath(GraphReference reference)
        {
            List<(GraphReference, SubgraphUnit)> nodePath = new List<(GraphReference, SubgraphUnit)>() { (reference, !reference.isRoot ? reference.GetParent<SubgraphUnit>() : null) };
            while (reference.ParentReference(false) != null)
            {
                reference = reference.ParentReference(false);
                nodePath.Add((reference, !reference.isRoot ? reference.GetParent<SubgraphUnit>() : null));
            }
            nodePath.Reverse();
            return nodePath;
        }

        IEnumerable<(GraphReference, Unit)> TraverseFlowGraph(GraphReference graphReference)
        {
            var flowGraph = graphReference.graph as FlowGraph;
            if (flowGraph == null) yield break;
            var units = flowGraph.units;
            foreach (var element in units)
            {
                var unit = element as Unit;
                switch (unit)
                {
                    case SubgraphUnit subgraphUnit:
                        {
                            var subGraph = subgraphUnit.nest.embed ?? subgraphUnit.nest.graph;
                            if (subGraph == null) continue;
                            yield return (graphReference, subgraphUnit);
                            var childReference = graphReference.ChildReference(subgraphUnit, false);
                            foreach (var item in TraverseFlowGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    case StateUnit stateUnit:
                        {
                            var stateGraph = stateUnit.nest.embed ?? stateUnit.nest.graph;
                            if (stateGraph == null) continue;
                            // find state graph.
                            var childReference = graphReference.ChildReference(stateUnit, false);
                            foreach (var item in TraverseStateGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    default:
                        yield return (graphReference, unit);
                        break;
                }
            }
        }

        IEnumerable<(GraphReference, Unit)> TraverseStateGraph(GraphReference graphReference)
        {
            var stateGraph = graphReference.graph as StateGraph;
            if (stateGraph == null) yield break;

            foreach (var state in stateGraph.states)
            {
                switch (state)
                {
                    case FlowState flowState:
                        {
                            var graph = flowState.nest.embed ?? flowState.nest.graph;

                            if (graph == null) continue;
                            var childReference = graphReference.ChildReference(flowState, false);
                            foreach (var item in TraverseFlowGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    case SuperState superState:
                        {
                            var subStateGraph = superState.nest.embed ?? superState.nest.graph;
                            if (subStateGraph == null) continue;
                            var childReference = graphReference.ChildReference(superState, false);
                            foreach (var item in TraverseStateGraph(childReference))
                            {
                                yield return item;
                            }

                            break;
                        }
                    case AnyState:
                        continue;
                }
            }

            foreach (var transition in stateGraph.transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;
                var graph = flowStateTransition.nest.embed ?? flowStateTransition.nest.graph;
                if (graph == null) continue;
                var childReference = graphReference.ChildReference(flowStateTransition, false);
                foreach (var item in TraverseFlowGraph(childReference))
                {
                    yield return item;
                }
            }
        }

        private GraphReference GetFirstReference()
        {
            if (asset is ClassAsset classAsset)
            {
                var variables = classAsset.variables.Where(variable => variable.isProperty);
                if (classAsset.constructors.Count > 0)
                {
                    return classAsset.constructors[0].GetReference() as GraphReference;
                }
                else if (variables.Count() > 0)
                {
                    var first = variables.First();
                    if (first.get)
                    {
                        return first.getter.GetReference() as GraphReference;
                    }
                    else if (first.set)
                    {
                        return first.setter.GetReference() as GraphReference;
                    }
                }
                else if (classAsset.methods.Count > 0)
                {
                    return classAsset.methods[0].GetReference() as GraphReference;
                }
            }
            else if (asset is StructAsset structAsset)
            {
                var variables = structAsset.variables.Where(variable => variable.isProperty);
                if (structAsset.constructors.Count > 0)
                {
                    return structAsset.constructors[0].GetReference() as GraphReference;
                }
                else if (variables.Count() > 0)
                {
                    var first = variables.First();
                    if (first.get)
                    {
                        return first.getter.GetReference() as GraphReference;
                    }
                    else if (first.set)
                    {
                        return first.setter.GetReference() as GraphReference;
                    }
                }
                else if (structAsset.methods.Count > 0)
                {
                    return structAsset.methods[0].GetReference() as GraphReference;
                }
            }
            return null;
        }

    }
}