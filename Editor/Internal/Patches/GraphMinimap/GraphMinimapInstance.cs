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
        private static Vector2 lastMousePos = Vector2.zero;

        private readonly GraphWindow window;
        private IGraphContext context;

        private readonly VisualElement container;
        private readonly GraphMinimapElement minimapRenderer;
        private readonly Button toggle;

        private IGraph subscribedGraph;
        public Sidebars sidebars;

        private Vector2 lastExpandedSize = new Vector2(GraphMinimapElement.DefaultX, GraphMinimapElement.DefaultY);

        private bool minimized => minimapRenderer == null || !minimapRenderer.value;

        private static readonly Color MinimapBackgroundDark = new Color(0f, 0f, 0f, 0.9f);
        private static readonly Color MinimapBackgroundLight = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Color BorderColor = new Color(1f, 1f, 1f, 0.25f);

        private static FieldInfo sidebarsField = typeof(GraphWindow).GetField("sidebars", BindingFlags.NonPublic | BindingFlags.Instance);

        public GraphMinimapInstance(GraphWindow window)
        {
            this.window = window;
            this.context = window.context;
            sidebars = (Sidebars)sidebarsField.GetValue(window);

            container = CreateContainer();
            minimapRenderer = CreateRenderer();
            toggle = CreateToggle();

            minimapRenderer.RegisterValueChangedCallback(OnMinimapValueChanged);

            container.Add(minimapRenderer);
            container.Add(toggle);

            AddResizeHandle(container);

            window.rootVisualElement.Add(container);
            minimapRenderer.RegisterCallback<GeometryChangedEvent>(Initialize);

            Subscribe(context);
        }

        public void Initialize(GeometryChangedEvent evt)
        {
            UpdateState();

            minimapRenderer.UnregisterCallback<GeometryChangedEvent>(Initialize);
        }

        private void OnMinimapValueChanged(ChangeEvent<bool> evt)
        {
            UpdateState();
        }

        public void Tick()
        {
            if (window.context != context)
            {
                Subscribe(window.context);
            }

            if (!minimized)
            {
                minimapRenderer.UpdateMinimap(context, widgets);
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

        private readonly List<IGraphElementWidget> _cachedWidgets = new List<IGraphElementWidget>();
        public List<IGraphElementWidget> widgets => _cachedWidgets;

        private void CacheWidgets()
        {
            _cachedWidgets.Clear();

            if (context?.graph?.elements == null || context.canvas == null)
                return;

            var elements = context.graph.elements.ToList();
            int count = elements.Count;

            for (int i = 0; i < count; i++)
            {
                var element = elements[i];
                if (element == null) continue;

                var widget = context.canvas.Widget(element);
                if (widget != null)
                {
                    _cachedWidgets.Add(widget);
                }
            }
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
                    width = 55,
                    height = 26,
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

        private GraphMinimapElement CreateRenderer()
        {
            var renderer = new GraphMinimapElement
            {
                style =
                {
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    position = Position.Absolute,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                }
            };
            return renderer;
        }

        private Button CreateToggle()
        {
            var button = new Button(() =>
            {
                minimapRenderer.value = !minimapRenderer.value;
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
                if (minimized) return;
                resizing = true;
                lastMousePos = evt.mousePosition;
                resizeHandle.CaptureMouse();
                evt.StopPropagation();
            });

            resizeHandle.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!resizing || minimized) return;

                Vector2 delta = evt.mousePosition - lastMousePos;
                lastMousePos = evt.mousePosition;

                float maxW = GetSafeMaxViewportWidth();
                float maxH = GetSafeMaxViewportHeight();

                float newWidth = Mathf.Clamp(container.resolvedStyle.width - delta.x, 200, maxW);
                float newHeight = Mathf.Clamp(container.resolvedStyle.height + delta.y, 150, maxH);

                lastExpandedSize = new Vector2(newWidth, newHeight);

                container.style.width = newWidth;
                container.style.height = newHeight;

                ApplySize(lastExpandedSize);
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

        private void ApplySize(Vector2 size)
        {
            if (minimized)
                return;

            float maxW = GetSafeMaxViewportWidth();
            float maxH = GetSafeMaxViewportHeight();

            container.style.width = Mathf.Min(size.x, maxW);
            container.style.height = Mathf.Min(size.y, maxH);

            minimapRenderer.UpdateMinimap(context, widgets);
        }

        private void UpdateState()
        {
            if (container == null || minimapRenderer == null || toggle == null)
                return;

            var resizeHandle = container.Q("MinimapResizeHandle");

            if (minimized)
            {
                minimapRenderer.style.display = DisplayStyle.None;
                container.style.width = 55;
                container.style.height = 26;
                toggle.text = "+";
                if (resizeHandle != null) resizeHandle.style.display = DisplayStyle.None;
            }
            else
            {
                minimapRenderer.style.display = DisplayStyle.Flex;

                float maxW = GetSafeMaxViewportWidth();
                float maxH = GetSafeMaxViewportHeight();

                container.style.width = Mathf.Clamp(lastExpandedSize.x, 200, maxW);
                container.style.height = Mathf.Clamp(lastExpandedSize.y, 150, maxH);

                toggle.text = "—";
                if (resizeHandle != null) resizeHandle.style.display = DisplayStyle.Flex;

                minimapRenderer.UpdateMinimap(context, widgets);
            }

            container.MarkDirtyRepaint();
        }

        private float GetSafeMaxViewportWidth()
        {
            return 600f;
        }

        private float GetSafeMaxViewportHeight()
        {
            return 550f;
        }

        public void Dispose()
        {
            if (minimapRenderer != null)
            {
                minimapRenderer.UnregisterValueChangedCallback(OnMinimapValueChanged);

            }

            if (subscribedGraph != null)
            {
                subscribedGraph.elements.CollectionChanged -= OnElementsChanged;
            }

            container.RemoveFromHierarchy();
        }
    }
}