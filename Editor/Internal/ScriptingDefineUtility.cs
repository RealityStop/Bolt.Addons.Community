using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Profile;

namespace Unity.VisualScripting.Community
{
    [InitializeOnLoad]
    public static class ScriptingDefineUtility
    {
        private const string RESTRICT_EVENT_TYPES_SYMBOL = "RESTRICT_EVENT_TYPES";

        private const string NEW_UNIT_UI = "NEW_UNIT_UI";
        private const string VERTICAL_FLOW = "ENABLE_VERTICAL_FLOW";
        private const string UNIT_STYLE = "NEW_UNIT_STYLE";
        private const string TOOLBAR_STYLE = "NEW_TOOLBAR_STYLE";
        private const string GRAPH_MINIMAP = "ENABLE_GRAPH_MINIMAP";
        private const string NEW_VARIABLES_UI = "NEW_VARIABLES_UI";
        private const string NEW_LIST_UI = "NEW_LIST_UI";
        private const string NEW_DICTIONARY_UI = "NEW_DICTIONARY_UI";

        static ScriptingDefineUtility()
        {
            SetDefine(RESTRICT_EVENT_TYPES_SYMBOL, CommunityOptionFetcher.DefinedEvent_RestrictEventTypes);
            UpdateUnitUI();
            UpdateVerticalFlow();
            UpdateUnitStyle();
            UpdateToolbarStyle();
            UpdateGraphMiniMap();
            UpdateVariablesUI();
            UpdateListUI();
            UpdateDictionaryUI();
        }

        public static void UpdateUnitUI()
        {
            SetDefine(NEW_UNIT_UI, EditorPrefs.GetBool(ProjectSettingsProviderView.UnitUIKey, false));
        }

        public static void UpdateVerticalFlow()
        {
            var set = (GraphLayout)EditorPrefs.GetInt(ProjectSettingsProviderView.GraphLayoutKey, (int)GraphLayout.Horizontal) == GraphLayout.Vertical;
            SetDefine(VERTICAL_FLOW, set && EditorPrefs.GetBool(ProjectSettingsProviderView.UnitUIKey, false));
        }

        public static void UpdateUnitStyle()
        {
            SetDefine(UNIT_STYLE, EditorPrefs.GetBool(ProjectSettingsProviderView.UnitStyleKey, false) && EditorPrefs.GetBool(ProjectSettingsProviderView.UnitUIKey, false));
        }

        public static void UpdateToolbarStyle()
        {
            SetDefine(TOOLBAR_STYLE, EditorPrefs.GetBool(ProjectSettingsProviderView.NewToolbarKey, false));
        }

        public static void UpdateGraphMiniMap()
        {
            SetDefine(GRAPH_MINIMAP, EditorPrefs.GetBool(ProjectSettingsProviderView.GraphMinimapKey, false));
        }

        public static void UpdateVariablesUI()
        {
            SetDefine(NEW_VARIABLES_UI, EditorPrefs.GetBool(ProjectSettingsProviderView.NewVariablesUIKey, false));
        }

        public static void UpdateListUI()
        {
            SetDefine(NEW_LIST_UI, EditorPrefs.GetBool(ProjectSettingsProviderView.NewListUIKey, false));
        }

        public static void UpdateDictionaryUI()
        {
            SetDefine(NEW_DICTIONARY_UI, EditorPrefs.GetBool(ProjectSettingsProviderView.NewDictionaryUIKey, false));
        }

        public static void SetDefine(string symbol, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

#if UNITY_6000_0_OR_NEWER
            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();

            if (activeProfile != null)
            {
                List<string> list = activeProfile.scriptingDefines != null
                    ? activeProfile.scriptingDefines.Where(d => !string.IsNullOrWhiteSpace(d)).ToList()
                    : new List<string>();

                bool changed = false;
                if (enabled && !list.Contains(symbol))
                {
                    list.Add(symbol);
                    changed = true;
                }
                else if (!enabled && list.Contains(symbol))
                {
                    list.Remove(symbol);
                    changed = true;
                }

                if (changed)
                {
                    activeProfile.scriptingDefines = list.ToArray();
                    EditorUtility.SetDirty(activeProfile);
                    AssetDatabase.SaveAssetIfDirty(activeProfile);
                }
                return;
            }
#endif

#if UNITY_2022_1_OR_NEWER
            var target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string definesString = PlayerSettings.GetScriptingDefineSymbols(target);
#else
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
#endif

            List<string> legacyList = definesString.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
            bool legacyChanged = false;

            if (enabled && !legacyList.Contains(symbol))
            {
                legacyList.Add(symbol);
                legacyChanged = true;
            }
            else if (!enabled && legacyList.Contains(symbol))
            {
                legacyList.Remove(symbol);
                legacyChanged = true;
            }

            if (!legacyChanged) return;

            string result = string.Join(";", legacyList);

#if UNITY_2022_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(target, result);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, result);
#endif
        }
    }
}
