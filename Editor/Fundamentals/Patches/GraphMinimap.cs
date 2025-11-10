using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    [InitializeAfterPlugins]
    public static class GraphMiniMap
    {
        private class MiniMapInstance
        {
            public GraphWindow window;
            public IGraphContext context;
            public VisualElement container;
            public IMGUIContainer background;
            public Button toggleButton;
            public bool isMinimized;
            public Sidebars sidebars;
            public VisualElement root;
            public Vector2 lastPan;
            public float lastZoom;
        }

        private static readonly Dictionary<GraphWindow, MiniMapInstance> instances = new Dictionary<GraphWindow, MiniMapInstance>();

        private static readonly float Padding = 50f;
        private static readonly Color MinimapBackgroundDark = new Color(0f, 0f, 0f, 0.9f);
        private static readonly Color MinimapBackgroundLight = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Color BorderColor = new Color(1f, 1f, 1f, 0.25f);

        private static readonly Dictionary<NodeColor, Color> colorMap = new Dictionary<NodeColor, Color>()
        {
            { NodeColor.Gray, new Color(0.5f, 0.5f, 0.5f) },
            { NodeColor.Blue, new Color(0.25f, 0.6f, 1f) },
            { NodeColor.Teal, new Color(0f, 0.75f, 0.75f) },
            { NodeColor.Green, new Color(0.4f, 0.8f, 0.4f) },
            { NodeColor.Yellow, new Color(1f, 0.9f, 0.2f) },
            { NodeColor.Orange, new Color(1f, 0.6f, 0.2f) },
            { NodeColor.Red, new Color(1f, 0.3f, 0.3f) }
        };

        private static readonly Dictionary<Type, PropertyInfo> colorPropertyCache = new Dictionary<Type, PropertyInfo>();
        private static readonly List<IGraphElementWidget> tempWidgets = new List<IGraphElementWidget>();

        private static Vector2 lastMousePos = Vector2.zero;

        private static Vector2 MinimapSize
        {
            get => new Vector2(GraphMiniMapStorage.Settings.width, GraphMiniMapStorage.Settings.height);
            set
            {
                if (GraphMiniMapStorage.Settings.width == value.x && GraphMiniMapStorage.Settings.height == value.y)
                    return;

                GraphMiniMapStorage.Settings.width = value.x;
                GraphMiniMapStorage.Settings.height = value.y;
                GraphMiniMapStorage.MarkDirty();
            }
        }

        private static double lastUpdateTime;
        private static double lastCleanupTime;
        private const double UpdateInterval = 0.25;
        private const double CleanupInterval = 5.0;

        static GraphMiniMap()
        {
#if ENABLE_GRAPH_MINIMAP
            GraphMiniMapStorage.Load();
            EditorApplication.update += OnEditorUpdate;
#endif
        }

        private static void OnEditorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            bool mustClean = BoltCore.instance == null || EditorApplication.isCompiling;
            if (now - lastUpdateTime < UpdateInterval && !mustClean) return;
            lastUpdateTime = now;

            if (mustClean)
            {
                Cleanup();
                foreach (var instance in instances.Values)
                {
                    instance.container?.RemoveFromHierarchy();
                }
                return;
            }

            var tabs = GraphWindow.tabs;
            if (tabs == null || !tabs.Any()) return;

            foreach (var window in tabs)
            {
                if (window == null && window.context == null && window.reference == null) continue;

                if (!instances.TryGetValue(window, out var instance))
                {
                    if (window.context != null)
                    {
                        instance = CreateInstance(window);
                        instances[window] = instance;
                    }
                }
                else if (instance.context != window.context)
                {
                    instance.context = window.context;
                    instance.background.MarkDirtyRepaint();
                }
            }

            foreach (var kvp in instances)
            {
                var inst = kvp.Value;

                if (inst.container?.parent == null)
                    inst.window.rootVisualElement.Add(inst.container);

                var canvas = inst.context?.canvas;
                if (canvas == null) continue;

                if (canvas.pan != inst.lastPan || canvas.zoom != inst.lastZoom)
                {
                    inst.lastPan = canvas.pan;
                    inst.lastZoom = canvas.zoom;
                    inst.background?.MarkDirtyRepaint();
                }

                UpdateMinimapState(inst);
                KeepMinimapAnchored(inst);
            }

            if (now - lastCleanupTime > CleanupInterval)
            {
                Cleanup();
                lastCleanupTime = now;
            }

            GraphMiniMapStorage.AutoSaveIfNeeded();
        }

        private static void SetMinimizedKey(string reference, bool minimized)
        {
            var settings = GraphMiniMapStorage.Settings;
            settings.minimized[reference] = minimized;
            GraphMiniMapStorage.MarkDirty();
        }

        private static bool GetMinimizedKey(string reference)
        {
            var settings = GraphMiniMapStorage.Settings;
            if (settings.minimized.TryGetValue(reference, out bool minimized))
                return minimized;
            return false;
        }

        private static MiniMapInstance CreateInstance(GraphWindow window)
        {
            var instance = new MiniMapInstance
            {
                sidebars = (Sidebars)sidebarsField.GetValue(window),
                root = window.rootVisualElement,
                window = window,
                context = window.context,
                isMinimized = GetMinimizedKey(window.reference.ToString())
            };

            bool isDark = EditorGUIUtility.isProSkin;
            var root = instance.root;

            var miniMapContainer = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
#if NEW_TOOLBAR_STYLE
                    top = 60,
#else
                    top = 30,
#endif
                    right = 10,
                    width = Mathf.Min(MinimapSize.x, instance.context.canvas.viewport.width - 25),
                    height = Mathf.Min(MinimapSize.y, instance.context.canvas.viewport.height - 25),
                    backgroundColor = isDark ? MinimapBackgroundDark : MinimapBackgroundLight,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = BorderColor,
                    borderLeftColor = BorderColor,
                    borderRightColor = BorderColor,
                    borderTopColor = BorderColor
                },
                pickingMode = PickingMode.Ignore
            };

            var background = new IMGUIContainer(() => DrawMiniMap(instance));
            background.cullingEnabled = false;
            miniMapContainer.Add(background);

            var toggleButton = new Button(() =>
            {
                instance.isMinimized = !instance.isMinimized;
                foreach (var otherInstance in instances.Where(i => i.Value.window.reference == instance.window.reference && i.Value != instance))
                {
                    otherInstance.Value.isMinimized = instance.isMinimized;
                }
                SetMinimizedKey(instance.window.reference.ToString(), instance.isMinimized);
                UpdateMinimapState(instance);
            })
            {
                text = instance.isMinimized ? "+" : "—",
                style =
                {
                    position = Position.Absolute,
                    top = 2,
                    right = 1,
                    width = 18,
                    height = 18,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    backgroundColor = new Color(0, 0, 0, 0.3f),
                    color = Color.white,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            miniMapContainer.Add(toggleButton);

            var resizeHandle = new VisualElement
            {
                name = "MinimapResizeHandle",
                style =
                {
                    position = Position.Absolute,
                    bottom = 0,
                    left = 0,
                    width = 12,
                    height = 12,
                    backgroundColor = Color.clear,
                    cursor = UIElementsCursorUpdater.DefaultCursor(UIElementsCursorUpdater.CursorType.ResizeUpRight)
                }
            };
            miniMapContainer.Add(resizeHandle);

            bool resizing = false;
            resizeHandle.RegisterCallback<MouseDownEvent>(evt =>
            {
                resizing = true;
                lastMousePos = evt.mousePosition;
                resizeHandle.CaptureMouse();
                evt.StopPropagation();
            });

            resizeHandle.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!resizing) return;

                Vector2 delta = evt.mousePosition - lastMousePos;
                lastMousePos = evt.mousePosition;

                float newWidth = Mathf.Clamp(miniMapContainer.resolvedStyle.width - delta.x, 200, Mathf.Min(600, instance.window.context.canvas.viewport.width - 25));
                float newHeight = Mathf.Clamp(miniMapContainer.resolvedStyle.height + delta.y, 150, Mathf.Min(500, instance.window.context.canvas.viewport.height - 25));

                miniMapContainer.style.width = newWidth;
                miniMapContainer.style.height = newHeight;

                MinimapSize = new Vector2(newWidth, newHeight);
                background.MarkDirtyRepaint();
                evt.StopPropagation();
            });

            resizeHandle.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!resizing) return;
                resizing = false;
                resizeHandle.ReleaseMouse();
                evt.StopPropagation();
            });

            root.Add(miniMapContainer);

            instance.container = miniMapContainer;
            instance.background = background;
            instance.toggleButton = toggleButton;

            return instance;
        }

        private static FieldInfo sidebarsField = typeof(GraphWindow).GetField("sidebars", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void KeepMinimapAnchored(MiniMapInstance instance)
        {
            if (instance?.container == null || instance?.window == null || instance?.context == null)
                return;

            var canvas = instance.context.canvas;
            if (canvas == null)
                return;
            const float rightMargin = 10f;
            try
            {
                instance.container.style.right = instance.sidebars.right.show ? instance.sidebars.right.GetWidth() + rightMargin : rightMargin;
            }
            catch { }
        }

        private static void UpdateMinimapState(MiniMapInstance instance)
        {
            if (instance.container == null || instance.background == null || instance.toggleButton == null)
                return;

            if (instance.isMinimized)
            {
                instance.background.style.display = DisplayStyle.None;
                instance.container.style.height = 26;
                instance.container.style.width = 55;
                instance.toggleButton.text = "+";
                instance.container.Q("MinimapResizeHandle").style.display = DisplayStyle.None;
            }
            else
            {
                instance.background.style.display = DisplayStyle.Flex;
                instance.container.style.width = Mathf.Min(MinimapSize.x, instance.context.canvas.viewport.width - 25);
                instance.container.style.height = Mathf.Min(MinimapSize.y, instance.context.canvas.viewport.height - 25);
                instance.toggleButton.text = "—";
                instance.container.Q("MinimapResizeHandle").style.display = DisplayStyle.Flex;
            }

            instance.container.MarkDirtyRepaint();
        }

        private static IGraphElementWidget selectedWidget;

        private static void CleanupUnusedPrefs()
        {
            var activeRefs = new HashSet<string>(instances.Values.Where(i => i.window != null).Select(i => i.window.reference.ToString()));
            var minimized = GraphMiniMapStorage.Settings.minimized;

            var toRemove = minimized.Keys.Where(k => !activeRefs.Contains(k)).ToList();
            foreach (var key in toRemove)
                minimized.Remove(key);

            if (toRemove.Count > 0)
                GraphMiniMapStorage.MarkDirty();
        }

        private static void Cleanup()
        {
            CleanupUnusedPrefs();
            var closed = new List<GraphWindow>();
            foreach (var kv in instances)
                if (kv.Key == null)
                    closed.Add(kv.Key);

            foreach (var w in closed)
            {
                instances.Remove(w);
            }
        }

        private static void DrawMiniMap(MiniMapInstance instance)
        {
            if (instance.isMinimized) return;

            var context = instance.context;
            if (context?.graph == null) return;

            var graph = context.graph;

            var canvas = graph.Canvas();
            if (canvas == null) return;

            Rect rect = GUILayoutUtility.GetRect(MinimapSize.x, MinimapSize.y);
            GUI.BeginGroup(rect);
            Handles.color = Color.white * 0.4f;

            tempWidgets.Clear();
            foreach (var e in graph.elements)
                if (canvas.widgetProvider.IsValid(e) && canvas.Widget(e) is IGraphElementWidget w)
                    tempWidgets.Add(w);

            if (tempWidgets.Count == 0)
            {
                GUI.EndGroup();
                return;
            }

            Rect contentBounds = GraphGUI.CalculateArea(tempWidgets);
            contentBounds.xMin -= Padding;
            contentBounds.yMin -= Padding;
            contentBounds.xMax += Padding;
            contentBounds.yMax += Padding;

            Rect viewportWorld = new Rect(canvas.pan - canvas.viewport.size * 0.5f, canvas.viewport.size);
            Rect combinedBounds = contentBounds.Encompass(viewportWorld);

            float scale = Mathf.Min(rect.width / combinedBounds.width, rect.height / combinedBounds.height);
            Vector2 minimapOffset = rect.center - 0.5f * scale * combinedBounds.size;

            Vector2 ToMinimap(Vector2 worldPos) => (worldPos - combinedBounds.min) * scale + minimapOffset;

            GraphUtility.OverrideContextIfNeeded(() =>
            {
                Rect position = default;
                List<IGraphElementWidget> hitWidgets = new List<IGraphElementWidget>();
                Vector2 mousePos = Event.current.mousePosition;

                foreach (var widget in tempWidgets)
                {
                    if (!canvas.widgetProvider.IsValid(widget.element) || !GraphContextProvider.instance.IsValid(instance.context.reference)) continue;
                    Rect wp = widget.position;
                    position.position = ToMinimap(wp.position);
                    position.size = wp.size * scale;

                    Handles.DrawSolidRectangleWithOutline(
                        position,
                        GetElementColor(widget).WithAlpha(0.5f),
                        Color.white * (canvas.selection.Contains(widget.element) ? 1f : 0.25f)
                    );

                    if (position.Contains(mousePos))
                        hitWidgets.Add(widget);
                }

                var e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0 && hitWidgets.Count > 0)
                {
                    int nextIndex = 0;
                    if (selectedWidget != null)
                    {
                        int currentIndex = hitWidgets.IndexOf(selectedWidget);
                        nextIndex = (currentIndex + 1) % hitWidgets.Count;
                    }

                    selectedWidget = hitWidgets[nextIndex];
                    canvas.ViewElements(selectedWidget.element.Yield());

                    if (e.shift)
                        canvas.selection.Add(selectedWidget.element);
                    else
                        canvas.selection.Select(selectedWidget.element);

                    e.Use();
                }
            });

            Vector2 viewPos = ToMinimap(viewportWorld.position);
            Vector2 viewSize = viewportWorld.size * scale;
            Rect viewRect = new Rect(viewPos, viewSize);
            viewRect = Rect.MinMaxRect(
                Mathf.Max(viewRect.xMin, 0),
                Mathf.Max(viewRect.yMin, 0),
                Mathf.Min(viewRect.xMax, MinimapSize.x),
                Mathf.Min(viewRect.yMax, MinimapSize.y)
            );

            Handles.DrawSolidRectangleWithOutline(viewRect, new Color(1f, 1f, 1f, 0.1f), Color.yellow);
            GUI.EndGroup();
        }

        private static Color GetElementColor(IWidget widget)
        {
            if (widget is IUnitWidget unitWidget)
            {
                if (unitWidget is CommentNodeWidget commentNodeWidget)
                    return commentNodeWidget.element.color;
                else if (unitWidget is ArrowWidget arrowWidget)
                    return arrowWidget.element.Color;

                Color result = new NodeColorMix(NodeColor.Gray).ToColor();
                try
                {
                    GraphUtility.OverrideContextIfNeeded(() =>
                    {
                        NodeColorMix mix = default;
                        var type = widget.GetType();
                        if (!colorPropertyCache.TryGetValue(type, out var prop))
                        {
                            prop = type.GetProperty("color", BindingFlags.NonPublic | BindingFlags.Instance);
                            colorPropertyCache[type] = prop;
                        }

                        if (prop != null)
                            mix = (NodeColorMix)prop.GetValue(widget);

                        result = mix.ToColor();
                    });
                }
                catch
                {
                    return result;
                }
                return result;
            }
            else if (widget is GraphGroupWidget group)
            {
                return group.element.color;
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (widget is StickyNoteWidget sticky)
            {
                return StickyNote.GetStickyColor(sticky.element.colorTheme);
            }
#endif
            return Color.white;
        }

        public static Color ToColor(this NodeColorMix mix)
        {
            mix = mix.normalized;
            Color result = Color.black;
            foreach (var kvp in mix)
                if (colorMap.TryGetValue(kvp.Key, out var c))
                    result += c * kvp.Value;

            result.r = Mathf.Clamp01(result.r);
            result.g = Mathf.Clamp01(result.g);
            result.b = Mathf.Clamp01(result.b);
            result.a = 1f;
            return result;
        }
    }
}