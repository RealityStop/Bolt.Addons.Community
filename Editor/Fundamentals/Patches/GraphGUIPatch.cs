using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using UnityEditor.UIElements;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections;

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
            public Label counterLabel;
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

        private static readonly Dictionary<GraphWindow, VisualElement> floatingToolbars = new Dictionary<GraphWindow, VisualElement>();
        private static readonly Dictionary<GraphWindow, VisualElement> Toolbars = new Dictionary<GraphWindow, VisualElement>();

        static Dictionary<GraphWindow, Sidebars> sidebars = new Dictionary<GraphWindow, Sidebars>();

        private static VisualElement GetFloatingToolbar(GraphWindow window)
        {
            if (!floatingToolbars.TryGetValue(window, out var result))
            {
                result = CreateFloatingToolbar(window);
                if (result == null) return null;
                floatingToolbars[window] = result;
            }
            return result;
        }

        private static VisualElement GetToolbar(GraphWindow window)
        {
            if (!Toolbars.TryGetValue(window, out var result))
            {
                result = CreateToolbar(window);
                Toolbars[window] = result;
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

                        GetToolbar(window)?.RemoveFromHierarchy();
                        GetFloatingToolbar(window)?.RemoveFromHierarchy();

                        if (floatingToolbars.ContainsKey(window))
                            floatingToolbars.Remove(window);
                        if (Toolbars.ContainsKey(window))
                            Toolbars.Remove(window);
                    }
                    else
                        continue;
                }
                var disableUI = BoltCore.instance == null || EditorApplication.isCompiling || PluginContainer.anyVersionMismatch;
                if (!disableUI && !patchedRoots.Contains(window.rootVisualElement) && window.reference != null)
                {
                    PatchGraphGUI(window.rootVisualElement, window);
                    patchedRoots.Add(window.rootVisualElement);
                }
                else if (disableUI)
                {
                    patchedRoots.Remove(window.rootVisualElement);
                    GetToolbar(window).RemoveFromHierarchy();
                    GetFloatingToolbar(window)?.RemoveFromHierarchy();
                    floatingToolbars.Remove(window);
                    Toolbars.Remove(window);
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
            if (window.reference == null || window.context == null) return null;
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

        private static VisualElement CreateToolbar(GraphWindow window)
        {
            return new VisualElement
            {
                name = "CustomGraphToolbar",
                style =
                {
                    top = 0,
                    right = 0,
                    height = 20,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    flexShrink = 0,
#if DARKER_UI
                    backgroundColor = CommunityStyles.backgroundColor
#else
                    backgroundColor = ColorPalette.unityBackgroundLight.color
#endif
                }
            };
        }
        public const float FloatingToolbarButtonSize = 28;
        private static bool previousDeveloperMode = BoltCore.Configuration.developerMode;
        private static void FloatingToolbarGUI(VisualElement root, GraphWindow window)
        {
            var floatingToolbar = GetFloatingToolbar(window);
            if (floatingToolbar == null) return;
            floatingToolbar.style.flexDirection = FlexDirection.Row;
            floatingToolbar.style.alignItems = Align.FlexEnd;
            floatingToolbar.style.justifyContent = Justify.SpaceBetween;
            floatingToolbar.style.flexShrink = 0;

            void RebuildToolbar()
            {
                floatingToolbar.Clear();
                var reference = window.reference;
                if (reference == null || !reference.isValid) return;
                var canvas = reference.Context().canvas;
                floatingToolbar.style.width = GetCanvasToolbarWidth(canvas);
                ToolbarButton errorButton = null;
                errorButton = CreateFloatingButton(EditorGUIUtility.IconContent("console.erroricon").image, "Clear Errors", () =>
                {
                    var reference = window.reference;
                    foreach (var ed in reference.debugData.elementsData.Where(e => e.runtimeException != null))
                    {
                        ed.runtimeException = null;
                    }
                }, () =>
                {
                    // A bit hacky but using this so I do not need to add another IMGUI container just to detect the developer mode change
                    if (previousDeveloperMode != BoltCore.Configuration.developerMode)
                    {
                        previousDeveloperMode = BoltCore.Configuration.developerMode;
                        errorButton.schedule.Execute(() => RebuildToolbar());
                        return;
                    }

                    var reference = window.reference;
                    var erroredElementsDebugData = ListPool<IGraphElementDebugData>.New();

                    foreach (var elementDebugData in reference.debugData.elementsData)
                    {
                        if (elementDebugData.runtimeException != null)
                        {
                            erroredElementsDebugData.Add(elementDebugData);
                        }
                    }

                    if (erroredElementsDebugData.Count > 0)
                    {
                        errorButton.style.opacity = 1;
                        errorButton.SetEnabled(true);
                    }
                    else
                    {
                        errorButton.style.opacity = 0;
                        errorButton.SetEnabled(false);
                    }

                    erroredElementsDebugData.Free();
                });
                floatingToolbar.Add(errorButton);

                if (canvas is FlowCanvas flowCanvas)
                {
                    var relationsButton = CreateToggleButton(EditorGUIUtility.IconContent("UnityEditor.Graphs.AnimatorControllerTool").image, "Port Relations", flowCanvas.showRelations, v => flowCanvas.showRelations = v);
                    var valuesButton = CreateToggleButton(EditorGUIUtility.IconContent("UnityEditor.InspectorWindow").image, "Flow Values", BoltFlow.Configuration.showConnectionValues, v => { BoltFlow.Configuration.showConnectionValues = v; BoltFlow.Configuration.Save(); });
                    var dimButton = CreateToggleButton(EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image, "Dim Nodes", BoltCore.Configuration.dimInactiveNodes, v => { BoltCore.Configuration.dimInactiveNodes = v; BoltCore.Configuration.Save(); });
                    var carryButton = CreateToggleButton(BoltCore.Icons.window?[IconSize.Small], "Carry Children", BoltCore.Configuration.carryChildren, v => { BoltCore.Configuration.carryChildren = v; BoltCore.Configuration.Save(); });

                    floatingToolbar.Add(relationsButton);
                    floatingToolbar.Add(valuesButton);
                    floatingToolbar.Add(dimButton);
                    floatingToolbar.Add(carryButton);
                }
                else if (canvas is StateCanvas stateCanvas)
                {
                    var dimButton = CreateToggleButton(EditorGUIUtility.IconContent("animationvisibilitytoggleoff").image, "Dim States", BoltCore.Configuration.dimInactiveNodes, v => { BoltCore.Configuration.dimInactiveNodes = v; BoltFlow.Configuration.Save(); });
                    floatingToolbar.Add(dimButton);
                }

                floatingToolbar.Add(CreateEnumButton(PathUtil.Load("Align", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Align", (r) =>
                {
                    LudiqGUI.FuzzyDropdown(r, EnumOptionTree.For<AlignOperation>(), null, op => canvas.Align((AlignOperation)op));
                }, canvas));

                floatingToolbar.Add(CreateEnumButton(PathUtil.Load("Distribute", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Distribute", (r) =>
                {
                    LudiqGUI.FuzzyDropdown(r, EnumOptionTree.For<DistributeOperation>(), null, op => canvas.Distribute((DistributeOperation)op));
                }, canvas));

                floatingToolbar.Add(CreateOverviewButton(PathUtil.Load("Overview", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Overview", () =>
                {
                    GraphUtility.OverrideContextIfNeeded(() =>
                    canvas.ViewElements(reference.graph.elements));
                }));

                floatingToolbar.Add(CreateWindowMaximizeButton(PathUtil.Load("Maximize", CommunityEditorPath.Fundamentals)?[IconSize.Small], "Maximize", window));

                if (BoltCore.Configuration.developerMode)
                {
                    floatingToolbar.Add(CreateToggleButton(typeof(Debug).Icon()?[IconSize.Small], "Debug", BoltCore.Configuration.debug, v =>
                    {
                        BoltCore.Configuration.debug = !BoltCore.Configuration.debug;
                    }));
                }
            }
            RebuildToolbar();
            GraphWindow.activeContextChanged += _ => RebuildToolbar();

            root.Add(floatingToolbar);
        }

        private static ToolbarButton CreateFloatingButton(Texture icon, string tooltip, Action callback, Action imgui)
        {
            var btn = new ToolbarButton(() => callback())
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon as Texture2D;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif
            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = LudiqStyles.spinnerButton.normal.background;
                imgui();
            }));

            return btn;
        }

        private static ToolbarButton CreateToggleButton(Texture icon, string tooltip, bool isOn, Action<bool> callback)
        {
            ToolbarButton btn = null;
            btn = new ToolbarButton(() =>
            {
                isOn = !isOn;
                callback(isOn);
            })
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon as Texture2D;
#else
            if (icon != null)
            {
                var imgContainer = new VisualElement();
                imgContainer.style.flexGrow = 1;
                imgContainer.style.flexShrink = 1;
                imgContainer.style.justifyContent = Justify.Center;
                imgContainer.style.alignItems = Align.Center;

                var img = new Image();
                img.image = icon;
                img.scaleMode = ScaleMode.ScaleToFit;
                img.style.width = StyleKeyword.Auto;
                img.style.height = StyleKeyword.Auto;

                imgContainer.Add(img);
                btn.Add(imgContainer);
            }
#endif
            var imgui = new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = !isOn ? LudiqStyles.spinnerButton.normal.background : LudiqStyles.spinnerButton.active.background;
            });

            btn.Add(imgui);

            return btn;
        }

        private static ToolbarButton CreateWindowMaximizeButton(Texture icon, string tooltip, GraphWindow window)
        {
            ToolbarButton btn = null;
            btn = new ToolbarButton(() =>
            {
                window.maximized = !window.maximized;
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
                GUIUtility.ExitGUI();
            })
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon as Texture2D;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif

            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = !window.maximized ? LudiqStyles.spinnerButton.normal.background : LudiqStyles.spinnerButton.active.background;
            }));

            return btn;
        }

        private static ToolbarButton CreateOverviewButton(Texture2D icon, string tooltip, Action callback)
        {
            var btn = new ToolbarButton(callback)
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif

            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = LudiqStyles.spinnerButton.normal.background;
            }));

            return btn;
        }

        private static ToolbarButton CreateEnumButton(Texture2D icon, string tooltip, Action<Rect> callback, ICanvas canvas)
        {
            bool show = false;
            var btn = new ToolbarButton(() =>
            {
                show = true;
            })
            {
                tooltip = tooltip,
                focusable = false,
                style =
                {
                    width = FloatingToolbarButtonSize,
                    height = FloatingToolbarButtonSize,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100))
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

            btn.Add(new IMGUIContainer(() =>
            {
                btn.style.backgroundImage = LudiqStyles.spinnerButton.normal.background;
                btn.SetEnabled(canvas.selection.Count > 1);

                if (show)
                {
                    show = false;
                    callback?.Invoke(btn.layout);
                }
            }));

#if UNITY_2023_2_OR_NEWER
            if (icon != null)
                btn.iconImage = icon;
#else
            if (icon != null)
            {
                var img = new Image();
                img.image = icon;
                btn.Add(img);
            }
#endif

            return btn;
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
            var size = 0;
            if (BoltCore.Configuration.developerMode)
            {
                size = 1;
            }
            if (canvas is FlowCanvas)
            {
                size += 8;
                return (FloatingToolbarButtonSize * size) + 10;
            }
            else if (canvas is StateCanvas)
            {
                size += 6;
                return (FloatingToolbarButtonSize * size) + 10;
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

            var Toolbar = GetToolbar(window);

            Toolbar.Add(new IMGUIContainer(() =>
            {
                if (window == null || window.context?.canvas == null) return;

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

                    screenRect.y += 14;

                    var sidebars = GetSidebars(window);

                    if (sidebars.left.show)
                        screenRect.x += sidebars.left.GetWidth();

                    Handles.BeginGUI();
                    Handles.DrawSolidRectangleWithOutline(screenRect, Color.clear, new Color(0.3f, 0.8f, 1f, alpha * 0.6f));
                    Handles.EndGUI();
                }
            }));

            var disabledColor = GetDisabledColor();

            Texture2D lockedIconTex = null;
            ToolbarButton lockedButton = null;
            lockedButton = CreateToggleButton("", 30, 20, () => !window.locked ? disabledColor : ColorPalette.unityBackgroundMid, () =>
            {
                window.locked = !window.locked;
                lockedButton.style.backgroundColor = !window.locked ? disabledColor : ColorPalette.unityBackgroundMid;
            });
            IMGUIContainer extractor = null;
            extractor = new IMGUIContainer(() =>
            {
                InitializeNewGUI();
                if (lockedIconTex != null) return;
                try
                {
                    lockedIconTex = GraphGUI.Styles.lockIcon.image as Texture2D;
                }
                catch { }
                lockedButton.style.backgroundImage = lockedIconTex;
                extractor.schedule.Execute(() => extractor.RemoveFromHierarchy());
            });

            ToolbarButton inspectorButton = null;
            inspectorButton = CreateToggleButton("", 30, 20, () => !window.graphInspectorEnabled ? disabledColor : ColorPalette.unityBackgroundMid, () =>
            {
                window.graphInspectorEnabled = !window.graphInspectorEnabled;
                inspectorButton.style.backgroundColor = !window.graphInspectorEnabled ? disabledColor : ColorPalette.unityBackgroundMid;
                window.MatchSelection();
            }, BoltCore.Icons.inspectorWindow?[IconSize.Small]);

            ToolbarButton variablesButton = null;
            variablesButton = CreateToggleButton("", 40, 20, () => !window.variablesInspectorEnabled ? disabledColor : ColorPalette.unityBackgroundMid, () =>
            {
                window.variablesInspectorEnabled = !window.variablesInspectorEnabled;
                variablesButton.style.backgroundColor = !window.variablesInspectorEnabled ? disabledColor : ColorPalette.unityBackgroundMid;
                window.MatchSelection();
            }, BoltCore.Icons.variablesWindow?[IconSize.Small]);

            var breadcrumbContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 20,
                    paddingLeft = 0,
                    paddingRight = 0,
                    marginLeft = 0,
                    marginRight = 0,
                }
            };

            var searchContainer = CreateSearchContainer(window, state);
            Sprite breadCrumbRootIcon = null;
            Sprite breadCrumbIcon = null;

            RebuildBreadcrumbs();

            Toolbar.Add(lockedButton);
            Toolbar.Add(inspectorButton);
            Toolbar.Add(variablesButton);
            Toolbar.Add(breadcrumbContainer);
            Toolbar.Add(searchContainer);
            Toolbar.Add(CreateZoomContainer(window));

            root.Add(extractor);
            root.Add(Toolbar);
            Toolbar.style.position = Position.Relative;
            Toolbar.style.width = new Length(100, LengthUnit.Percent);

            GraphWindow.activeContextChanged += v => RebuildBreadcrumbs();

            void RebuildBreadcrumbs()
            {
                state.reference = window.reference;
                state.highlightTimers.Clear();
                state.matches.Clear();
                state.currentIndex = -1;
                Search(window, state, false);
                breadcrumbContainer.Clear();
                if (state.counterLabel != null)
                    state.counterLabel.text = state.matches.Count == 0 ? "0/0" : $"{state.currentIndex + 1}/{state.matches.Count}";

                if (window.reference == null || !window.reference.isValid) return;

                foreach (var breadcrumb in window.reference.GetBreadcrumbs())
                {
                    bool isCurrent = breadcrumb == window.reference;
                    var btn = new ToolbarButton(() =>
                    {
                        if (!isCurrent)
                        {
                            window.reference = breadcrumb;
                            state.reference = window.reference;
                            state.highlightTimers.Clear();
                            state.matches.Clear();
                            state.currentIndex = -1;
                            Search(window, state, false);
                            RebuildBreadcrumbs();
                        }
                    });

                    if (breadcrumb.isRoot)
                        btn.style.marginLeft = 0;
                    else
                        btn.style.marginLeft = -10;

                    btn.style.marginRight = 0;
                    btn.style.paddingLeft = 10;
#if UNITY_2023_2_OR_NEWER
                    btn.style.paddingRight = 10;
#else
                    btn.style.paddingRight = 0;
#endif
                    btn.style.borderLeftWidth = 0;
                    btn.style.borderRightWidth = 0;

                    btn.style.minWidth = 80;

                    btn.style.borderRightWidth = 0;
                    btn.style.borderLeftWidth = 0;

                    btn.style.unityTextAlign = TextAnchor.MiddleCenter;
                    btn.style.whiteSpace = WhiteSpace.NoWrap;

#if !UNITY_2023_2_OR_NEWER
                    var img = new Image();

                    var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);
                    img.image = title.image;
                    img.AlignSelf(Align.FlexStart);

                    btn.Add(img);

                    btn.style.unityTextOverflowPosition = TextOverflowPosition.End;
                    btn.RegisterCallback<ChangeEvent<string>>(s =>
                    {
                        var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);
                        var size = btn.MeasureTextSize(title.text, btn.resolvedStyle.width, VisualElement.MeasureMode.Undefined, 20, VisualElement.MeasureMode.Exactly).x + 50;
                        if (size > btn.resolvedStyle.minWidth.value)
                            btn.style.width = size;
                        else
                            btn.style.width = 80;
                    });

                    btn.text = (breadcrumb.isRoot ? "       " : " ") + title.text;

                    btn.RegisterCallback<GeometryChangedEvent>(evt =>
                    {
                        var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);

                        var textSize = btn.MeasureTextSize(
                            title.text,
                            float.NaN,
                            VisualElement.MeasureMode.Undefined,
                            20,
                            VisualElement.MeasureMode.Exactly
                        ).x + 50;

                        btn.style.width = Mathf.Max(80, textSize);
                    });
