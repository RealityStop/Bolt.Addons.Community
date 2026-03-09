using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community
{
    internal static class GraphGUIUtilities
    {
        static FieldInfo sidebarsField =
            typeof(GraphWindow).GetField("sidebars", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool DisableUI => BoltCore.instance == null || EditorApplication.isCompiling || PluginContainer.anyVersionMismatch;

        public static Sidebars GetSidebars(GraphWindow window)
        {
            return (Sidebars)sidebarsField.GetValue(window);
        }

        public static VisualElement CreateZoomContainer(GraphWindow window)
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
        public const float FloatingToolbarButtonSize = 28;
        public static float GetCanvasToolbarWidth(ICanvas canvas)
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
    }
}