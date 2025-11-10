using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace Unity.VisualScripting.Community
{
    internal class ProjectSettingsProviderView : SettingsProvider
    {
        private const string Path = "Project/Visual Scripting/Customisation";
        private const string Title = "Customisation";

        public const string GraphLayoutKey = "Community_Settings_GraphLayout";
        public const string UnitStyleKey = "Community_Settings_UnitStyle";
        public const string NewToolbarKey = "Community_Settings_NewToolbar";
        public const string GraphMinimapKey = "Community_Settings_GraphMinimap";

        public const string DarkerUIKey = "Community_Settings_DarkerUI";
        public const string NewVariablesUIKey = "Community_Settings_NewVariablesUI";
        public const string ShowVariablesQuickbarKey = "Community_Settings_ShowVariablesQuickbar";
        public const string NewListUIKey = "Community_Settings_NewListUI";
        public const string NewDictionaryUIKey = "Community_Settings_NewDictionaryUI";

        private readonly GUIStyle marginStyle = new GUIStyle() { margin = new RectOffset(10, 10, 10, 10) };

        public ProjectSettingsProviderView() : base(Path, SettingsScope.Project)
        {
            label = Title;
        }

        public override void OnGUI(string searchContext)
        {
            GUILayout.BeginVertical(marginStyle);

            GUILayout.Label("Graph", EditorStyles.boldLabel);

            var currentLayout = (GraphLayout)EditorPrefs.GetInt(GraphLayoutKey, (int)GraphLayout.Horizontal);
            var newGraphLayout = (GraphLayout)EditorGUILayout.EnumPopup("Graph Layout", currentLayout);
            if (newGraphLayout != currentLayout)
            {
                EditorPrefs.SetInt(GraphLayoutKey, (int)newGraphLayout);
                ScriptingDefinesHandler.UpdateVerticalFlow();
            }

            var currentStyle = EditorPrefs.GetBool(UnitStyleKey, false);
            var newStyle = EditorGUILayout.Toggle("New Unit Style", currentStyle);
            if (newStyle != currentStyle)
            {
                EditorPrefs.SetBool(UnitStyleKey, newStyle);
                ScriptingDefinesHandler.UpdateUnitStyle();
            }

            var currentToolbar = EditorPrefs.GetBool(NewToolbarKey, false);
            var newToolbar = EditorGUILayout.Toggle("New Toolbar Style", currentToolbar);
            if (newToolbar != currentToolbar)
            {
                EditorPrefs.SetBool(NewToolbarKey, newToolbar);
                ScriptingDefinesHandler.UpdateToolbarStyle();
            }

            var currentGraphMinimap = EditorPrefs.GetBool(GraphMinimapKey, false);
            var newGraphMinimap = EditorGUILayout.Toggle("Graph Minimap", currentGraphMinimap);
            if (newGraphMinimap != currentGraphMinimap)
            {
                EditorPrefs.SetBool(GraphMinimapKey, newGraphMinimap);
                ScriptingDefinesHandler.UpdateGraphMiniMap();
            }

            GUILayout.Label("UI", EditorStyles.boldLabel);

            var currentDarkUI = EditorPrefs.GetBool(DarkerUIKey, false);
            var newDarkUI = EditorGUILayout.Toggle("Darker UI", currentDarkUI);
            if (newDarkUI != currentDarkUI)
            {
                EditorPrefs.SetBool(DarkerUIKey, newDarkUI);
                ScriptingDefinesHandler.UpdateDarkUI();
            }

            var currentVariablesUI = EditorPrefs.GetBool(NewVariablesUIKey, false);
            var newVariablesUI = EditorGUILayout.Toggle("New Variables UI", currentVariablesUI);
            if (newVariablesUI != currentVariablesUI)
            {
                EditorPrefs.SetBool(NewVariablesUIKey, newVariablesUI);
                ScriptingDefinesHandler.UpdateVariablesUI();
            }

            if (newVariablesUI)
            {
                var currentQuickbar = EditorPrefs.GetBool(ShowVariablesQuickbarKey, false);
                var newQuickbar = EditorGUILayout.Toggle("Show Variables Quickbar", currentQuickbar);
                if (newQuickbar != currentQuickbar)
                {
                    EditorPrefs.SetBool(ShowVariablesQuickbarKey, newQuickbar);
                }
            }

            var currentListUI = EditorPrefs.GetBool(NewListUIKey, false);
            var newListUI = EditorGUILayout.Toggle("New List UI", currentListUI);
            if (newListUI != currentListUI)
            {
                EditorPrefs.SetBool(NewListUIKey, newListUI);
                ScriptingDefinesHandler.UpdateListUI();
            }

            var currentDictionaryUI = EditorPrefs.GetBool(NewDictionaryUIKey, false);
            var newDictionaryUI = EditorGUILayout.Toggle("New Dictionary UI", currentDictionaryUI);
            if (newDictionaryUI != currentDictionaryUI)
            {
                EditorPrefs.SetBool(NewDictionaryUIKey, newDictionaryUI);
                ScriptingDefinesHandler.UpdateDictionaryUI();
            }

            GUILayout.EndVertical();
        }
    }
}