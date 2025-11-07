using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Text.RegularExpressions;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif
namespace Unity.VisualScripting.Community.CSharp
{
    public class CSharpPreviewWindow : EditorWindow
    {
        [SerializeField] private UnityEngine.Object _asset;
        private string manualCode;
        private Vector2 scrollPosition;
        private float visualZoom = 0.7f;

        private bool canCompile = true;

        private string[] codeLines;
        private Dictionary<int, List<ClickableRegion>> cachedRegions;

        private string searchQuery = "";
        private List<int> searchMatches = new List<int>();
        private int currentMatchIndex = 0;
        private Color highlightColor = new Color(1f, 0.8f, 0f, 0.1f);

        private const int LineHeight = 18;
        private const int BufferLines = 20;

        [MenuItem("Window/Community Addons/C# Preview")]
        public static void Open()
        {
            var win = GetWindow<CSharpPreviewWindow>();
            win.titleContent = new GUIContent("C# Preview");
            win.minSize = new Vector2(400, 400);
            win.UpdateCodeDisplay();
            window = win;
        }

        private void OnEnable()
        {
            window = this;
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
            UpdateCodeDisplay();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeObject is ScriptGraphAsset ||
                Selection.activeObject is CodeAsset ||
                Selection.activeObject is GameObject)
            {
                _asset = Selection.activeObject;
                canCompile = true;
                manualCode = "";
                UpdateCodeDisplay();
                Repaint();
            }
        }

        /// <summary>
        /// Opens the preview window with specific code, no asset required.
        /// </summary>
        public static void OpenWithCode(string code, bool canCompile)
        {
            var win = GetWindow<CSharpPreviewWindow>();
            window = win;
            win.titleContent = new GUIContent("C# Preview");
            win.minSize = new Vector2(400, 400);
            win.manualCode = code;
            win.canCompile = canCompile;
            win.UpdateCodeDisplay();
            win.Show();
        }

        private void UpdateCodeDisplay()
        {
            string code = string.Empty;

            if (!string.IsNullOrEmpty(manualCode))
            {
                code = manualCode;
            }
            else if (_asset != null)
            {
                code = LoadCode();
            }

            codeLines = string.IsNullOrEmpty(code)
                ? Array.Empty<string>()
                : code.Split(new[] { '\n' }, StringSplitOptions.None);

            var clickableRegions = CodeUtility.ExtractAndPopulateClickableRegions(code ?? "");
            cachedRegions = clickableRegions
                .GroupBy(r => r.startLine)
                .ToDictionary(g => g.Key, g => g.ToList());

            lineWidths = new float[codeLines.Length];
        }

