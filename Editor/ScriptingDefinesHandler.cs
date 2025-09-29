using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;

namespace Unity.VisualScripting.Community
{
    [InitializeOnLoad]
    public static class ScriptingDefinesHandler
    {
        private const string DEFINE_SYMBOL = "RESTRICT_EVENT_TYPES";

        static ScriptingDefinesHandler()
        {
            UpdateDefineSymbol();
        }

        private static void UpdateDefineSymbol()
        {
            bool shouldEnable = CommunityOptionFetcher.DefinedEvent_RestrictEventTypes;

#if UNITY_2022_1_OR_NEWER
            var target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string defines = PlayerSettings.GetScriptingDefineSymbols(target);
#else
            BuildTargetGroup target = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
#endif

            List<string> defineList = defines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();

            if (shouldEnable && !defineList.Contains(DEFINE_SYMBOL))
            {
                defineList.Add(DEFINE_SYMBOL);
            }
            else if (!shouldEnable && defineList.Contains(DEFINE_SYMBOL))
            {
                defineList.Remove(DEFINE_SYMBOL);
            }
            else
            {
                return;
            }

            string newDefines = string.Join(";", defineList);

#if UNITY_2022_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(target, newDefines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, newDefines);
#endif
        }
    }
}
