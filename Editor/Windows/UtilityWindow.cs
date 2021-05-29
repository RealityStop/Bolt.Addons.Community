using System.Collections;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    public class UtilityWindow : EditorWindow
    {
        [MenuItem("Window/Community Addons/Utilities")]
        public static void Open()
        {
            var window = GetWindow<UtilityWindow>();
            window.titleContent = new GUIContent("UVS Community Utilities");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            SelectionToSuperUnit();
            EditorGUILayout.EndVertical();
        }

        private void SelectionToSuperUnit()
        {
            GUILayout.Label("Selection To Super Unit");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("To Macro"))
            {
                UnitSelection.Convert(GraphSource.Macro);
            }
            if (GUILayout.Button("To Embed"))
            {
                UnitSelection.Convert(GraphSource.Embed);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}