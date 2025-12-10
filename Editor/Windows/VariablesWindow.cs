using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class VariablesWindow : SidebarPanelWindow<VariablesPanel>
    {
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
            }
            else
            {
                panel = new VariablesPanel(null);
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
                Repaint();
            }
            else
                panel = new VariablesPanel(null);
        }

        protected override void OnGUI()
        {
            GraphGUIPatch.InitializeNewGUI();
#if DARKER_UI
            if (BoltCore.instance != null && !EditorApplication.isCompiling)
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), CommunityStyles.backgroundColor);
#endif

            if (panel?.context?.reference?.serializedObject.IsUnityNull() ?? false) return;

            base.OnGUI();
        }
    }
}