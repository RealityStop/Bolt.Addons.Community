using UnityEngine;
using UnityEditor;

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

        private readonly GUIStyle marginStyle = new GUIStyle { margin = new RectOffset(10, 10, 10, 10) };

        private GraphLayout _graphLayout;
        private bool _unitStyle;
        private bool _newToolbar;
        private bool _graphMinimap;
        private bool _darkerUI;
        private bool _newVariablesUI;
        private bool _showVariablesQuickbar;
        private bool _newListUI;
        private bool _newDictionaryUI;

        private bool _hasPendingChanges;
        private bool _isInitialized;

        public ProjectSettingsProviderView() : base(Path, SettingsScope.Project)
        {
            label = Title;
        }

        public override void OnGUI(string searchContext)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                LoadValues();
            }

            GUILayout.BeginVertical(marginStyle);

            GUILayout.Label("Graph", EditorStyles.boldLabel);

            DrawEnumField("Graph Layout", ref _graphLayout);
            DrawToggle("New Unit Style", ref _unitStyle);
            DrawToggle("New Toolbar Style", ref _newToolbar);
            DrawToggle("Graph Minimap", ref _graphMinimap);

            GUILayout.Space(10);
            GUILayout.Label("UI", EditorStyles.boldLabel);

            DrawToggle("Darker UI", ref _darkerUI);
            DrawToggle("New Variables UI", ref _newVariablesUI);

            if (_newVariablesUI)
            {
                DrawToggle("Show Variables Quickbar", ref _showVariablesQuickbar);
            }

            DrawToggle("New List UI", ref _newListUI);
            DrawToggle("New Dictionary UI", ref _newDictionaryUI);

            GUILayout.Space(15);
            DrawBottomButtons();

            GUILayout.EndVertical();
        }

        private void LoadValues()
        {
            _graphLayout = (GraphLayout)EditorPrefs.GetInt(GraphLayoutKey, (int)GraphLayout.Horizontal);
            _unitStyle = EditorPrefs.GetBool(UnitStyleKey, false);
            _newToolbar = EditorPrefs.GetBool(NewToolbarKey, false);
            _graphMinimap = EditorPrefs.GetBool(GraphMinimapKey, false);

            _darkerUI = EditorPrefs.GetBool(DarkerUIKey, false);
            _newVariablesUI = EditorPrefs.GetBool(NewVariablesUIKey, false);
            _showVariablesQuickbar = EditorPrefs.GetBool(ShowVariablesQuickbarKey, false);
            _newListUI = EditorPrefs.GetBool(NewListUIKey, false);
            _newDictionaryUI = EditorPrefs.GetBool(NewDictionaryUIKey, false);
        }

        private void DrawEnumField<T>(string label, ref T value) where T : struct
        {
            var newValue = (T)(object)EditorGUILayout.EnumPopup(label, (System.Enum)(object)value);
            if (!Equals(newValue, value))
            {
                value = newValue;
                _hasPendingChanges = true;
            }
        }

        private void DrawToggle(string label, ref bool value)
        {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue != value)
            {
                value = newValue;
                _hasPendingChanges = true;
            }
        }

        private void DrawBottomButtons()
        {
            GUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(!_hasPendingChanges))
            {
                if (GUILayout.Button("Apply", GUILayout.Height(18), GUILayout.Width(50)))
                {
                    ApplyAllChanges();
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(18), GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset to Defaults",
                    "Are you sure you want to reset all customisation settings to their default values?",
                    "Yes, Reset",
                    "Cancel"))
                {
                    ResetToDefaults();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void ApplyAllChanges()
        {
            EditorPrefs.SetInt(GraphLayoutKey, (int)_graphLayout);
            EditorPrefs.SetBool(UnitStyleKey, _unitStyle);
            EditorPrefs.SetBool(NewToolbarKey, _newToolbar);
            EditorPrefs.SetBool(GraphMinimapKey, _graphMinimap);
            EditorPrefs.SetBool(DarkerUIKey, _darkerUI);
            EditorPrefs.SetBool(NewVariablesUIKey, _newVariablesUI);
            EditorPrefs.SetBool(ShowVariablesQuickbarKey, _showVariablesQuickbar);
            EditorPrefs.SetBool(NewListUIKey, _newListUI);
            EditorPrefs.SetBool(NewDictionaryUIKey, _newDictionaryUI);

            ScriptingDefinesHandler.UpdateVerticalFlow();
            ScriptingDefinesHandler.UpdateUnitStyle();
            ScriptingDefinesHandler.UpdateToolbarStyle();
            ScriptingDefinesHandler.UpdateGraphMiniMap();
            ScriptingDefinesHandler.UpdateDarkUI();
            ScriptingDefinesHandler.UpdateVariablesUI();
            ScriptingDefinesHandler.UpdateListUI();
            ScriptingDefinesHandler.UpdateDictionaryUI();

            _hasPendingChanges = false;
        }

        private void ResetToDefaults()
        {
            // Default values
            _graphLayout = GraphLayout.Horizontal;
            _unitStyle = false;
            _newToolbar = false;
            _graphMinimap = false;
            _darkerUI = false;
            _newVariablesUI = false;
            _showVariablesQuickbar = false;
            _newListUI = false;
            _newDictionaryUI = false;

            _hasPendingChanges = true;
        }
    }
}