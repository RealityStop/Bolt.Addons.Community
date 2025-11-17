using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Reflection;
using UnityEditor.UIElements;

namespace Unity.VisualScripting.Community
{
    [InitializeAfterPlugins]
    public static class GraphGUIPatch
    {
        private static readonly HashSet<VisualElement> patchedRoots = new HashSet<VisualElement>();

        static GraphGUIPatch()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private class SearchState
        {
            public string text;
            public List<IGraphElement> matches = new List<IGraphElement>();
            public int currentIndex = -1;
            public Dictionary<IGraphElement, float> highlightTimers = new Dictionary<IGraphElement, float>();
            public GraphReference reference;
        }

        private static readonly Dictionary<GraphWindow, SearchState> windowSearchStates = new Dictionary<GraphWindow, SearchState>();

        private static SearchState GetSearchState(GraphWindow window)
        {
            if (!windowSearchStates.TryGetValue(window, out var state))
            {
                state = new SearchState();
                windowSearchStates[window] = state;
            }
            return state;
        }

        private static Event e => Event.current;

        private static VisualElement Toolbar;
        private static readonly Dictionary<GraphWindow, VisualElement> floatingToolbars = new Dictionary<GraphWindow, VisualElement>();

        static Dictionary<GraphWindow, Sidebars> sidebars = new Dictionary<GraphWindow, Sidebars>();

        private static VisualElement GetFloatingToolbar(GraphWindow window)
        {
            if (!floatingToolbars.TryGetValue(window, out var result))
            {
                result = CreateFloatingToolbar(window);
                floatingToolbars[window] = result;
            }
            return result;
        }
        private static void OnEditorUpdate()
        {
#if NEW_TOOLBAR_STYLE
            var tabs = GraphWindow.tabs;
            if (tabs == null || tabs.Count() == 0)
                return;

            foreach (var window in tabs)
            {
                if (window == null || window.rootVisualElement == null || window.reference == null || window.context == null)
                {
                    if (patchedRoots.Contains(window.rootVisualElement))
                    {
                        patchedRoots.Remove(window.rootVisualElement);
                        Toolbar.RemoveFromHierarchy();
                        GetFloatingToolbar(window)?.RemoveFromHierarchy();
                    }
                    else
                        continue;
                }
                var disableUI = BoltCore.instance == null || EditorApplication.isCompiling;
                if (!disableUI && !patchedRoots.Contains(window.rootVisualElement))
                {
                    PatchGraphGUI(window.rootVisualElement, window);
                    patchedRoots.Add(window.rootVisualElement);
                }
                else if (disableUI)
                {
                    patchedRoots.Remove(window.rootVisualElement);
                    Toolbar.RemoveFromHierarchy();
                    GetFloatingToolbar(window)?.RemoveFromHierarchy();
                    floatingToolbars.Remove(window);
                }
                KeepToolbarAnchored(window);
            }
#else
            var tabs = GraphWindow.tabs;
            if (tabs == null || tabs.Count() == 0)
                return;
            var window = tabs.FirstOrDefault();
            if (window != null)
            {
                var IMGUI = new IMGUIContainer();
                IMGUI.onGUIHandler += () =>
                {
                    InitializeNewGUI();
                };
                window.rootVisualElement.Add(IMGUI);

                if (isInitialized)
                {
                    EditorApplication.update -= OnEditorUpdate;
                    IMGUI.RemoveFromHierarchy();
                }
            }
#endif
        }

