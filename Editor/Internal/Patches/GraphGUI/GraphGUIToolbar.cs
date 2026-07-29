using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUIToolbar
    {
        private static readonly Color BackgroundColor = EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);
        private static readonly Color HoverColor = EditorGUIUtility.isProSkin ? new Color32(76, 76, 76, 255) : new Color32(214, 214, 214, 255);
        private static readonly Color ActiveColor = EditorGUIUtility.isProSkin ? new Color32(88, 88, 88, 255) : new Color32(150, 150, 150, 255);
        private static readonly Color ActiveHoverColor = EditorGUIUtility.isProSkin ? new Color32(108, 108, 108, 255) : new Color32(130, 130, 130, 255);

        public static void Build(VisualElement root, GraphWindow window)
        {
            var state = GraphGUIState.Get(window);

            if (state.Toolbar != null)
                return;

            var toolbar = CreateToolbar();
            state.Toolbar = toolbar;

            ToolbarGUI(root, window, toolbar, state);

            root.Add(toolbar);
        }

        static VisualElement CreateToolbar()
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

        private static SearchState GetSearchState(GraphWindow window)
        {
            var state = GraphGUIState.Get(window);
            if (state == null) return null;

            if (state.SearchState.reference != window.reference)
            {
                state.SearchState.reference = window.reference;
                state.SearchState.highlightTimers?.Clear();
                state.SearchState.matches.Clear();
            }

            return state.SearchState;
        }

        static void ToolbarGUI(VisualElement root, GraphWindow window, VisualElement toolbar, WindowState windowState)
        {
            var state = windowState.SearchState;
            toolbar.Add(new IMGUIContainer(() =>
            {
                if (window == null || window.context?.canvas == null) return;

                windowState.SetContext(window.context);

                if (state.reference != window.reference)
                {
                    state = GetSearchState(window);
                }

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

                    var sidebars = GraphGUIUtilities.GetSidebars(window);

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
            lockedButton = CreateToggleButton("", 30, 20, () => window.locked, (val) => window.locked = val);
            IMGUIContainer initializer = null;
            initializer = new IMGUIContainer(() =>
            {
                try
                {
                    GraphGUIStyles.InitializeNewGUI();
                    if (lockedIconTex != null)
                        return;

                    try
                    {
                        lockedIconTex = GraphGUI.Styles.lockIcon.image as Texture2D;
                    }
                    catch { }
                    lockedButton.style.backgroundImage = lockedIconTex;
                }
                finally
                {
                    initializer.schedule.Execute(() => initializer.RemoveFromHierarchy());
                }
            });

            ToolbarButton inspectorButton = null;
            inspectorButton = CreateToggleButton("", 30, 20, () => window.graphInspectorEnabled,
                (val) =>
                {
                    window.graphInspectorEnabled = val;
                    window.MatchSelection();
                }, BoltCore.Icons.inspectorWindow?[IconSize.Small]);

            ToolbarButton variablesButton = null;
            variablesButton = CreateToggleButton("", 40, 20, () => window.variablesInspectorEnabled,
                (val) =>
                {
                    window.variablesInspectorEnabled = val;
                    window.MatchSelection();
                }, BoltCore.Icons.variablesWindow?[IconSize.Small]);

            toolbar.Add(lockedButton);
            toolbar.Add(inspectorButton);
            toolbar.Add(variablesButton);

            root.Add(initializer);

            toolbar.style.position = Position.Relative;
            toolbar.style.width = new Length(100, LengthUnit.Percent);

            toolbar.Add(GraphGUIBreadcrumbs.Create(window, windowState, toolbar));
            toolbar.Add(GraphGUISearch.Create(window));
            toolbar.Add(GraphGUIUtilities.CreateZoomContainer(window));
        }

        private static ToolbarButton CreateToggleButton(string text, float width, float height, Func<bool> getter, Action<bool> setter, Texture2D icon = null)
        {
            var btn = new ToolbarButton()
            {
                text = text,
                focusable = false,
                style =
                {
                    width = width,
                    height = height,
                    backgroundColor = getter() ? ActiveColor : BackgroundColor,
#if UNITY_2022_2_OR_NEWER
                    backgroundSize = new BackgroundSize(16f, 16f)
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                }
            };

            void UpdateColor(bool isHovering)
            {
                bool isActive = getter();
                if (isHovering)
                    btn.style.backgroundColor = isActive ? ActiveHoverColor : HoverColor;
                else
                    btn.style.backgroundColor = isActive ? ActiveColor : BackgroundColor;
            }

            btn.clicked += () =>
            {
                setter(!getter());
                UpdateColor(true);
            };

            btn.RegisterCallback<MouseEnterEvent>(evt => UpdateColor(true));
            btn.RegisterCallback<MouseLeaveEvent>(evt => UpdateColor(false));

            if (icon != null)
            {
#if UNITY_2022_2_OR_NEWER
                btn.style.backgroundImage = icon;
#else
                var img = new Image { image = icon, scaleMode = ScaleMode.ScaleToFit };
                img.style.width = 16f; img.style.height = 16f;
                btn.Add(img);
#endif
            }

            return btn;
        }

        private static Color GetDisabledColor()
        {
#if DARKER_UI
            return CommunityStyles.backgroundColor;
#else
            return ColorPalette.unityBackgroundLight.color;
#endif
        }
    }
}