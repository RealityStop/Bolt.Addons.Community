using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal sealed class GraphMinimapInstance : IDisposable
    {
        private static readonly HashSet<GraphMinimapInstance> activeInstances = new HashSet<GraphMinimapInstance>();
        private static Vector2 MinimapSize
        {
            get => new Vector2(GraphMiniMapStorage.Settings.width, GraphMiniMapStorage.Settings.height);
            set
            {
                GraphMiniMapStorage.Settings.width = value.x;
                GraphMiniMapStorage.Settings.height = value.y;
                GraphMiniMapStorage.MarkDirty();
            }
        }

        private static Vector2 lastMousePos = Vector2.zero;

        private readonly GraphWindow window;
        private IGraphContext context;

        private readonly VisualElement container;
        private readonly IMGUIContainer background;
        private readonly Button toggle;

        private IGraph subscribedGraph;
        private IEnumerable<IGraphElementWidget> widgets = Enumerable.Empty<IGraphElementWidget>();

        public Sidebars sidebars;

        private bool minimized;

        private static readonly Color MinimapBackgroundDark = new Color(0f, 0f, 0f, 0.9f);
        private static readonly Color MinimapBackgroundLight = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Color BorderColor = new Color(1f, 1f, 1f, 0.25f);

        private static FieldInfo sidebarsField = typeof(GraphWindow).GetField("sidebars", BindingFlags.NonPublic | BindingFlags.Instance);

        public GraphMinimapInstance(GraphWindow window)
        {
            activeInstances.Add(this);

            this.window = window;
            this.context = window.context;
            sidebars = (Sidebars)sidebarsField.GetValue(window);

            minimized = GraphMiniMapStorage.Settings
                .minimized
                .TryGetValue(window.reference.ToString(), out var value) && value;

            container = CreateContainer();
            background = CreateRenderer();
            toggle = CreateToggle();

            container.Add(background);
            container.Add(toggle);

            AddResizeHandle(container);

            window.rootVisualElement.Add(container);

            UpdateState();

            Subscribe(context);
        }

        public void Tick()
        {
            if (window.context != context)
            {
                Subscribe(window.context);
            }

            if (!minimized)
            {
                background.MarkDirtyRepaint();
            }

            KeepMinimapAnchored();
        }

        private void Subscribe(IGraphContext newContext)
        {
            if (subscribedGraph != null)
            {
                subscribedGraph.elements.CollectionChanged -= OnElementsChanged;
                subscribedGraph = null;
            }

            context = newContext;

            if (context?.graph == null)
                return;

            subscribedGraph = context.graph;
            subscribedGraph.elements.CollectionChanged += OnElementsChanged;

            CacheWidgets();
        }

        private void OnElementsChanged()
        {
            EditorApplication.delayCall += CacheWidgets;
        }

        private void CacheWidgets()
        {
            if (context?.graph == null)
                return;

            widgets = context.graph.elements.Select(e => context.canvas.Widget(e)).OfType<IGraphElementWidget>();
        }

        private void KeepMinimapAnchored()
        {
            if (window == null || context == null)
                return;

            var canvas = context.canvas;
            if (canvas == null)
                return;
            const float rightMargin = 10f;
            try
            {
                container.style.right = sidebars.right.show ? sidebars.right.GetWidth() + rightMargin : rightMargin;
            }
            catch { }
        }

        private VisualElement CreateContainer()
        {
            bool isDark = EditorGUIUtility.isProSkin;

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
                        width = Mathf.Min(MinimapSize.x, context.canvas.viewport.width - 25),
                        height = Mathf.Min(MinimapSize.y, context.canvas.viewport.height - 25),
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

            return miniMapContainer;
        }

        private IMGUIContainer CreateRenderer()
        {
            var imgui = new IMGUIContainer(Draw);
            imgui.cullingEnabled = false;

            return imgui;
        }

        private Button CreateToggle()
        {
            var button = new Button(() =>
            {
                minimized = !minimized;
                GraphMiniMapStorage.Settings
                    .minimized[window.reference.ToString()] = minimized;

                GraphMiniMapStorage.MarkDirty();
                UpdateState();
            })
            {
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
            button.text = minimized ? "+" : "—";
            return button;
        }

        private void AddResizeHandle(VisualElement container)
        {
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

            container.Add(resizeHandle);

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

                float newWidth = Mathf.Clamp(container.resolvedStyle.width - delta.x, 200, Mathf.Min(600, window.context.canvas.viewport.width - 25));
                float newHeight = Mathf.Clamp(container.resolvedStyle.height + delta.y, 150, Mathf.Min(500, window.context.canvas.viewport.height - 25));

                container.style.width = newWidth;
                container.style.height = newHeight;

                SetGlobalSize(new Vector2(newWidth, newHeight));
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
        }

        private static void SetGlobalSize(Vector2 size)
        {
            MinimapSize = size;
            GraphMiniMapStorage.MarkDirty();

            foreach (var instance in activeInstances)
            {
                instance.ApplySize(size);
            }
        }

        private void ApplySize(Vector2 size)
        {
            if (minimized)
                return;

            container.style.width =
                Mathf.Min(size.x, context.canvas.viewport.width - 25);

            container.style.height =
                Mathf.Min(size.y, context.canvas.viewport.height - 25);

            background.MarkDirtyRepaint();
        }

        private void UpdateState()
        {
            if (container == null || background == null || toggle == null)
                return;

            if (minimized)
            {
                background.style.display = DisplayStyle.None;
                container.style.height = 26;
                container.style.width = 55;
                toggle.text = "+";
                container.Q("MinimapResizeHandle").style.display = DisplayStyle.None;
            }
            else
            {
                background.style.display = DisplayStyle.Flex;
                container.style.width = Mathf.Min(MinimapSize.x, context.canvas.viewport.width - 25);
                container.style.height = Mathf.Min(MinimapSize.y, context.canvas.viewport.height - 25);
                toggle.text = "—";
                container.Q("MinimapResizeHandle").style.display = DisplayStyle.Flex;
            }

            container.MarkDirtyRepaint();
        }

        private void Draw()
        {
            if (minimized)
                return;

            MiniMapRenderer.Draw(context, widgets);
        }

        public void Dispose()
        {
            activeInstances.Remove(this);

            if (subscribedGraph != null)
            {
                subscribedGraph.elements.CollectionChanged -= OnElementsChanged;
            }

            container.RemoveFromHierarchy();
        }
    }
}