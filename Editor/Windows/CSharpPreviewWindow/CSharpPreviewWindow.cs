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
        public static bool isPreviewing;
        [SerializeField] private UnityEngine.Object _asset;
        private CodeWriter manualCodeWriter;
        private Vector2 scrollPosition;
        private float visualZoom = 0.7f;

        private bool canCompile = true;

        private string[] codeLines;
        private int[] lineStartIndices;
        private SourceMap sourceMap;
        private SourceNode activeNode;
        private CodeWriter generatedCodeWriter;

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
                manualCodeWriter = null;
                UpdateCodeDisplay();
                Repaint();
            }
        }

        /// <summary>
        /// Opens the preview window with specific code, no asset required.
        /// </summary>
        public static void OpenWithCode(CodeWriter code, bool canCompile)
        {
            var win = GetWindow<CSharpPreviewWindow>();
            window = win;
            win.titleContent = new GUIContent("C# Preview");
            win.minSize = new Vector2(400, 400);
            win.manualCodeWriter = code;
            win.canCompile = canCompile;
            win.UpdateCodeDisplay();
            win.Show();
        }

        private void UpdateCodeDisplay()
        {
            settingsManager.InitializeSettings();
            generatedCodeWriter = null;
            sourceMap = null;
            activeNode = null;

            if (manualCodeWriter != null)
            {
                generatedCodeWriter = manualCodeWriter;
            }
            else if (_asset != null)
            {
                generatedCodeWriter = LoadCode();
            }

            string displayCode = generatedCodeWriter?.ToHighlightedString() ?? "";
            codeLines = string.IsNullOrEmpty(displayCode)
                ? Array.Empty<string>()
                : displayCode.Split('\n');

            if (generatedCodeWriter != null)
            {
                sourceMap = generatedCodeWriter.GetSourceMap();
            }

            lineStartIndices = new int[codeLines.Length];
            int index = 0;
            for (int i = 0; i < codeLines.Length; i++)
            {
                lineStartIndices[i] = index;
                index += codeLines[i].RemoveHighlights().RemoveMarkdown().Length + 1;
            }

            lineWidths = new float[codeLines.Length];
        }

        private CodeWriter LoadCode()
        {
            if (_asset == null) return null;
            isPreviewing = true;
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

            if (generator == null) return null;

            var writer = new CodeWriter();
            generator.Generate(writer, generator.GetGenerationData());
            isPreviewing = false;
            return writer;
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
                        activeNode = null;
                        selectionStartLine = -1;
                        selectionEndLine = -1;
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
                string textToCopy;

                if (activeNode != null)
                {
                    int start = activeNode.Span.Start;
                    int end = activeNode.Span.End;

                    var codeText = generatedCodeWriter.ToString();

                    start = Mathf.Clamp(start, 0, codeText.Length);
                    end = Mathf.Clamp(end, 0, codeText.Length);

                    textToCopy = codeText.Substring(start, end - start);
                }
                else if (selectionStartLine >= 0 && selectionEndLine >= 0)
                {
                    textToCopy = string.Join("\n", codeLines.Skip(selectionStartLine).Take(selectionEndLine - selectionStartLine + 1)
                        .Select(l => RemoveColorTags(l)));
                }
                else
                {
                    textToCopy = string.Join("\n", codeLines.Select(l => RemoveColorTags(l)));
                }

                GUIUtility.systemCopyBuffer = textToCopy;
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
            return input.RemoveHighlights().RemoveMarkdown();
        }

        private const float LineNumberWidth = 35f;
        private bool elementWasClicked = false;

        private int selectionStartLine = -1;
        private int selectionEndLine = -1;
        private int clickCount = 0;
        private double lastClickTime = 0;
        private const double doubleClickTime = 0.3;

        private float DrawLine(Rect rect, int index)
        {
            float x = rect.x - scrollPosition.x;
            float lineHeight = LineHeight * visualZoom;

            if (!string.IsNullOrEmpty(searchQuery) && codeLines[index].IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
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

            x += lineNumberWidth;

            string cleanText = codeLines[index].RemoveHighlights().RemoveMarkdown();

            // Highlight active source node
            if (activeNode != null)
            {
                SourceSpan span = activeNode.Span;
                int lineStart = lineStartIndices[index];
                int lineEnd = lineStart + cleanText.Length;

                if (span.Start < lineEnd && span.End > lineStart)
                {
                    int localStart = Mathf.Max(span.Start, lineStart) - lineStart;
                    int localEnd = Mathf.Min(span.End, lineEnd) - lineStart;

                    Vector2 startPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), localStart);
                    Vector2 endPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), localEnd);

                    Rect highlight = new Rect(x + startPos.x, rect.y, endPos.x - startPos.x, rect.height);
                    EditorGUI.DrawRect(highlight, new Color(0.25f, 0.5f, 0.8f, 0.25f));
                }
            }

            if (selectionStartLine >= 0 && selectionEndLine >= 0)
            {
                if (index >= selectionStartLine && index <= selectionEndLine)
                {
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), new Color(0.3f, 0.5f, 0.8f, 0.25f));
                }
            }

            // Highlight search matches
            if (!string.IsNullOrEmpty(searchQuery))
            {
                int matchIndex = cleanText.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
                if (matchIndex >= 0)
                {
                    Vector2 startPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), matchIndex);
                    Vector2 endPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), matchIndex + searchQuery.Length);
                    Rect highlightRect = new Rect(x + startPos.x, rect.y, endPos.x - startPos.x, rect.height);
                    EditorGUI.DrawRect(highlightRect, highlightColor.WithAlpha(0.3f));
                }
            }

            CodeDiagnostic best = null;
            float bestScore = float.MinValue;

            // Display diagnostics
            if (generatedCodeWriter != null)
            {
                foreach (CodeDiagnostic diagnostic in generatedCodeWriter.GetDiagnostics())
                {
                    SourceSpan span = diagnostic.Span;
                    int lineStart = lineStartIndices[index];
                    int lineEnd = lineStart + cleanText.Length;

                    if (span.Start < lineEnd && span.End > lineStart)
                    {
                        int localStart = Mathf.Max(span.Start, lineStart) - lineStart;
                        int localEnd = Mathf.Min(span.End, lineEnd) - lineStart;

                        Vector2 startPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), localStart);
                        Vector2 endPos = baseStyle.GetCursorPixelPosition(new Rect(0, 0, 1000, lineHeight), Temp(cleanText), localEnd);

                        Rect diagnosticRect = new Rect(x + startPos.x, rect.y, Mathf.Max(2f, endPos.x - startPos.x), rect.height);

                        if (diagnosticRect.Contains(Event.current.mousePosition))
                        {
                            float spanSize = span.End - span.Start;
                            float severity = (int)diagnostic.Kind;
                            float centerX = diagnosticRect.x + diagnosticRect.width * 0.5f;
                            float mouseDist = Mathf.Abs(Event.current.mousePosition.x - centerX);

                            float score =
                                (100000f / Mathf.Max(1f, spanSize)) +
                                (severity * 1000f) -
                                mouseDist;

                            if (score > bestScore)
                            {
                                bestScore = score;
                                best = diagnostic;
                            }
                        }
                    }
                }
            }

            var highlightedCode = codeLines[index].RemoveMarkdown();
            var content = best != null ? Temp(highlightedCode, GetDiagnosticMessage(best)) : Temp(highlightedCode);
            Vector2 size = baseStyle.CalcSize(content);
            Rect labelRect = new Rect(x, rect.y, size.x, rect.height);

            if (GUI.Button(labelRect, content, baseStyle))
            {
                double time = EditorApplication.timeSinceStartup;
                if (time - lastClickTime < doubleClickTime)
                    clickCount++;
                else
                    clickCount = 1;

                lastClickTime = time;

                if (clickCount == 2)
                {
                    activeNode = null;
                    selectionStartLine = index;
                    selectionEndLine = index;
                }
                else if (clickCount >= 3)
                {
                    activeNode = null;
                    selectionStartLine = 0;
                    selectionEndLine = codeLines.Length - 1;
                }
                else
                {
                    ResolveClick(labelRect, index);
                    selectionStartLine = -1;
                    selectionEndLine = -1;
                }

                elementWasClicked = true;
            }

            x += size.x;
            lineWidths[index] = x;
            return x;
        }

        private string GetDiagnosticMessage(CodeDiagnostic diagnostic)
        {
            switch (diagnostic.Kind)
            {
                case CodeDiagnosticKind.Info: return "Info: " + diagnostic.Message;
                case CodeDiagnosticKind.Warning: return $"<color={"#" + CodeBuilder.WarningColor}>Warning: " + diagnostic.Message + "</color>";
                case CodeDiagnosticKind.Recommendation: return $"<color={"#" + CodeBuilder.RecommendationColor}>Recommendation: " + diagnostic.Message + "</color>";
                case CodeDiagnosticKind.Error: return $"<color={"#" + CodeBuilder.ErrorColor}>Error: " + diagnostic.Message + "</color>";
            }
            return diagnostic.Message;
        }

        private void ResolveClick(Rect rect, int lineIndex)
        {
            Vector2 mouse = Event.current.mousePosition;
            float lineHeight = LineHeight * visualZoom;

            float clickXInRect = mouse.x - rect.x;

            string displayText = codeLines[lineIndex];
            string cleanText = displayText.RemoveHighlights().RemoveMarkdown();

            int charIndex = 0;

            Rect measureRect = new Rect(0, 0, 2000, lineHeight);

            for (int i = 0; i < cleanText.Length; i++)
            {
                Vector2 startPos = baseStyle.GetCursorPixelPosition(measureRect, Temp(cleanText), i);
                Vector2 endPos = baseStyle.GetCursorPixelPosition(measureRect, Temp(cleanText), i + 1);

                float charStartX = startPos.x;
                float charEndX = endPos.x;

                if (clickXInRect >= charStartX && clickXInRect < charEndX)
                {
                    charIndex = i;
                    break;
                }

                if (charStartX > clickXInRect)
                {
                    charIndex = i;
                    break;
                }

                charIndex = i + 1;
            }

            int globalIndex = lineStartIndices[lineIndex] + charIndex;
            activeNode = sourceMap?.Resolve(globalIndex);

            if (activeNode != null && activeNode.Unit != null)
            {
                HandleClickableRegionClick(activeNode.Unit, lineIndex + 1);
            }
            else
            {
                int lineStart = lineStartIndices[lineIndex];
                int lineEnd = lineStart + cleanText.Length;
                activeNode = new SourceNode(new SourceSpan(lineStart, lineEnd), null, null);
            }

            Repaint();
        }

        const float Height = 16f;

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

        private UnityEngine.Object GetRootObject(GraphReference reference)
        {
            var root = reference.rootObject;
            if (root is MethodDeclaration method) return method.parentAsset;
            if (root is ConstructorDeclaration constructor) return constructor.parentAsset;
            if (root is PropertyGetterMacro getter) return getter.parentAsset;
            if (root is PropertySetterMacro setter) return setter.parentAsset;
            return root;
        }

        private void HandleClickableRegionClick(Unit unit, int line)
        {
            var code = CodeGenerator.GetSingleDecorator(_asset);

            if (GraphWindow.active != null &&
                GraphWindow.active.reference != null &&
                GraphWindow.active.context.graph is FlowGraph)
            {
                if (GetRootObject(GraphWindow.active.reference) !=
                    (_asset is GameObject ? (code as GameObjectGenerator).current : _asset))
                {
                    OpenInitial(unit, line, code);
                }

                var reference =
                    GraphWindow.active.reference.isRoot
                        ? GraphWindow.active.reference
                        : GraphWindow.active.reference.root.GetReference() as GraphReference;

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

                var match = units.FirstOrDefault(u => u.Item2 == unit);

                if (match.Item2 != null)
                {
                    if (!GraphWindow.active.reference.isRoot &&
                        reference != match.Item1)
                    {
                        GraphWindow.active.reference =
                            GraphWindow.active.reference.root.GetReference() as GraphReference;
                    }

                    var path = GraphTraversal.GetReferencePath(match.Item1);

                    if (GraphWindow.active.reference != match.Item1)
                    {
                        foreach (var item in path)
                        {
                            if (item.Item2 != null)
                            {
                                GraphWindow.active.reference =
                                    GraphWindow.active.reference.ChildReference(item.Item2, false);
                            }
                            else if (item.Item1.isRoot)
                            {
                                GraphWindow.active.reference = item.Item1;
                            }
                        }
                    }

                    var canvas = GraphWindow.active.context.canvas as FlowCanvas;
                    GraphWindow.active.context.BeginEdit();
                    canvas.ViewElements(new List<Unit> { match.Item2 });
                    GraphWindow.active.context.canvas.UpdateViewport();
                    GraphWindow.active.context.EndEdit();
                }
            }
            else
            {
                OpenInitial(unit, line, code);
            }
        }

        private void OpenInitial(Unit unit, int line, CodeGenerator code)
        {
            if (code is ClassAssetGenerator classAssetGenerator &&
                classAssetGenerator.Data != null)
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

                var match = units.FirstOrDefault(u => u.Item2 == unit);
                if (match.Item2 != null)
                {
                    GraphWindow.OpenActive(match.Item1);
                    HandleClickableRegionClick(unit, line);
                }
            }
            else if (code is StructAssetGenerator structAssetGenerator &&
                     structAssetGenerator.Data != null)
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

                var match = units.FirstOrDefault(u => u.Item2 == unit);
                if (match.Item2 != null)
                {
                    var root = match.Item1.isRoot
                        ? match.Item1
                        : match.Item1.root.GetReference() as GraphReference;

                    GraphWindow.OpenActive(root);
                    HandleClickableRegionClick(unit, line);
                }
            }
            else if (code is ScriptGraphAssetGenerator graphAssetGenerator &&
                     graphAssetGenerator.Data != null)
            {
                var units =
                    GraphTraversal.TraverseFlowGraph<Unit>(
                        graphAssetGenerator.Data.GetReference() as GraphReference).ToList();

                var match = units.FirstOrDefault(u => u.Item2 == unit);
                if (match.Item2 != null)
                {
                    var root = match.Item1.isRoot
                        ? match.Item1
                        : match.Item1.root.GetReference() as GraphReference;

                    GraphWindow.OpenActive(root);
                    HandleClickableRegionClick(unit, line);
                }
            }
            else if (code is GameObjectGenerator gameObjectGenerator &&
                     gameObjectGenerator.Data != null)
            {
                var units =
                    GraphTraversal.TraverseFlowGraph<Unit>(
                        gameObjectGenerator.current.GetReference() as GraphReference).ToList();

                var match = units.FirstOrDefault(u => u.Item2 == unit);
                if (match.Item2 != null)
                {
                    var root = match.Item1.isRoot
                        ? match.Item1
                        : match.Item1.root.GetReference() as GraphReference;

                    GraphWindow.OpenActive(root);
                    HandleClickableRegionClick(unit, line);
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
                if (codeLines[i].IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
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