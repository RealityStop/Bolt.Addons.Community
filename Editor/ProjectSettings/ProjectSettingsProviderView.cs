using UnityEngine;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    internal class ProjectSettingsProviderView : SettingsProvider
    {
        private const string Path = "Preferences/Visual Scripting/Customisation";
        private const string Title = "Customisation";

        public const string UnitUIKey = "Community_Settings_UnitUI";
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
        private bool _unitUI;
        private bool _unitStyle;
        private bool _newToolbar;
        private bool _graphMinimap;
        private bool _darkerUI;
        private bool _newVariablesUI;
        private bool _showVariablesQuickbar;
        private bool _newListUI;
        private bool _newDictionaryUI;

        private GraphLayout _originalGraphLayout;
        private bool _originalUnitUI;
        private bool _originalUnitStyle;
        private bool _originalNewToolbar;
        private bool _originalGraphMinimap;
        private bool _originalDarkerUI;
        private bool _originalNewVariablesUI;
        private bool _originalShowVariablesQuickbar;
        private bool _originalNewListUI;
        private bool _originalNewDictionaryUI;

        private bool _isInitialized;

        public ProjectSettingsProviderView() : base(Path, SettingsScope.User)
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

            DrawToggle("New Unit UI", ref _unitUI, () =>
            {
                if (!_unitUI)
                {
                    var enable = EditorUtility.DisplayDialog(
                        "Enable New Unit UI?",
                        "Enabling this feature will modify ALL Unit widgets globally.\n\n" +
                        "If your project includes custom Unit widgets, they may no longer function correctly. " +
                        "To ensure compatibility, enable this then make them inherit from:\n\n" +
                        "Unity.VisualScripting.Community.UnitWidget<> instead of Unity.VisualScripting.UnitWidget<>.\n\n" +
                        "If you do not have any custom widgets, this should be safe to enable.\n\n" +
                        "Are you sure you want to proceed?",
                        "Enable",
                        "Cancel"
                    );

                    EditorPrefs.SetBool(UnitUIKey, enable);

                    ScriptingDefinesHandler.UpdateUnitUI();
                    ScriptingDefinesHandler.UpdateVerticalFlow();
                    ScriptingDefinesHandler.UpdateUnitStyle();

                    return enable;
                }

                EditorPrefs.SetBool(UnitUIKey, false);

                ScriptingDefinesHandler.UpdateUnitUI();
                ScriptingDefinesHandler.UpdateVerticalFlow();
                ScriptingDefinesHandler.UpdateUnitStyle();

                return true;
            });
            EditorGUI.BeginDisabledGroup(!_unitUI);
            DrawEnumField("Graph Layout", ref _graphLayout);
            DrawToggle("New Unit Style", ref _unitStyle);
            EditorGUI.EndDisabledGroup();
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
            _originalUnitUI = _unitUI = EditorPrefs.GetBool(UnitUIKey, false);
            _originalGraphLayout = _graphLayout = (GraphLayout)EditorPrefs.GetInt(GraphLayoutKey, (int)GraphLayout.Horizontal);
            _originalUnitStyle = _unitStyle = EditorPrefs.GetBool(UnitStyleKey, false);
            _originalNewToolbar = _newToolbar = EditorPrefs.GetBool(NewToolbarKey, false);
            _originalGraphMinimap = _graphMinimap = EditorPrefs.GetBool(GraphMinimapKey, false);

            _originalDarkerUI = _darkerUI = EditorPrefs.GetBool(DarkerUIKey, false);
            _originalNewVariablesUI = _newVariablesUI = EditorPrefs.GetBool(NewVariablesUIKey, false);
            _originalShowVariablesQuickbar = _showVariablesQuickbar = EditorPrefs.GetBool(ShowVariablesQuickbarKey, false);
            _originalNewListUI = _newListUI = EditorPrefs.GetBool(NewListUIKey, false);
            _originalNewDictionaryUI = _newDictionaryUI = EditorPrefs.GetBool(NewDictionaryUIKey, false);
        }

        private bool HasPendingChanges()
        {
            return
                _unitUI != _originalUnitUI ||
                _graphLayout != _originalGraphLayout ||
                _unitStyle != _originalUnitStyle ||
                _newToolbar != _originalNewToolbar ||
                _graphMinimap != _originalGraphMinimap ||
                _darkerUI != _originalDarkerUI ||
                _newVariablesUI != _originalNewVariablesUI ||
                _showVariablesQuickbar != _originalShowVariablesQuickbar ||
                _newListUI != _originalNewListUI ||
                _newDictionaryUI != _originalNewDictionaryUI;
        }

        private void DrawEnumField<T>(string label, ref T value) where T : struct
        {
            var newValue = (T)(object)EditorGUILayout.EnumPopup(label, (System.Enum)(object)value);
            if (!Equals(newValue, value))
            {
                value = newValue;
            }
        }

        private void DrawToggle(string label, ref bool value, System.Func<bool> shouldChange = null)
        {
            var newValue = EditorGUILayout.Toggle(label, value);
            if (newValue != value && (shouldChange?.Invoke() ?? true))
            {
                value = newValue;
            }
        }

        private void DrawBottomButtons()
        {
            GUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(!HasPendingChanges()))
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
            EditorPrefs.SetBool(UnitUIKey, _unitUI);
            EditorPrefs.SetInt(GraphLayoutKey, (int)_graphLayout);
            EditorPrefs.SetBool(UnitStyleKey, _unitStyle);
            EditorPrefs.SetBool(NewToolbarKey, _newToolbar);
            EditorPrefs.SetBool(GraphMinimapKey, _graphMinimap);
            EditorPrefs.SetBool(DarkerUIKey, _darkerUI);
            EditorPrefs.SetBool(NewVariablesUIKey, _newVariablesUI);
            EditorPrefs.SetBool(ShowVariablesQuickbarKey, _showVariablesQuickbar);
            EditorPrefs.SetBool(NewListUIKey, _newListUI);
            EditorPrefs.SetBool(NewDictionaryUIKey, _newDictionaryUI);

            ScriptingDefinesHandler.UpdateUnitUI();
            ScriptingDefinesHandler.UpdateVerticalFlow();
            ScriptingDefinesHandler.UpdateUnitStyle();
            ScriptingDefinesHandler.UpdateToolbarStyle();
            ScriptingDefinesHandler.UpdateGraphMiniMap();
            ScriptingDefinesHandler.UpdateDarkUI();
            ScriptingDefinesHandler.UpdateVariablesUI();
            ScriptingDefinesHandler.UpdateListUI();
            ScriptingDefinesHandler.UpdateDictionaryUI();

            _originalUnitUI = _unitUI;
            _originalGraphLayout = _graphLayout;
            _originalUnitStyle = _unitStyle;
            _originalNewToolbar = _newToolbar;
            _originalGraphMinimap = _graphMinimap;

            _originalDarkerUI = _darkerUI;
            _originalNewVariablesUI = _newVariablesUI;
            _originalShowVariablesQuickbar = _showVariablesQuickbar;
            _originalNewListUI = _newListUI;
            _originalNewDictionaryUI = _newDictionaryUI;
        }

        private void ResetToDefaults()
        {
            _unitUI = false;
            _graphLayout = GraphLayout.Horizontal;
            _unitStyle = false;
            _newToolbar = false;
            _graphMinimap = false;
            _darkerUI = false;
            _newVariablesUI = false;
            _showVariablesQuickbar = false;
            _newListUI = false;
            _newDictionaryUI = false;
        }
    }
}