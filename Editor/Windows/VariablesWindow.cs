using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    public class VariablesWindow : SidebarPanelWindow<VariablesPanel>
    {
        private bool needsOpen;

        protected override GUIContent defaultTitleContent => new GUIContent("Variables", BoltCore.Icons.variablesWindow?[IconSize.Small]);

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
                needsOpen = false;
            }
            else
                needsOpen = true;
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
                Repaint();
            }
            else
                panel = null;
        }

        protected override void OnGUI()
        {
            GraphGUIPatch.InitializeNewGUI();
#if DARKER_UI
            if (BoltCore.instance != null && !EditorApplication.isCompiling)
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), CommunityStyles.backgroundColor);
#endif
            if (GraphWindow.tabs.Count() > 0 && needsOpen)
            {
                try
                {
                    var tab = GraphWindow.tabs.First();
                    if (tab.reference != null)
                    {
                        panel = new VariablesPanel(tab.reference.Context());
                        needsOpen = false;
                    }
                }
                catch (InvalidOperationException) { }
            }

            if (needsOpen)
            {
                EditorGUILayout.HelpBox("Open a Graph to display it's variables.", MessageType.Info);
                return;
            }
            base.OnGUI();
        }
    }
}