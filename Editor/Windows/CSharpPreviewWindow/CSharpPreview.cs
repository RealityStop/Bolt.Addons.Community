using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Linq;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [Serializable]
    [Obsolete("CSharp preview functionality was moved to 'CSharpPreviewWindow'")]
    public sealed class CSharpPreview
    {
        public string output = string.Empty;

        [SerializeField]
        private Vector2 scrollPosition;

        #region Colors

        [SerializeField]
        public static Color VariableColor;

        [SerializeField]
        public static Color StringColor;

        [SerializeField]
        public static Color NumericColor;

        [SerializeField]
        public static Color ConstructColor;

        [SerializeField]
        public static Color TypeColor;

        [SerializeField]
        public static Color EnumColor;

        [SerializeField]
        public static Color InterfaceColor;

        #endregion
        const string path = "Assets/Unity.VisualScripting.Community.Generated/";
        private static CSharpPreviewSettings Settings;

        public static Color background => HUMColor.Grey(0.1f);
        public static Color Settingsbackground => HUMColor.Grey(0.2f);

        private GUIStyle readOnlyTextStyle;

        [SerializeField]
        private bool shouldRefresh = true;
        public bool shouldRepaint = true;

        [SerializeReference]
        private ICodeGenerator _code;
        public ICodeGenerator code
        {
            get => _code;
            set
            {
                _code = value;
                RefreshIfNeeded();
            }
        }

        [SerializeField]
        private float zoomFactor = 0.7f;
        [SerializeField]
        private bool ShowCodeWindow = true;

        private GUIStyle TextStyle
        {
            get
            {
                if (readOnlyTextStyle == null)
                {
                    readOnlyTextStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                    {
                        richText = true,
                        stretchWidth = true,
                        wordWrap = false,
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                    };
                }

                readOnlyTextStyle.fontSize = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight * zoomFactor);

                return readOnlyTextStyle;
            }
        }

        public void RefreshIfNeeded()
        {
            if (shouldRefresh)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            shouldRefresh = true;
            Settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");
            // CSharpPreviewWindow.instance?.Repaint();
        }

        private string searchQuery = "";
        public void DrawLayout()
        {
            LoadSettings();
            if (ShowCodeWindow)
                HandleZoomEvents();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.enabled = ShowCodeWindow;

            GUILayout.Label("Zoom", LudiqStyles.toolbarLabel);
            float newZoomFactor = GUILayout.HorizontalSlider(zoomFactor, 0.5f, 2.0f, GUILayout.Width(150));
            GUILayout.Label(newZoomFactor.ToString("0.#") + "x", LudiqStyles.toolbarLabel);

            if (!Mathf.Approximately(newZoomFactor, zoomFactor))
            {
                zoomFactor = newZoomFactor;
                shouldRefresh = true;
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(code == null || !code.CanCompile);

            //TODO: Add search feature
            // GUILayout.Label("Search:");
            // searchQuery = EditorGUILayout.TextField(searchQuery);

            if (GUILayout.Button("Compile", LudiqStyles.toolbarButton, GUILayout.Width(80)))
            {
                //AssetCompiler.CompileAsset(CSharpPreviewWindow.asset);
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy to Clipboard", LudiqStyles.toolbarButton, GUILayout.Width(150)))
            {
                var result = output;
                if (selectedRegions.Count > 0)
                {
                    result = "";
                    foreach (var region in selectedRegions)
                    {
                        result += region.code;
                    }
                }
                var outputToCopy = CodeUtility.RemoveAllSelectableTags(CodeUtility.RemoveCustomHighlights(RemoveColorTags(result)));
                EditorGUIUtility.systemCopyBuffer = outputToCopy;
            }

            if (GUILayout.Button("Utility Window", LudiqStyles.toolbarButton, GUILayout.Width(110)))
            {
                UtilityWindow.Open();
            }

            if (GUILayout.Button("Refresh", LudiqStyles.toolbarButton, GUILayout.Width(80)))
            {
                shouldRefresh = true;
            }

            GUI.enabled = true;

            if (!ShowCodeWindow)
            {
                if (GUILayout.Button("Preview", LudiqStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ShowCodeWindow = true;
                }
            }
            else
            {
                if (GUILayout.Button("Settings", LudiqStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ShowCodeWindow = false;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (ShowCodeWindow)
            {
                CodeWindow();
            }
            else
            {
                SettingsWindow();
            }
        }

        private void HandleZoomEvents()
        {
            Event e = Event.current;

            if (e.type == EventType.ScrollWheel && e.control)
            {
                float zoomChange = e.delta.y * -0.05f;
                float newZoomFactor = Mathf.Clamp(zoomFactor + zoomChange, 0.5f, 2.0f);

                if (!Mathf.Approximately(newZoomFactor, zoomFactor))
                {
                    zoomFactor = newZoomFactor;
                    shouldRefresh = true;
                    e.Use();
                }
            }
        }

        private string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=#[0-9a-fA-F]{6,8}>|</color>", string.Empty, RegexOptions.Compiled);
        }

        private void CodeWindow()
        {
            if (shouldRefresh)
            {
                if (code != null)
                {
                    output = code.Generate(0);
                    output = ProcessOutput(output); // Call a method that handles all regex replacements.
                }

                shouldRefresh = false;
                shouldRepaint = true;
            }

            DrawCodeOutput();
        }

        private string ProcessOutput(string output)
        {
            if (output.Length > 0)
            {
                output = Regex.Replace(output, @"/\*(?!.*\(Recommendation\))", "<color=#CC3333>/*", RegexOptions.Compiled);
                output = output.Replace("*/", "*/</color>");
                output = output.RemoveMarkdown();
            }
            return output;
        }

        private Dictionary<int, List<ClickableRegion>> regionCache = new Dictionary<int, List<ClickableRegion>>();
        private string lastOutput = "";
        private ClickableRegion firstSelectedRegion = null; // Track the first clicked region
        private List<ClickableRegion> selectedRegions = new(); // List of selected regions


        private GUIStyle _verticalStyle;
        private GUIStyle verticalStyle
        {
            get
            {
                if (_verticalStyle == null)
                {
                    _verticalStyle = new GUIStyle(GUI.skin.box);
                    _verticalStyle.normal.background = HUMColor.CacheTexture(HUMColor.Grey(0.15f));
                    _verticalStyle.stretchWidth = true;
                    _verticalStyle.stretchHeight = true;
                    _verticalStyle.border = new RectOffset();
                    _verticalStyle.margin = new RectOffset();
                }
                return _verticalStyle;
            }
        }
        private void DrawCodeOutput()
        {
            //if (!CSharpPreviewWindow.instance.IsFocused())
                selectedRegions.Clear();

            if (lastOutput != output)
            {
                lastOutput = output;
                regionCache.Clear();
            }

            if (GraphWindow.active?.context?.graph is FlowGraph flowGraph)
            {
                foreach (var item in GraphWindow.active.context.canvas.selection)
                {
                    output = CodeUtility.HighlightCode(output, item.ToString());
                }
            }

            var clickableRegions = CodeUtility.ExtractAndPopulateClickableRegions(output);
            var plainOutput = CodeUtility.RemoveAllSelectableTags(output);
            var lines = plainOutput.Split(new[] { '\n' }, StringSplitOptions.None);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical(verticalStyle);
            var lineCount = lines.Length;
            for (int currentLine = 0; currentLine < lineCount; currentLine++)
            {
                string line = lines[currentLine];
                try
                {
                    GUILayout.BeginHorizontal(TextStyle);
                    GUILayout.Label(currentLine.ToString(), TextStyle, GUILayout.Width(40));

                    bool isClickable = RenderCachedRegions(currentLine, line, clickableRegions);

                    if (!isClickable)
                    {
                        RenderPlainLine(line);
                    }
                }
                catch
                {
                    // Silently catch errors
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private bool RenderCachedRegions(int currentLine, string line, List<ClickableRegion> clickableRegions)
        {
            bool isClickable = false;

            if (regionCache.TryGetValue(currentLine, out var cachedRegions) && cachedRegions.Count > 0)
            {
                isClickable = true;
                AdjustLeadingWhitespacesForFirstRegion(line, cachedRegions[0]);
                int index = 0;
                foreach (var region in cachedRegions)
                {
                    if (MatchesSearchQuery(region.code))
                        RenderButtonWithHighlight(region, currentLine, searchQuery);
                    else
                    {
                        RenderButton(region, currentLine);
                    }
                    index++;
                }
            }
            else
            {
                foreach (var region in clickableRegions)
                {
                    if (IsWithinLineRange(region, currentLine))
                    {
                        AddRegionToCache(currentLine, region);
                        isClickable = true;

                        if (MatchesSearchQuery(region.code))
                            RenderButtonWithHighlight(region, currentLine, searchQuery);
                        else
                            RenderButton(region, currentLine);
                    }
                }
            }

            return isClickable;
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

        private bool IsWithinLineRange(ClickableRegion region, int line)
        {
            return line >= region.startLine && line <= region.endLine;
        }

        private void RenderPlainLine(string line)
        {
            if (searchQuery.Length > 0 && line.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            {
                string highlightedLine = VisualScripting.SearchUtility.HighlightQuery(line, searchQuery, "<color=yellow>", "</color>");
                GUILayout.Label(highlightedLine ?? "", TextStyle, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(line ?? "", TextStyle, GUILayout.ExpandWidth(false));
            }
        }

        private bool MatchesSearchQuery(string text)
        {
            return searchQuery.Length == 0 ||
                   CodeUtility.RemoveAllSelectableTags(text).Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
        }

        private void AddRegionToCache(int line, ClickableRegion region)
        {
            if (!regionCache.ContainsKey(line))
                regionCache[line] = new List<ClickableRegion>();

            regionCache[line].Add(region);
        }

        private void RenderButton(ClickableRegion region, int currentLine)
        {
            string cleanCode = CodeUtility.RemoveAllSelectableTags(region.code);
            GUIStyle lineStyle = selectedRegions.Contains(region) ? GetSelectedLineStyle() : TextStyle;

            if (GUILayout.Button(cleanCode ?? "", lineStyle, GUILayout.Width(lineStyle.CalcSize(new GUIContent(cleanCode)).x)))
            {
                HandleButtonClick(region, currentLine);
            }
        }

        private void HandleButtonClick(ClickableRegion region, int currentLine)
        {
            if (Event.current.shift && firstSelectedRegion != null)
            {
                SelectRange(firstSelectedRegion, region, CodeUtility.ExtractAndPopulateClickableRegions(output));
            }
            else if (Event.current.control)
            {
                firstSelectedRegion = region;
                if (selectedRegions.Contains(region))
                {
                    selectedRegions.Remove(region);
                }
                else
                {
                    selectedRegions.Add(region);
                }
            }
            else
            {
                selectedRegions.Clear();
                selectedRegions.Add(region);
                firstSelectedRegion = region;
            }
            HandleClickableRegionClick(region.unitId, currentLine);
        }

        private void RenderButtonWithHighlight(ClickableRegion region, int currentLine, string query)
        {
            // Part of search logic
            //string cleanCode = SearchUtility.HighlightQuery(CodeUtility.RemoveAllSelectableTags(region.code), query, "<color=yellow>", "</color>");

            string cleanCode = CodeUtility.RemoveAllSelectableTags(region.code);
            GUIStyle lineStyle = selectedRegions.Contains(region) ? GetSelectedLineStyle() : TextStyle;
            try
            {
                if (GUILayout.Button(cleanCode ?? "", lineStyle, GUILayout.Width(lineStyle.CalcSize(new GUIContent(cleanCode)).x)))
                {
                    if (Event.current.shift && firstSelectedRegion != null) // Shift-click logic
                    {
                        SelectRange(firstSelectedRegion, region, CodeUtility.ExtractAndPopulateClickableRegions(output));
                    }
                    else if (Event.current.control) // Ctrl-click logic
                    {
                        firstSelectedRegion = region;
                        if (selectedRegions.Contains(region))
                        {
                            selectedRegions.Remove(region); // Deselect if already selected
                        }
                        else
                        {
                            selectedRegions.Add(region); // Add to selection
                        }
                    }
                    else // Regular click (no modifiers)
                    {
                        selectedRegions.Clear();
                        selectedRegions.Add(region);
                        firstSelectedRegion = region; // Track the first region for shift-click
                    }
                    HandleClickableRegionClick(region.unitId, currentLine);
                }
            }
            catch
            {
                // Silently catch errors
            }
        }

        private void SelectRange(ClickableRegion start, ClickableRegion end, List<ClickableRegion> allRegions)
        {
            selectedRegions.Clear();

            bool inRange = false;
            foreach (var region in allRegions)
            {
                if (region == start || region == end)
                {
                    inRange = !inRange;
                    selectedRegions.Add(region);
                }
                else if (inRange)
                {
                    selectedRegions.Add(region);
                }
            }
        }

        private void HandleClickableRegionClick(string unitId, int line)
        {
            if (GraphWindow.active?.reference != null && GraphWindow.active.context.graph is FlowGraph)
            {
                var reference = GraphWindow.active.reference.isRoot ? GraphWindow.active.reference : GraphWindow.active.reference.root.GetReference() as GraphReference;
                GraphWindow.active.Focus();
                //CSharpPreviewWindow.instance.Focus();

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
                        canvas.UpdateViewport();
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

            shouldRefresh = true;
        }

        private void ProcessMethodDeclaration(MethodDeclaration methodDeclaration, List<(GraphReference, Unit)> units)
        {
            if (methodDeclaration.parentAsset is ClassAsset classAsset)
            {
                AddUnitsFromClassAsset(classAsset, units);
            }
            else if (methodDeclaration.parentAsset is StructAsset structAsset)
            {
                AddUnitsFromStructAsset(structAsset, units);
            }
        }

        private void ProcessConstructorDeclaration(ConstructorDeclaration constructorDeclaration, List<(GraphReference, Unit)> units)
        {
            if (constructorDeclaration.parentAsset is ClassAsset classAsset)
            {
                AddUnitsFromClassAsset(classAsset, units);
            }
            else if (constructorDeclaration.parentAsset is StructAsset structAsset)
            {
                AddUnitsFromStructAsset(structAsset, units);
            }
        }

        private void ProcessFieldDeclaration(FieldDeclaration fieldDeclaration, List<(GraphReference, Unit)> units)
        {
            if (fieldDeclaration.parentAsset is ClassAsset classAsset)
            {
                AddUnitsFromClassAsset(classAsset, units);
            }
            else if (fieldDeclaration.parentAsset is StructAsset structAsset)
            {
                AddUnitsFromStructAsset(structAsset, units);
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

        private GUIStyle GetSelectedLineStyle()
        {
            var style = new GUIStyle(TextStyle)
            {
                normal = { background = Texture2D.whiteTexture },
                fontStyle = FontStyle.Bold,
                stretchWidth = false
            };

            style.normal.background = CreateTexture(1, 1, HUMColor.Grey(0.1f));
            return style;
        }

        private Texture2D CreateTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
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

        private void SettingsWindow()
        {
            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, () =>
            {
                HUMEditor.Vertical().Box(Settingsbackground, Color.black, action: () =>
                {
                    var windowTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        fontSize = 20
                    };

                    var sectionTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        fontSize = 15
                    };

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("C# Preview Settings", windowTitleStyle);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Label("Code Generation Settings", sectionTitleStyle);
                    GUILayout.Space(10);

                    Rect labelRect = GUILayoutUtility.GetLastRect();

                    EditorGUILayout.BeginHorizontal();
                    DrawLabelWithTooltip("Show Subgraph Comments", "A comment where the Subgraph and Port are being generated.", labelRect, GUILayout.Width(200));
                    Settings.showSubgraphComment = EditorGUILayout.Toggle(Settings.showSubgraphComment);
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    GUILayout.Label("Text Colors", sectionTitleStyle);
                    GUILayout.Space(10);
                    DrawColorField("Variable Color", ref VariableColor, "#26CCCC");
                    DrawColorField("String Color", ref StringColor, "#CC8833");
                    DrawColorField("Numeric Color", ref NumericColor, "#DDFFBB");
                    DrawColorField("Construct Color", ref ConstructColor, "#4488FF");
                    DrawColorField("Type Color", ref TypeColor, "#33EEAA");
                    DrawColorField("Enum Color", ref EnumColor, "#FFFFBB");
                    DrawColorField("Interface Color", ref InterfaceColor, "#DDFFBB");

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Save Settings", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        SaveSettings();
                    }

                }, stretchHorizontal: true, stretchVertical: true);
            });
        }


        private void DrawLabelWithTooltip(string labelText, string tooltipText, Rect labelRect, params GUILayoutOption[] options)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            Color normalColor = Color.white;
            Color hoverColor = new Color(0.8f, 0.8f, 1.0f);
            labelStyle.normal.textColor = normalColor;
            bool isMouseOverLabel = labelRect.Contains(Event.current.mousePosition);

            if (isMouseOverLabel)
            {
                labelStyle.normal.textColor = hoverColor;
            }
            GUIContent labelContent = new GUIContent(labelText, tooltipText);
            GUILayout.Label(labelContent, labelStyle, options);
        }

        private void DrawColorField(string label, ref Color color, string defaultHex)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label + ":", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            color = EditorGUILayout.ColorField(color, GUILayout.ExpandWidth(true));
            CodeBuilder.VariableColor = color.ToHexString();
            if (GUILayout.Button("Default", GUILayout.Width(60)))
            {
                UnityEngine.ColorUtility.TryParseHtmlString(defaultHex, out color);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        private void SaveSettings()
        {
            Settings.NumericColor = NumericColor;
            Settings.EnumColor = EnumColor;
            Settings.ConstructColor = ConstructColor;
            Settings.VariableColor = VariableColor;
            Settings.StringColor = StringColor;
            Settings.InterfaceColor = InterfaceColor;
            Settings.TypeColor = TypeColor;
            Settings.showSubgraphComment = CSharpPreviewSettings.ShouldShowSubgraphComment;
            Settings.SaveAndDirty();
        }

        public void LoadSettings()
        {
            CodeBuilder.VariableColor = Settings.VariableColor.ToHexString();
            CodeBuilder.StringColor = Settings.StringColor.ToHexString();
            CodeBuilder.NumericColor = Settings.NumericColor.ToHexString();
            CodeBuilder.ConstructColor = Settings.ConstructColor.ToHexString();
            CodeBuilder.TypeColor = Settings.TypeColor.ToHexString();
            CodeBuilder.EnumColor = Settings.EnumColor.ToHexString();
            CodeBuilder.InterfaceColor = Settings.InterfaceColor.ToHexString();
            CSharpPreviewSettings.ShouldShowSubgraphComment = Settings.showSubgraphComment;
        }
    }
}