#endif
                    btn.Add(new IMGUIContainer(() =>
                    {
                        if (breadCrumbRootIcon == null || breadCrumbIcon == null)
                        {
                            var root = Styles.toolbarBreadcrumbRoot.normal.background;
                            var child = Styles.toolbarBreadcrumb.normal.background;

                            breadCrumbRootIcon = Sprite.Create(root, new Rect(0, 0, root.width, root.height),
                            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect,
                            new Vector4(10, 0, 10, 0));

                            breadCrumbIcon = Sprite.Create(child, new Rect(0, 0, child.width, child.height),
                            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect,
                            new Vector4(10, 0, 10, 0));
                        }

                        Sprite bgSprite = breadcrumb.isRoot ? breadCrumbRootIcon : breadCrumbIcon;
                        btn.style.backgroundImage = Background.FromSprite(bgSprite);
                        var title = breadcrumb.parent.Description().ToGUIContent(IconSize.Small);
#if !UNITY_2023_2_OR_NEWER
                        btn.text = (breadcrumb.isRoot ? "       " : " ") + title.text;
#else
                        btn.text = title.text;
#endif

#if UNITY_2023_2_OR_NEWER
                        btn.schedule.Execute(() =>
                        {
                            if (title.image is Texture2D iconTex)
                                btn.iconImage = iconTex;
                        });
#endif
                    }));
                    btn.style.unityFontStyleAndWeight = isCurrent ? FontStyle.Bold : FontStyle.Normal;
                    btn.style.backgroundColor = Color.clear;
                    btn.style.height = 20;
                    btn.style.fontSize = 9;
                    btn.focusable = false;

                    breadcrumbContainer.Add(btn);
                }
            }
        }

        private static ToolbarButton CreateToggleButton(string text, float width, float height, Func<Color> backgroundColor, Action callback, Texture2D icon = null)
        {
            var btn = new ToolbarButton(callback)
            {
                text = text,
                focusable = false,
                style =
                {
                    width = width,
                    height = height,
                    backgroundColor = backgroundColor(),
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(16f, 16f)
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };
#if UNITY_2022_2_OR_NEWER
            if (icon != null) btn.style.backgroundImage = icon;
#else
            if (icon != null)
            {
                var imgContainer = new VisualElement();
                imgContainer.style.flexGrow = 1;
                imgContainer.style.flexShrink = 1;
                imgContainer.style.justifyContent = Justify.Center;
                imgContainer.style.alignItems = Align.Center;

                var img = new Image();
                img.image = icon;
                img.scaleMode = ScaleMode.ScaleToFit;
                img.style.width = 16f;
                img.style.height = 16f;

                imgContainer.Add(img);
                btn.Add(imgContainer);
            }
#endif
            btn.RegisterCallback<MouseEnterEvent>(evt => btn.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f));
            btn.RegisterCallback<MouseLeaveEvent>(evt => btn.style.backgroundColor = backgroundColor());
            return btn;
        }

        private static VisualElement CreateSearchContainer(GraphWindow window, SearchState state)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            var searchField = new ToolbarSearchField
            {
                value = state.text ?? "",
                style =
                {
                    height = 15,
                    width = 180
                }
            };

            ToolbarButton prevButton = null;
            ToolbarButton nextButton = null;
            var counterLabel = new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = 50,
                    height = 20
                }
            };

            void UpdateCounter()
            {
                prevButton.SetEnabled(state.matches.Count > 0);
                nextButton.SetEnabled(state.matches.Count > 0);
                counterLabel.text = state.matches.Count == 0 ? "0/0" : $"{state.currentIndex + 1}/{state.matches.Count}";
            }

            state.counterLabel = counterLabel;

            searchField.RegisterValueChangedCallback(ev =>
            {
                state.text = ev.newValue;
                state.matches.Clear();
                state.currentIndex = -1;
                state.highlightTimers.Clear();
                Search(window, state);
                UpdateCounter();
            });

            container.Add(searchField);

            prevButton = CreateNavigationButton("Previous", 65, 20, () =>
            {
                if (state.matches.Count == 0) return;
                state.currentIndex--;
                if (state.currentIndex < 0) state.currentIndex = state.matches.Count - 1;
                HighlightCurrentMatch(window);
                UpdateCounter();
            });

            nextButton = CreateNavigationButton("Next", 45, 20, () =>
            {
                if (state.matches.Count == 0) return;
                state.currentIndex++;
                if (state.currentIndex >= state.matches.Count) state.currentIndex = 0;
                HighlightCurrentMatch(window);
                UpdateCounter();
            });

            container.Add(prevButton);
            container.Add(nextButton);
            container.Add(counterLabel);

            UpdateCounter();

            return container;
        }

        private static ToolbarButton CreateNavigationButton(string text, float width, float height, Action callback)
        {
            var btn = new ToolbarButton(callback)
            {
                text = text,
                focusable = false,
                style =
                {
                    width = width,
                    height = height,
                    fontSize = 11,
                    backgroundColor = Color.clear,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            btn.RegisterCallback<MouseEnterEvent>(evt => btn.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f));
            btn.RegisterCallback<MouseLeaveEvent>(evt => btn.style.backgroundColor = Color.clear);
            return btn;
        }

        private static VisualElement CreateZoomContainer(GraphWindow window)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexEnd,
                    flexGrow = 1
                }
            };

            var zoomLabel = new Label("Zoom")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginRight = 2,
                    fontSize = 11
                }
            };
            container.Add(zoomLabel);

            var zoomSlider = new Slider
            {
                lowValue = GraphGUI.MinZoom,
                highValue = GraphGUI.MaxZoom,
                value = window.context.graph.zoom,
                style = { width = 100 }
            };
            container.Add(zoomSlider);

            var zoomValue = new Label($"{window.context.graph.zoom:0.#}x")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 2,
                    fontSize = 11
                }
            };

            container.Add(zoomValue);

            container.Add(new IMGUIContainer(() =>
            {
                if (window.context == null) return;
                zoomValue.text = $"{window.context.graph.zoom:0.#}x";
                zoomSlider.value = window.context.graph.zoom;
            }));

            zoomSlider.RegisterValueChangedCallback(ev =>
            {
                window.context.graph.zoom = ev.newValue;
                zoomValue.text = $"{ev.newValue:0.#}x";
            });

            return container;
        }

        private static Color GetDisabledColor()
        {
#if DARKER_UI
            return CommunityStyles.backgroundColor;
#else
            return ColorPalette.unityBackgroundLight.color;
#endif
        }

        private static void Search(GraphWindow window, SearchState state, bool tween = true)
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

                if (state.matches.Count > 0 && tween)
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

                var pro = EditorGUIUtility.isProSkin;
                var tabField = typeof(VariablesPanel.Styles).GetField("tab", BindingFlags.Static | BindingFlags.Public);
                var tab = tabField.GetValue(null) as GUIStyle;
                tab.normal.background = CommunityStyles.toolbarButtonBackground;

                tab.hover.background = CommunityStyles.MakeBorderedTexture(CommunityStyles.backgroundColor.Brighten(0.07f),
                pro ? CommunityStyles.backgroundColor.Darken(0.1f) : CommunityStyles.backgroundColor.Brighten(0.1f), BorderSide.LeftRight);

                tab.onNormal.background = CommunityStyles.MakeBorderedTexture(CommunityStyles.backgroundColor.Brighten(0.07f),
                pro ? CommunityStyles.backgroundColor.Darken(0.1f) : CommunityStyles.backgroundColor.Brighten(0.1f), BorderSide.LeftRight);

                tab.alignment = TextAnchor.MiddleLeft;
                tab.padding = new RectOffset(4, 0, 2, 2);
                tab.border = new RectOffset(2, 10, 0, 0);
                tab.overflow = new RectOffset(0, 1, 0, 0);

                tab.fixedHeight = 22;

                var subTabField = typeof(VariablesPanel.Styles).GetField("subTab", BindingFlags.Static | BindingFlags.Public);
                subTabField.SetValue(null, new GUIStyle(tab)
                {
                    alignment = TextAnchor.MiddleCenter
                });
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

