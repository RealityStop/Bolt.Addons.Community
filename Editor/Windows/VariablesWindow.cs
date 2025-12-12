using System;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class VariablesWindow : SidebarPanelWindow<VariablesPanel>
    {
        protected override GUIContent defaultTitleContent => new GUIContent("Variables", BoltCore.Icons.variablesWindow?[IconSize.Small]);
        private static IGraph _currentGraph;
        public static bool isVariablesWindowContext { get; private set; }
        public static IGraph currentGraph
        {
            get
            {
                if (!isVariablesWindowContext) throw new InvalidOperationException(
                    "currentGraph was accessed from outside the Variables Window's execution flow. " +
                    "This property is only valid when the calling code is triggered by the Variables Window."
                );

                return _currentGraph;
            }
            private set => _currentGraph = value;
        }

        [MenuItem("Window/Community Addons/Variables Window")]
        public static void Open()
        {
            var window = GetWindow<VariablesWindow>();
            window.minSize = new Vector2(335, 250);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GraphWindow.activeContextChanged += OnContextChanged;

            if (GraphWindow.activeContext != null)
            {
                panel = new VariablesPanel(GraphWindow.activeContext);
                currentGraph = GraphWindow.activeContext.graph;
            }
            else
            {
                panel = new VariablesPanel(null);
                currentGraph = null;
            }
        }

        protected override void OnDisable()
        {
            GraphWindow.activeContextChanged -= OnContextChanged;
        }

        private void OnContextChanged(IGraphContext context)
        {
            if (context != null)
            {
                panel = new VariablesPanel(context);
                currentGraph = context.graph;
                Repaint();
            }
            else
            {
                panel = new VariablesPanel(null);
                currentGraph = null;
            }
        }

        protected override void OnGUI()
        {
            GraphGUIPatch.InitializeNewGUI();
#if DARKER_UI
            if (BoltCore.instance != null && !EditorApplication.isCompiling)
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), CommunityStyles.backgroundColor);
#endif

            if (panel?.context?.reference?.serializedObject.IsUnityNull() ?? false) return;
            isVariablesWindowContext = true;
            base.OnGUI();
            isVariablesWindowContext = false;
        }
    }
}