        private static void PatchGraphGUI(VisualElement root, GraphWindow window)
        {
            ToolbarGUI(root, window);
            FloatingToolbarGUI(root, window);
        }
        private static VisualElement CreateFloatingToolbar(GraphWindow window)
        {
            var context = window.context;
            var sidebars = GetSidebars(window);
            const float rightMargin = 10f;
            return new VisualElement
            {
                name = "Floating-Toolbar",
                style =
                {
                    position = Position.Absolute,
                    top = 30,
                    right = sidebars.right.show ? sidebars.right.GetWidth() + rightMargin : rightMargin,
                    width = GetCanvasToolbarWidth(context.canvas),
                    height = 25,
                    backgroundColor = Color.clear,
                }
            };
        }
        public const float FloatingToolbarButtonSize = 27;
        private static void FloatingToolbarGUI(VisualElement root, GraphWindow window)
        {
            var FloatingToolbar = GetFloatingToolbar(window);

            FloatingToolbar.style.flexDirection = FlexDirection.Row;
            FloatingToolbar.style.alignItems = Align.FlexEnd;
            FloatingToolbar.style.justifyContent = Justify.SpaceBetween;
            FloatingToolbar.style.flexShrink = 0;

            Action gui = () =>
            {
                var reference = window.reference;
                if (reference == null) return;
                var graph = reference.graph;
                var canvas = reference.Context().canvas;
                var floatingButtonStyle = new GUIStyle(LudiqStyles.spinnerButton)
                {
                    imagePosition = ImagePosition.ImageAbove,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(2, 2, 2, 2)
                };

                GUILayout.BeginHorizontal();

                var carryIcon = EditorGUIUtility.IconContent("MoveTool").image;
                var dimIcon = EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image;
                var valuesIcon = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image;
                var relationsIcon = EditorGUIUtility.IconContent("UnityEditor.HierarchyWindow").image;
                FloatingToolbar.style.width = GetCanvasToolbarWidth(canvas);

                var debugData = reference.debugData;

                var erroredElementsDebugData = ListPool<IGraphElementDebugData>.New();

                foreach (var elementDebugData in debugData.elementsData)
                {
                    if (elementDebugData.runtimeException != null)
                    {
                        erroredElementsDebugData.Add(elementDebugData);
                    }
                }

                if (erroredElementsDebugData.Count > 0)
                {
                    FloatingToolbar.style.width = GetCanvasToolbarWidth(canvas) + FloatingToolbarButtonSize;
                    var clearIcon = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
                    var errorIcon = EditorGUIUtility.IconContent("console.erroricon").image;

                    Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, floatingButtonStyle, GUILayout.Width(25), GUILayout.Height(25));

                    if (GUI.Button(buttonRect, new GUIContent("", "Clear Errors"), floatingButtonStyle))
                    {
                        foreach (var erroredElementDebugData in erroredElementsDebugData)
                        {
                            erroredElementDebugData.runtimeException = null;
                        }

                        FloatingToolbar.style.width = GetCanvasToolbarWidth(canvas);
                    }

                    var iconPadding = 4f;
                    Rect mainRect = new Rect(
                        buttonRect.x + iconPadding,
                        buttonRect.y + iconPadding,
                        buttonRect.width - iconPadding * 2,
                        buttonRect.height - iconPadding * 2
                    );

                    GUI.DrawTexture(mainRect, errorIcon, ScaleMode.ScaleToFit);

                    const float badgeSize = 14f;
                    Rect badgeRect = new Rect(
                        mainRect.xMax - badgeSize + 4,
                        mainRect.yMax - badgeSize + 2,
                        badgeSize,
                        badgeSize
                    );
                    GUI.DrawTexture(badgeRect, clearIcon, ScaleMode.ScaleToFit);
                }

                erroredElementsDebugData.Free();

                if (canvas is FlowCanvas flowCanvas)
                {
                    flowCanvas.showRelations = GUILayout.Toggle(flowCanvas.showRelations, new GUIContent(relationsIcon, "Relations"), ToggleStyle(floatingButtonStyle, flowCanvas.showRelations), GUILayout.Width(25), GUILayout.Height(25));

                    EditorGUI.BeginChangeCheck();

                    BoltFlow.Configuration.showConnectionValues = GUILayout.Toggle(BoltFlow.Configuration.showConnectionValues, new GUIContent(valuesIcon, "Values"), ToggleStyle(floatingButtonStyle, BoltFlow.Configuration.showConnectionValues), GUILayout.Width(25), GUILayout.Height(25));

                    BoltCore.Configuration.dimInactiveNodes = GUILayout.Toggle(BoltCore.Configuration.dimInactiveNodes, new GUIContent(dimIcon, "Dim"), ToggleStyle(floatingButtonStyle, BoltCore.Configuration.dimInactiveNodes), GUILayout.Width(25), GUILayout.Height(25));

                    if (EditorGUI.EndChangeCheck())
                    {
                        BoltFlow.Configuration.Save();

                        BoltCore.Configuration.Save();
                    }
                }
                else if (canvas is StateCanvas stateCanvas)
                {
                    EditorGUI.BeginChangeCheck();

                    BoltCore.Configuration.dimInactiveNodes = GUILayout.Toggle(BoltCore.Configuration.dimInactiveNodes, new GUIContent(dimIcon, "Dim"), ToggleStyle(floatingButtonStyle, BoltCore.Configuration.dimInactiveNodes), GUILayout.Width(25), GUILayout.Height(25));

                    if (EditorGUI.EndChangeCheck())
                    {
                        BoltFlow.Configuration.Save();
                    }
                }

                EditorGUI.BeginChangeCheck();

                BoltCore.Configuration.carryChildren = GUILayout.Toggle(BoltCore.Configuration.carryChildren, new GUIContent(carryIcon, "Carry"), ToggleStyle(new GUIStyle(floatingButtonStyle) { contentOffset = new Vector2(0, -1) }, BoltCore.Configuration.carryChildren), GUILayout.Width(25), GUILayout.Height(25));

                if (EditorGUI.EndChangeCheck())
                {
                    BoltCore.Configuration.Save();
                }

                GUILayout.Space(5);

                if (canvas is FlowCanvas _flowCanvas)
                    EditorGUI.BeginDisabledGroup(_flowCanvas.alignableAndDistributable.Count() < 2);
                else if (canvas is StateCanvas stateCanvas)
                    EditorGUI.BeginDisabledGroup(stateCanvas.alignableAndDistributable.Count() < 2);

                if (GUILayout.Button(new GUIContent(Icons.Enum(AlignOperation.AlignMiddles)?[IconSize.Small], "Align"), floatingButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    var lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.y += 25;
                    LudiqGUI.FuzzyDropdown
                    (
                        lastRect,
                        EnumOptionTree.For<AlignOperation>(),
                        null,
                        (operation) => canvas.Align((AlignOperation)operation)
                    );
                }

                if (GUILayout.Button(new GUIContent(Icons.Enum(DistributeOperation.DistributeCenters)?[IconSize.Small], "Distribute"), floatingButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    var lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.y += 25;
                    LudiqGUI.FuzzyDropdown
                    (
                        lastRect,
                        EnumOptionTree.For<DistributeOperation>(),
                        null,
                        (operation) => canvas.Distribute((DistributeOperation)operation)
                    );
                }

                EditorGUI.EndDisabledGroup();

                GUILayout.Space(5);

                var OverviewStyle = new GUIStyle(floatingButtonStyle)
                {
                    padding = new RectOffset(6, 6, 6, 6)
                };

                if (GUILayout.Button(new GUIContent(PathUtil.Load("Overview", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Overview"), new GUIStyle(OverviewStyle) { contentOffset = new Vector2(0, -1) }, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    GraphUtility.OverrideContextIfNeeded(() =>
                    {
                        canvas.ViewElements(graph.elements);
                    });
                }

                var MaximizeStyle = new GUIStyle(floatingButtonStyle)
                {
                    padding = new RectOffset(6, 5, 5, 6)
                };

                var newMaximized = GUILayout.Toggle(
                    window.maximized,
                    new GUIContent(PathUtil.Load("Maximize", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Maximize"),
                    ToggleStyle(MaximizeStyle, window.maximized),
                    GUILayout.Width(25),
                    GUILayout.Height(25)
                );

                if (newMaximized != window.maximized)
                {
                    window.maximized = newMaximized;
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();
            };
            var imgui = new IMGUIContainer(gui);
            imgui.delegatesFocus = true;
            imgui.style.flexGrow = 1;
            imgui.style.flexShrink = 1;
            imgui.style.flexBasis = 0;
            imgui.style.width = new Length(100, LengthUnit.Percent);

            FloatingToolbar.Add(imgui);

            root.Add(FloatingToolbar);
        }

        private static FieldInfo sidebarsField = typeof(GraphWindow).GetField("sidebars", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void KeepToolbarAnchored(GraphWindow window)
        {
            if (GetFloatingToolbar(window) == null || window.reference == null || window.context == null)
                return;

            var canvas = window.context.canvas;

            if (canvas == null)
                return;

            const float rightMargin = 10f;
            try
            {
                var sidebars = GetSidebars(window);
                GetFloatingToolbar(window).style.right = sidebars.right.show ? sidebars.right.GetWidth() + rightMargin : rightMargin;
            }
            catch { }
        }

        private static Sidebars GetSidebars(GraphWindow window)
        {
            if (!sidebars.TryGetValue(window, out var result))
            {
                result = (Sidebars)sidebarsField.GetValue(window);
                sidebars[window] = result;
            }
            return result;
        }

        private static float GetCanvasToolbarWidth(ICanvas canvas)
        {
            if (canvas is FlowCanvas)
            {
                return (FloatingToolbarButtonSize * 8) + 10;
            }
            else if (canvas is StateCanvas)
            {
                return (FloatingToolbarButtonSize * 6) + 10;
            }
            return 0;
        }

        private static GUIStyle ToggleStyle(GUIStyle baseStyle, bool isOn)
        {
            var style = new GUIStyle(baseStyle);

            if (isOn)
            {
                style.normal.background = baseStyle.active.background ?? baseStyle.normal.background;
                style.normal.textColor = baseStyle.active.textColor != default
                    ? baseStyle.active.textColor
                    : baseStyle.normal.textColor;
            }

            return style;
        }

        private static void ToolbarGUI(VisualElement root, GraphWindow window)
        {
            var state = GetSearchState(window);
            Toolbar = new VisualElement
            {
                name = "CustomGraphToolbar"
            };
            Toolbar.style.top = 0;
            Toolbar.style.right = 0;
            Toolbar.style.height = 20;
            Toolbar.style.flexDirection = FlexDirection.Row;
            Toolbar.style.alignItems = Align.Center;
            Toolbar.style.justifyContent = Justify.SpaceBetween;
#if DARKER_UI
            Toolbar.style.backgroundColor = CommunityStyles.backgroundColor;
#else
            Toolbar.style.backgroundColor = ColorPalette.unityBackgroundLight.color;
#endif
            Toolbar.style.flexShrink = 0;

            var imgui = new IMGUIContainer(() =>
            {
                InitializeNewGUI();

                if (window == null || window.reference == null) return;

                LudiqGUI.BeginHorizontal();

                if (!LudiqGUIUtility.newSkin)
                {
                    LudiqGUI.Space(-6);
                }

                EditorGUI.BeginChangeCheck();

                window.locked = GUILayout.Toggle(window.locked, GraphGUI.Styles.lockIcon, LudiqStyles.toolbarButton);

                if (window.showSidebars)
                {
                    window.graphInspectorEnabled = GUILayout.Toggle(window.graphInspectorEnabled, BoltCore.Icons.inspectorWindow?[IconSize.Small], LudiqStyles.toolbarButton);
                    window.variablesInspectorEnabled = GUILayout.Toggle(window.variablesInspectorEnabled, BoltCore.Icons.variablesWindow?[IconSize.Small], LudiqStyles.toolbarButton);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    window.MatchSelection();
                }

                LudiqGUI.Space(6);

                foreach (var breadcrumb in window.reference.GetBreadcrumbs())
                {
                    var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);
                    title.text = " " + title.text;
                    var style = breadcrumb.isRoot ? Styles.toolbarBreadcrumbRoot : Styles.toolbarBreadcrumb;
                    var isCurrent = breadcrumb == window.reference;
                    var restored = GUI.backgroundColor;
#if DARKER_UI
                    if (isCurrent) GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.gray : Color.white;
#endif
                    if (GUILayout.Toggle(isCurrent, title, style, GUILayout.MinWidth(80), GUILayout.Height(20)) && !isCurrent)
                    {
                        window.reference = breadcrumb;
                    }
                    GUI.backgroundColor = restored;
                }

                if (state.reference != window.reference)
                {
                    state.reference = window.reference;
                    state.highlightTimers.Clear();
                    state.matches.Clear();
                    state.currentIndex = -1;
                    Search(window, state);
                }

                GUILayout.Space(10);

                GUILayout.BeginHorizontal(GUILayout.Width(400));

                var newSearchText = GUILayout.TextField(state.text ?? "", EditorStyles.toolbarSearchField, GUILayout.Height(18));
                if (newSearchText != state.text)
                {
                    state.text = newSearchText;
                    state.matches.Clear();
                    state.currentIndex = -1;
                    state.highlightTimers.Clear();

                    Search(window, state);
                }
#if DARKER_UI
                if (GUILayout.Button("Previous", CommunityStyles.ToolbarButton, GUILayout.Width(60), GUILayout.Height(18)) && state.matches.Count > 0)
#else
                if (GUILayout.Button("Previous", LudiqStyles.toolbarButton, GUILayout.Width(60), GUILayout.Height(18)) && state.matches.Count > 0)
#endif
                {
                    state.currentIndex--;
                    if (state.currentIndex < 0) state.currentIndex = state.matches.Count - 1;
                    HighlightCurrentMatch(window);
                }
#if DARKER_UI
                if (GUILayout.Button("Next", CommunityStyles.ToolbarButton, GUILayout.Width(40), GUILayout.Height(18)) && state.matches.Count > 0)
#else
                if (GUILayout.Button("Next", LudiqStyles.toolbarButton, GUILayout.Width(40), GUILayout.Height(18)) && state.matches.Count > 0)
#endif
                {
                    state.currentIndex++;
                    if (state.currentIndex >= state.matches.Count) state.currentIndex = 0;
                    HighlightCurrentMatch(window);
                }

                GUILayout.Label((state.matches.Count == 0 ? 0 : state.currentIndex + 1) + "/" + state.matches.Count);

                GUILayout.EndHorizontal();

                LudiqGUI.FlexibleSpace();

                GUILayout.Label("Zoom", LudiqStyles.toolbarLabel);
                window.context.graph.zoom = GUILayout.HorizontalSlider(window.context.graph.zoom, GraphGUI.MinZoom, GraphGUI.MaxZoom, GUILayout.Width(100));
                GUILayout.Label(window.context.graph.zoom.ToString("0.#") + "x", LudiqStyles.toolbarLabel);

                LudiqGUI.EndHorizontal();

                if (window?.context?.canvas == null) return;

                var canvas = window.context.canvas;
                foreach (var kvp in state.highlightTimers.ToList())
                {
                    var element = kvp.Key;
                    float timeLeft = kvp.Value - (float)EditorApplication.timeSinceStartup;
                    if (timeLeft <= 0)
                    {
                        state.highlightTimers.Remove(element);
                        continue;
                    }

                    var widget = canvas.Widget(element);
                    if (widget == null) continue;

                    float alpha = Mathf.Clamp01(timeLeft / 1f);

                    var rect = widget.position.ExpandBy(new RectOffset(5, 5, 12, 10));
                    var zoom = canvas.zoom;
                    var pan = canvas.pan;
                    var viewport = canvas.viewport;

                    Vector2 viewportCenter = viewport.size * 0.5f;
                    Vector2 screenPos = (rect.position - pan + viewportCenter) * zoom;
                    Rect screenRect = new Rect(screenPos, rect.size * zoom);

                    screenRect.y += 24f;

                    var sidebars = GetSidebars(window);

                    if (sidebars.left.show)
                        screenRect.x += sidebars.left.GetWidth();

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(screenRect, Color.clear, new Color(0.3f, 0.8f, 1f, alpha * 0.6f));
                    Handles.EndGUI();
                }
            });

            imgui.style.flexGrow = 1;
            imgui.style.flexShrink = 1;
            imgui.style.flexBasis = 0;
            imgui.style.width = new Length(100, LengthUnit.Percent);

            Toolbar.Add(imgui);

            root.Add(Toolbar);

            Toolbar.style.position = Position.Relative;
            Toolbar.style.width = new Length(100, LengthUnit.Percent);
        }

        private static void Search(GraphWindow window, SearchState state)
        {
            if (!string.IsNullOrEmpty(state.text))
            {
                foreach (var element in window.reference.graph.elements)
                {
                    if (element is IUnitConnection) continue;

                    if (SearchUtility.SearchMatches(state.text, SearchUtility.GetSearchName(element),
                        NodeFinderWindow.SearchMode.Contains, out _, null))
                    {
                        state.matches.Add(element);
                    }
                }

                if (state.matches.Count > 0)
                {
                    state.currentIndex = 0;
                    HighlightCurrentMatch(window);
                }
            }
        }

        private static void HighlightCurrentMatch(GraphWindow window)
        {
            var state = GetSearchState(window);
            if (state.currentIndex < 0 || state.currentIndex >= state.matches.Count) return;

            var element = state.matches[state.currentIndex];

            GraphUtility.OverrideContextIfNeeded(() =>
            {
                if (window.reference.graph.elements.Contains(element))
                    window.context.canvas.ViewElements(element.Yield());
            });

            state.highlightTimers[element] = (float)EditorApplication.timeSinceStartup + 1f;
        }

        public static class Styles
        {
            static Styles()
            {
                toolbarBreadcrumbRoot = new GUIStyle(new GUIStyle("GUIEditor.BreadcrumbLeftBackground"))
                {
                    alignment = TextAnchor.MiddleCenter
                };
                toolbarBreadcrumbRoot.padding.bottom++;
                toolbarBreadcrumbRoot.padding.left = 0;
                toolbarBreadcrumbRoot.padding.right = 15;
                toolbarBreadcrumbRoot.fontSize = 9;

                toolbarBreadcrumb = new GUIStyle("GUIEditor.BreadcrumbMidBackground")
                {
                    alignment = TextAnchor.MiddleCenter
                };
                toolbarBreadcrumb.padding.bottom++;
                toolbarBreadcrumb.padding.right = 15;
                toolbarBreadcrumb.padding.left = 10;
                toolbarBreadcrumb.fontSize = 9;
            }

            public static readonly GUIStyle toolbarBreadcrumbRoot;
            public static readonly GUIStyle toolbarBreadcrumb;
        }

        private static Type sidebarpanelStylesType = typeof(SidebarPanel).GetNestedType("Styles", BindingFlags.NonPublic);

        private static bool isInitialized;
        public static void InitializeNewGUI()
        {
            if (!isInitialized)
            {
                isInitialized = true;

#if DARKER_UI
                // Sidebar
                var bg = sidebarpanelStylesType.GetField("background", BindingFlags.Public | BindingFlags.Static).GetValue(null) as GUIStyle;
                var title = sidebarpanelStylesType.GetField("title", BindingFlags.Public | BindingFlags.Static).GetValue(null) as GUIStyle;
                bg.normal.background = CommunityStyles.backgroundColor.GetPixel();
                title.normal.background = CommunityStyles.background;
                title.border = new RectOffset(0, 0, 2, 2);

                // Headers
                LudiqStyles.headerBackground.normal.background = CommunityStyles.backgroundColor.GetPixel();
#if !NEW_TOOLBAR_STYLE
                LudiqStyles.toolbarBackground.normal.background = CommunityStyles.backgroundColor.GetPixel();
#endif
                // Variables Panel 
                var tab = VariablesPanel.Styles.tab;
                var tabField = typeof(VariablesPanel.Styles).GetField("tab", BindingFlags.Static | BindingFlags.Public);
                tabField.SetValue(null, new GUIStyle(CommunityStyles.ToolbarButton));
                var tabStyle = VariablesPanel.Styles.tab;
                tabStyle.normal = CommunityStyles.ToolbarButton.normal;
                tabStyle.hover = CommunityStyles.ToolbarButton.hover;
                tabStyle.active = CommunityStyles.ToolbarButton.active;
                tabStyle.focused = CommunityStyles.ToolbarButton.focused;
                tabStyle.onNormal = CommunityStyles.ToolbarButton.onNormal;
                tabStyle.onHover = CommunityStyles.ToolbarButton.onHover;
                tabStyle.onActive = CommunityStyles.ToolbarButton.onActive;
                tabStyle.onFocused = CommunityStyles.ToolbarButton.onFocused;
                tabStyle.alignment = TextAnchor.MiddleLeft;
                tabStyle.padding = new RectOffset(4, 4, 2, 2);
                tabStyle.border = new RectOffset(2, 10, 0, 0);
                tabStyle.overflow = new RectOffset(0, 1, 0, 0);
                tabStyle.clipping = TextClipping.Clip;
                tabStyle.fontSize = 12;
                tabStyle.fontStyle = FontStyle.Normal;
                tabStyle.fixedHeight = 22;
                tabStyle.stretchWidth = true;

                var subTabField = typeof(VariablesPanel.Styles).GetField("subTab", BindingFlags.Static | BindingFlags.Public);
                subTabField.SetValue(null, tabStyle ?? new GUIStyle(CommunityStyles.ToolbarButton));

                UnitEditor.Styles.inspectorBackground.normal.background = CommunityStyles.background;
                UnitEditor.Styles.portsBackground.normal.background = CommunityStyles.background;
                StateEditor.Styles.inspectorBackground.normal.background = CommunityStyles.background;

                LudiqStyles.toolbarButton.normal = CommunityStyles.ToolbarButton.normal;
                LudiqStyles.toolbarButton.hover = CommunityStyles.ToolbarButton.hover;
                LudiqStyles.toolbarButton.active = CommunityStyles.ToolbarButton.active;
                LudiqStyles.toolbarButton.onNormal = CommunityStyles.ToolbarButton.onNormal;
                LudiqStyles.toolbarButton.onHover = CommunityStyles.ToolbarButton.onHover;
                LudiqStyles.toolbarButton.onActive = CommunityStyles.ToolbarButton.onActive;
#endif

#if NEW_UNIT_STYLE && NEW_UNIT_UI
                var green = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Green);
                green.normal.background = PathUtil.Load("GreenNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                green.active.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                green.focused.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                green.hover.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                green.padding = new RectOffset(5, 5, 5, 5);

                var gray = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Gray);
                gray.normal.background = PathUtil.Load("GrayNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                gray.active.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                gray.focused.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                gray.hover.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];

                var yellow = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Yellow);
                yellow.normal.background = PathUtil.Load("YellowNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                yellow.active.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                yellow.focused.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                yellow.hover.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];

                var orange = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Orange);
                orange.normal.background = PathUtil.Load("OrangeNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                orange.active.background = PathUtil.Load("OrangeNodeSelected", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                orange.focused.background = PathUtil.Load("OrangeNodeSelected", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                orange.hover.background = PathUtil.Load("OrangeNodeSelected", CommunityEditorPath.Fundamentals)?[IconSize.Large];

                var teal = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Teal);
                teal.normal.background = PathUtil.Load("TealNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                teal.active.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                teal.focused.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                teal.hover.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];

                var blue = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Blue);
                blue.normal.background = PathUtil.Load("BlueNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                blue.active.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                blue.focused.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                blue.hover.background = PathUtil.Load("SelectedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];

                var red = GraphGUI.GetNodeStyle(NodeShape.Square, NodeColor.Red);
                red.normal.background = PathUtil.Load("RedNode", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                red.active.background = PathUtil.Load("RedNodeSelected", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                red.focused.background = PathUtil.Load("RedNodeSelected", CommunityEditorPath.Fundamentals)?[IconSize.Large];
                red.hover.background = PathUtil.Load("RedNodeSelected", CommunityEditorPath.Fundamentals)?[IconSize.Large];

#if !ENABLE_VERTICAL_FLOW
                var unitWidgetGeneric = typeof(Unity.VisualScripting.UnitWidget<>);

                var normalType = VisualScripting.UnitWidget<IUnit>.Styles.portsBackground;

                var baseColor = CommunityStyles.backgroundColor;
                var tex = EditorGUIUtility.isProSkin
                    ? CommunityStyles.MakeBorderedTexture(baseColor, baseColor.Darken(0.05f))
                    : CommunityStyles.MakeBorderedTexture(baseColor, baseColor.Brighten(0.05f));

                normalType.normal.background = tex;

                foreach (var type in Codebase.editorTypes)
                {
                    if (!type.InheritsFromGeneric(unitWidgetGeneric, out var result))
                        continue;

                    var stylesType = unitWidgetGeneric.GetNestedType("Styles", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).MakeGenericType(result.GetGenericArguments());
                    if (stylesType == null || stylesType.ContainsGenericParameters)
                    {
                        continue;
                    }

                    var field = stylesType.GetField("portsBackground", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (field == null)
                        continue;

                    var portsBackground = field.GetValue(null) as GUIStyle;
                    if (portsBackground == null)
                        continue;

                    portsBackground.normal.background = tex;
                }
#endif

#endif
            }
        }
    }
}