#if NEW_UNIT_STYLE
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

                var stateBackground = VisualScripting.StateWidget<FlowState>.Styles.contentBackground;

                var baseStateColor = CommunityStyles.backgroundColor;
                var statePortBackgroundtex = EditorGUIUtility.isProSkin
                    ? CommunityStyles.MakeBorderedTexture(baseStateColor, baseStateColor.Darken(0.05f))
                    : CommunityStyles.MakeBorderedTexture(baseStateColor, baseStateColor.Brighten(0.05f));

                stateBackground.normal.background = statePortBackgroundtex;

#if !ENABLE_VERTICAL_FLOW || !NEW_UNIT_UI
                var unitWidgetGeneric = typeof(Unity.VisualScripting.UnitWidget<>);

                var normalType = VisualScripting.UnitWidget<IUnit>.Styles.portsBackground;

                var baseColor = CommunityStyles.backgroundColor;
                var tex = EditorGUIUtility.isProSkin
                    ? CommunityStyles.MakeBorderedTexture(baseStateColor, baseStateColor.Darken(0.05f))
                    : CommunityStyles.MakeBorderedTexture(baseStateColor, baseStateColor.Brighten(0.05f));

                normalType.normal.background = statePortBackgroundtex;

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

                    portsBackground.normal.background = statePortBackgroundtex;
                }
#endif

#endif
            }
        }
    }
}