        private string LoadCode()
        {
            if (_asset == null) return string.Empty;

            if (CodeGeneratorValueUtility.currentAsset == null && _asset != null)
                CodeGeneratorValueUtility.currentAsset = _asset;

            var generator = CodeGenerator.GetSingleDecorator(_asset);
            if (_asset is GameObject @object)
            {
                var components = @object.GetComponents<SMachine>();
                if (components.Length > 0)
                {
                    (generator as GameObjectGenerator).current = components[Mathf.Clamp(currentComponentIndex, 0, components.Length - 1)];
                    CodeGeneratorValueUtility.currentAsset = (generator as GameObjectGenerator).current;
                }
            }
            if (generator == null) return string.Empty;

            var code = generator.Generate(0);
            return code.RemoveMarkdown();
        }
        private float[] lineWidths;
        float contentWidth;
        private void OnGUI()
        {
            if (CodeGeneratorValueUtility.currentAsset == null && _asset != null)
                CodeGeneratorValueUtility.currentAsset = _asset;
            DrawToolbar();

            if (showSettings)
            {
                EditorGUILayout.Space();
                settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                settingsManager.InitializeSettings();
                settingsManager.UpdateSettings(settings => UIBuilder.DrawCSharpPreviewSettings(settings, () => RefreshPreview()));
                EditorGUILayout.EndScrollView();
                return;
            }

            if (codeLines == null || codeLines.Length == 0)
            {
                GUILayout.Label("No code to display.");
                return;
            }

            float lineHeight = LineHeight * visualZoom;
            float totalHeight = codeLines.Length * lineHeight;

            contentWidth = position.width;
            if (lineWidths != null && lineWidths.Length > 0)
            {
                contentWidth = Mathf.Max(position.width, lineWidths.Max()) + 20f;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true);

            Rect scrollRect = GUILayoutUtility.GetRect(contentWidth, totalHeight);

            float bgWidth = Mathf.Max(contentWidth, position.width);
            float bgHeight = Mathf.Max(totalHeight, position.height);

            EditorGUI.DrawRect(new Rect(0, 0, bgWidth, bgHeight), new Color(0.15f, 0.15f, 0.15f));

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / lineHeight) - BufferLines);
            int lastVisible = Mathf.Min(codeLines.Length - 1,
                Mathf.CeilToInt((scrollPosition.y + position.height) / lineHeight) + BufferLines);

            float maxLineWidth = 0f;
            elementWasClicked = false;
            for (int i = firstVisible; i <= lastVisible; i++)
            {
                Rect lineRect = new Rect(0, i * lineHeight, contentWidth, lineHeight);
                float lineWidth = DrawLine(lineRect, i);
                maxLineWidth = Mathf.Max(maxLineWidth, lineWidth);
            }

            if (lineWidths != null && lineWidths.Length > 0)
            {
                contentWidth = Mathf.Max(position.width, maxLineWidth) + 20f;
            }

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Rect codeAreaRect = GUILayoutUtility.GetLastRect();

                if (codeAreaRect.Contains(e.mousePosition))
                {
                    if (!elementWasClicked)
                    {
                        selectedRegions.Clear();
                        selectedLines.Clear();
                        selectedUnitID = "";
                        allCodeSelected = false;
                        GUI.FocusControl(null);
                        e.Use();
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private bool showSettings = false;
        private readonly CSharpPreviewSettingsManager settingsManager = new CSharpPreviewSettingsManager();
        private Vector2 settingsScroll;

        int currentComponentIndex;

        private float sliderValue = 0.15f;
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Zoom", GUILayout.Width(40));
            var newSliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f, GUILayout.Width(150));
            if (Math.Abs(newSliderValue - sliderValue) > 0.001f)
            {
                sliderValue = newSliderValue;
                visualZoom = Mathf.Lerp(0.5f, 1.5f, sliderValue);
            }
            GUILayout.Label($"{visualZoom * 100f:0}%", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:");
            if (GUILayout.Button("<", EditorStyles.toolbarButton))
            {
                if (searchMatches.Count > 0)
                {
                    currentMatchIndex = (currentMatchIndex - 1 + searchMatches.Count) % searchMatches.Count;
                    ScrollToLine(searchMatches[currentMatchIndex]);
                }
            }

            string newSearchQuery = GUILayout.TextField(searchQuery, EditorStyles.toolbarTextField, GUILayout.Width(150));
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                UpdateSearchMatches();
            }

            if (GUILayout.Button(">", EditorStyles.toolbarButton))
            {
                if (searchMatches.Count > 0)
                {
                    currentMatchIndex = (currentMatchIndex + 1) % searchMatches.Count;
                    ScrollToLine(searchMatches[currentMatchIndex]);
                }
            }
            if (searchMatches.Count > 0)
                GUILayout.Label($"{currentMatchIndex + 1}/{searchMatches.Count}");
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(!canCompile);
            if (GUILayout.Button("Compile", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                CompileCode();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy To Clipboard", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                if (allCodeSelected || (selectedLines.Count == 0 && selectedRegions.Count == 0))
                {
                    GUIUtility.systemCopyBuffer = string.Join("\n", codeLines.Select(l => CodeUtility.CleanCode(RemoveColorTags(l))));
                }
                else
                {
                    var lineSelections = new Dictionary<int, List<string>>();

                    foreach (var line in selectedLines)
                    {
                        if (line >= 0 && line < codeLines.Length)
                        {
                            if (!lineSelections.ContainsKey(line))
                                lineSelections[line] = new List<string>();

                            lineSelections[line].Add(CodeUtility.CleanCode(RemoveColorTags(codeLines[line])));
                        }
                    }

                    foreach (var region in selectedRegions.OrderBy(c => c.startLine).ThenBy(c => c.startIndex))
                    {
                        int lineIndex = region.startLine;
                        if (!lineSelections.ContainsKey(lineIndex))
                            lineSelections[lineIndex] = new List<string>();

                        lineSelections[lineIndex].Add(CodeUtility.CleanCode(RemoveColorTags(region.code)));
                    }

                    var finalLines = lineSelections.OrderBy(kv => kv.Key).Select(v => string.Join("", v.Value));

                    GUIUtility.systemCopyBuffer = string.Join("\n", finalLines);
                }
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                UpdateCodeDisplay();
                Repaint();
            }

            if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                showSettings = !showSettings;
            }

            if (_asset is GameObject @object)
            {
                var components = @object.GetComponents<SMachine>();
                var generator = CodeGenerator.GetSingleDecorator(@object) as GameObjectGenerator;

                if (components == null || components.Length <= 1)
                {
                    currentComponentIndex = 0;
                    GUILayout.EndHorizontal();
                    return;
                }

                if (currentComponentIndex < 0 || currentComponentIndex >= components.Length)
                    currentComponentIndex = 0;

                generator.current = components[currentComponentIndex];
                CodeGeneratorValueUtility.currentAsset = components[currentComponentIndex];

                if (GUILayout.Button("<", EditorStyles.toolbarButton))
                {
                    NavigateComponents(-1, generator);
                }
                GUILayout.Label($"{currentComponentIndex + 1}/{components.Length}");
                if (GUILayout.Button(">", EditorStyles.toolbarButton))
                {
                    NavigateComponents(1, generator);
                }
            }


            GUILayout.EndHorizontal();
        }

        private void NavigateComponents(int direction, GameObjectGenerator currentGameObjectGenerator)
        {
            if (currentGameObjectGenerator?.components == null) return;

            var components = currentGameObjectGenerator.components;
            var newIndex = currentComponentIndex + direction;

            if (newIndex >= 0 && newIndex < components.Length)
            {
                currentComponentIndex = newIndex;
                currentGameObjectGenerator.current = components[currentComponentIndex];
                UpdateCodeDisplay();
            }
        }

        private void CompileCode()
        {
            if (_asset != null)
                AssetCompiler.CompileAsset(_asset);
        }

        private string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=#[0-9a-fA-F]{6,8}>|</color>", string.Empty, RegexOptions.Compiled);
        }

        private const float LineNumberWidth = 35f;
        private HashSet<int> selectedLines = new HashSet<int>();
        private HashSet<ClickableRegion> selectedRegions = new HashSet<ClickableRegion>();
        private string selectedUnitID;
        private bool allCodeSelected = false;

        private string CleanLine(string line)
        {
            var toolTipText = CodeUtility.ExtractTooltip(line, out _);
            string cleanText = CodeUtility.CleanCode(toolTipText, false);
            return RemoveColorTags(cleanText);
        }

        private bool elementWasClicked = false;

        private float DrawLine(Rect rect, int index)
        {
            float x = rect.x - scrollPosition.x;

            if (!string.IsNullOrEmpty(searchQuery) && CleanLine(codeLines[index]).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), highlightColor);
            }

            Color borderColor = new Color(0.20f, 0.20f, 0.20f, 0.1f);
            const float borderHeight = 1f;
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, borderHeight), borderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - borderHeight, rect.width, borderHeight), borderColor);

            string lineNumberText = $"{index + 1,4}: ";
            Vector2 numberSize = baseStyle.CalcSize(new GUIContent(lineNumberText));
            float lineNumberWidth = Mathf.Max(LineNumberWidth, numberSize.x);
            Rect lineNumberRect = new Rect(x, rect.y, lineNumberWidth, rect.height);
            var e = Event.current;
            GUI.Label(lineNumberRect, lineNumberText, baseStyle);

            if (e.type == EventType.MouseDown && e.button == 0 && lineNumberRect.Contains(e.mousePosition))
            {
                if (e.CtrlOrCmd())
                {
                    if (selectedLines.Contains(index))
                        selectedLines.Remove(index);
                    else
                    {
                        if (cachedRegions != null && cachedRegions.TryGetValue(index, out var _regions) && _regions.Count > 0)
                        {
                            var first = _regions.FirstOrDefault();
                            if (first != null)
                                selectedUnitID = first.unitId;
                            else selectedUnitID = "";
                        }
                        else
                            selectedUnitID = "";
                        selectedLines.Add(index);
                    }
                }
                else
                {
                    selectedLines.Clear();
                    selectedRegions.Clear();
                    selectedLines.Add(index);
                    if (cachedRegions != null && cachedRegions.TryGetValue(index, out var _regions) && _regions.Count > 0)
                    {
                        var first = _regions.FirstOrDefault();
                        if (first != null)
                            selectedUnitID = first.unitId;
                        else selectedUnitID = "";
                    }
                    else
                        selectedUnitID = "";
                }
                e.Use();
            }

            x += lineNumberWidth;

            float lineHeight = LineHeight * visualZoom;

            if (cachedRegions != null && cachedRegions.TryGetValue(index, out var regions) && regions.Count > 0)
            {
                AdjustLeadingWhitespacesForFirstRegion(codeLines[index], regions[0]);
                bool isMatchingInLine = false;
                foreach (var region in regions)
                {
                    if (!string.IsNullOrEmpty(searchQuery) && !isMatchingInLine)
                    {
                        isMatchingInLine = true;
                        var cleanedCode = CleanLine(codeLines[index]);
                        int matchIndex = cleanedCode.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                        if (matchIndex >= 0)
                        {
                            Vector2 startPos = noPaddingStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanedCode), matchIndex);
                            Vector2 endPos = noPaddingStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanedCode), matchIndex + searchQuery.Length);
                            Rect highlightRect = new Rect(x + startPos.x, rect.y, endPos.x - startPos.x, rect.height);
                            EditorGUI.DrawRect(highlightRect, highlightColor.WithAlpha(0.3f));
                        }
                    }

                    var toolTipText = CodeUtility.ExtractTooltip(region.code, out var tooltip);
                    string cleanText = CodeUtility.CleanCode(toolTipText, false);

                    Vector2 size = noPaddingStyle.CalcSize(Temp(cleanText));
                    var content = string.IsNullOrEmpty(tooltip) ? Temp(cleanText) : Temp(cleanText, tooltip);
                    Rect buttonRect = new Rect(x, rect.y, size.x, rect.height);

                    if (allCodeSelected || selectedLines.Contains(index) || (region != null && selectedRegions.Contains(region)))
                    {
                        EditorGUI.DrawRect(buttonRect, new Color(0.25f, 0.5f, 0.8f, 0.3f));
                    }
                    else if (selectedUnitID == region.unitId)
                    {
                        EditorGUI.DrawRect(buttonRect, new Color(0.25f, 0.25f, 0.25f, 0.4f));
                    }

                    if (GUI.Button(buttonRect, content, noPaddingStyle))
                    {
                        selectedUnitID = region.unitId;
                        HandleClick(index, region);
                        HandleClickableRegionClick(region.unitId, index);
                        elementWasClicked = true;
                    }

                    x += size.x;
                }
            }
            else
            {
                string lineText = CodeUtility.ExtractTooltip(codeLines[index], out var tooltip);
                string cleanText = CodeUtility.CleanCode(lineText);

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    int matchIndex = RemoveColorTags(cleanText).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                    if (matchIndex >= 0)
                    {
                        Vector2 startPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), matchIndex);
                        Vector2 endPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), matchIndex + searchQuery.Length);
                        Rect highlightRect = new Rect(x + startPos.x, rect.y, endPos.x - startPos.x, rect.height);
                        EditorGUI.DrawRect(highlightRect, highlightColor.WithAlpha(0.3f));
                    }
                }

                Vector2 size = baseStyle.CalcSize(Temp(cleanText));
                Rect labelRect = new Rect(x, rect.y, size.x, rect.height);
                var content = string.IsNullOrEmpty(tooltip) ? Temp(cleanText) : Temp(cleanText, tooltip);

                if (allCodeSelected || selectedLines.Contains(index))
                {
                    EditorGUI.DrawRect(labelRect, new Color(0.25f, 0.5f, 0.8f, 0.3f));
                }

                if (GUI.Button(labelRect, content, baseStyle))
                {
                    selectedUnitID = "";
                    HandleClick(index, null);
                    elementWasClicked = true;
                }

                x += size.x;
            }

            lineWidths[index] = x;
            return x;
        }

        private int clickCount = 0;
        private float lastClickTime = 0f;
        private const float doubleClickThreshold = 0.3f;
        private int lastClickedLine = -1;

        private void HandleClick(int index, ClickableRegion region)
        {
            var e = Event.current;
            bool isRegion = region != null;

            if (e != null && e.button == 0)
            {
                float time = Time.realtimeSinceStartup;
                if (index == lastClickedLine && time - lastClickTime <= doubleClickThreshold)
                {
                    clickCount++;
                }
                else
                {
                    clickCount = 1;
                }

                lastClickTime = time; lastClickedLine = index;
                bool ctrl = e.CtrlOrCmd();

                if (clickCount == 2)
                {
                    if (ctrl)
                    {
                        selectedLines.Add(index);
                        if (selectedRegions.Contains(region))
                            selectedRegions.Remove(region);
                    }
                    else
                    {
                        selectedLines.Clear();
                        selectedRegions.Clear();
                        selectedLines.Add(index);
                    }
                    allCodeSelected = false;
                }
                else if (clickCount == 1)
                {
                    if (ctrl)
                    {
                        if (isRegion)
                        {
                            if (selectedRegions.Contains(region))
                                selectedRegions.Remove(region);
                            else
                                selectedRegions.Add(region);
                        }
                        else
                        {
                            if (selectedLines.Contains(index))
                                selectedLines.Remove(index);
                            else
                                selectedLines.Add(index);
                        }
                    }
                    else
                    {
                        selectedLines.Clear();
                        selectedRegions.Clear();
                        if (isRegion)
                            selectedRegions.Add(region);
                        else
                            selectedLines.Add(index);
                        allCodeSelected = false;
                    }
                }
                else if (clickCount >= 3)
                {
                    allCodeSelected = true;
                    selectedLines.Clear();
                    selectedRegions.Clear();
                    clickCount = 0;
                }
            }
        }

        const float Height = 16f;
        private static GUIStyle _noPaddingStyle;
        private GUIStyle noPaddingStyle
        {
            get
            {
                if (_noPaddingStyle == null)
                {
                    _noPaddingStyle = new GUIStyle(baseStyle)
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                    };
                    _noPaddingStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);

                    var c = Color.white;
                    _noPaddingStyle.normal.textColor = c;
                    _noPaddingStyle.hover.textColor = c;
                    _noPaddingStyle.active.textColor = c;
                    _noPaddingStyle.focused.textColor = c;
                }
                else
                {
                    _noPaddingStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);
                }
                return _noPaddingStyle;
            }
        }

        private static GUIStyle _baseStyle;
        private GUIStyle baseStyle
        {
            get
            {
                if (_baseStyle == null)
                {
                    _baseStyle = new GUIStyle(EditorStyles.label)
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                        richText = true
                    };
                    _baseStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);

                    var c = Color.white;
                    _baseStyle.normal.textColor = c;
                    _baseStyle.hover.textColor = c;
                    _baseStyle.active.textColor = c;
                    _baseStyle.focused.textColor = c;
                }
                else
                {
                    _baseStyle.fontSize = Mathf.RoundToInt(Height * visualZoom);
                }
                return _baseStyle;
            }
        }

        private static readonly GUIContent _temp = new GUIContent();
        private static readonly GUIContent _tempTooltip = new GUIContent();
        private GUIContent Temp(string text)
        {
            _temp.text = text;
            return _temp;
        }

        private GUIContent Temp(string text, string tooltip)
        {
            _tempTooltip.text = text;
            _tempTooltip.tooltip = tooltip;
            return _tempTooltip;
        }

        private void AdjustLeadingWhitespacesForFirstRegion(string line, ClickableRegion firstRegion)
        {
            int leadingWhitespaceLength = 0;
            while (leadingWhitespaceLength < line.Length && char.IsWhiteSpace(line[leadingWhitespaceLength]))
            {
                leadingWhitespaceLength++;
            }

            string whitespace = line.Substring(0, leadingWhitespaceLength);
            if (!firstRegion.code.StartsWith(whitespace))
            {
                firstRegion.code = whitespace + firstRegion.code.TrimStart();
            }
        }

        private UnityEngine.Object GetRootObject(GraphReference reference)
        {
            var root = reference.rootObject;
            if (root is MethodDeclaration method) return method.parentAsset;
            if (root is ConstructorDeclaration constructor) return constructor.parentAsset;
            if (root is PropertyGetterMacro getter) return getter.parentAsset;
            if (root is PropertySetterMacro setter) return setter.parentAsset;
            return root;
        }

        private void HandleClickableRegionClick(string unitId, int line)
        {
            var code = CodeGenerator.GetSingleDecorator(_asset);

            if (GraphWindow.active != null && GraphWindow.active.reference != null && GraphWindow.active.context.graph is FlowGraph)
            {
                if (GetRootObject(GraphWindow.active.reference) != (_asset is GameObject ? (code as GameObjectGenerator).current : _asset))
                {
                    OpenInitial(unitId, line, code);
                }
                var reference = GraphWindow.active.reference.isRoot ? GraphWindow.active.reference : GraphWindow.active.reference.root.GetReference() as GraphReference;
                GraphWindow.active.Focus();

                List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                if (reference.macro != null &&
                    (reference.macro is MethodDeclaration ||
                     reference.macro is ConstructorDeclaration ||
                     reference.macro is FieldDeclaration))
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
                    units = GraphTraversal.TraverseFlowGraph<Unit>(reference).ToList();
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
                    var path = GraphTraversal.GetReferencePath(ordered.First().result.Item1);
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
                        GraphWindow.active.context.canvas.UpdateViewport();
                        GraphWindow.active.context.EndEdit();
                    }
                }
            }
            else
            {
                OpenInitial(unitId, line, code);
            }
        }

        private void OpenInitial(string unitId, int line, CodeGenerator code)
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
                    GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                    HandleClickableRegionClick(unitId, line);
                }
            }
            else if (code is ScriptGraphAssetGenerator graphAssetGenerator)
            {
                if (graphAssetGenerator.Data != null)
                {
                    List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                    units = GraphTraversal.TraverseFlowGraph<Unit>(graphAssetGenerator.Data.GetReference() as GraphReference).ToList();
                    var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                    GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                    HandleClickableRegionClick(unitId, line);
                }
            }
            else if (code is GameObjectGenerator gameObjectGenerator)
            {
                if (gameObjectGenerator.Data != null)
                {
                    List<(GraphReference, Unit)> units = new List<(GraphReference, Unit)>();
                    units = GraphTraversal.TraverseFlowGraph<Unit>(gameObjectGenerator.current.GetReference() as GraphReference).ToList();
                    var ordered = units.OrderableSearchFilter(unitId ?? "", (value) => value.Item2.ToString());
                    GraphWindow.OpenActive(ordered.First().result.Item1.isRoot ? ordered.First().result.Item1 : ordered.First().result.Item1.root.GetReference() as GraphReference);
                    HandleClickableRegionClick(unitId, line);
                }
            }
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
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(method.GetReference() as GraphReference));

            foreach (var constructor in classAsset.constructors)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(constructor.GetReference() as GraphReference));

            foreach (var variable in classAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        private void AddUnitsFromStructAsset(StructAsset structAsset, List<(GraphReference, Unit)> units)
        {
            foreach (var method in structAsset.methods)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(method.GetReference() as GraphReference));

            foreach (var constructor in structAsset.constructors)
                units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(constructor.GetReference() as GraphReference));

            foreach (var variable in structAsset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.getter.GetReference() as GraphReference));
                    if (variable.set)
                        units.AddRange(GraphTraversal.TraverseFlowGraph<Unit>(variable.setter.GetReference() as GraphReference));
                }
            }
        }

        private void UpdateSearchMatches()
        {
            searchMatches.Clear();
            currentMatchIndex = 0;

            if (string.IsNullOrEmpty(searchQuery) || codeLines == null) return;

            for (int i = 0; i < codeLines.Length; i++)
            {
                if (CleanLine(codeLines[i]).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    searchMatches.Add(i);
            }

            if (searchMatches.Count > 0)
                ScrollToLine(searchMatches[currentMatchIndex]);
        }

        private void ScrollToLine(int line)
        {
            float lineHeight = LineHeight * visualZoom;
            scrollPosition.y = line * lineHeight;
        }

        private static CSharpPreviewWindow window;
        public static CSharpPreviewWindow RefreshPreview(bool open = false)
        {
            if (window == null && open) window = GetWindow<CSharpPreviewWindow>();
            if (window != null)
            {
                window.UpdateCodeDisplay();
                window.Repaint();
            }

            return window;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnReload()
        {
            var manager = new CSharpPreviewSettingsManager();
            manager.InitializeSettings();
            manager.UpdateSettings(settings =>
            {
                foreach (var info in settings.pendingInfo)
                {
                    info.RestoreCompiler();
                    var compiler = info.compiler as BaseCompiler;
                    if (compiler == null) continue;

#if UNITY_2023_1_OR_NEWER
                    if (!AssetDatabase.AssetPathExists(info.relativePath)) continue;
#endif

                    var type = AssetDatabase.LoadAssetAtPath<MonoScript>(info.relativePath)?.GetClass();
                    try
                    {
                        compiler.PostProcess(info.@object, new PathConfig(), type);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error with post process for {compiler.GetType().Name}: {ex.Message}");
                    }
                }

                settings.pendingInfo.Clear();
            });
        }
    }
}