using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(ControlGraphSnippet))]
    public class ControlGraphSnippetEditor : Editor
    {
        private Metadata arguments;
        void OnEnable()
        {
            if (arguments == null)
            {
                arguments = Metadata.FromProperty(serializedObject.FindProperty("snippetArguments"));
            }
        }
        public override void OnInspectorGUI()
        {
            var snippet = (ControlGraphSnippet)target;

            if (!snippet.graphContainsUnit)
            {
                snippet.graph.units.Add(snippet.SourceUnit());
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Snippet Name:");
            snippet.SnippetName = EditorGUILayout.TextField(snippet.SnippetName);
            EditorGUILayout.EndHorizontal();

            Inspector.BeginBlock(arguments, new Rect());
            LudiqGUI.InspectorLayout(arguments, new GUIContent("Snippet Arguments:"));
            Inspector.EndBlock(arguments);

            if (GUILayout.Button("Edit Graph"))
            {
                GraphWindow.OpenActive(GraphReference.New(snippet, true));
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(snippet);
            }
        }
    }
}