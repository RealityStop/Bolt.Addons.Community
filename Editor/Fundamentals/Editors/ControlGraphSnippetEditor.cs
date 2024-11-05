using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [CustomEditor(typeof(ControlGraphSnippet))]
    public class ControlGraphSnippetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Reference to the target object
            var snippet = (ControlGraphSnippet)target;
    
            if (!snippet.graph.units.Contains(snippet.SourceUnit()))
            {
                snippet.graph.units.Add(snippet.SourceUnit());
            }
    
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Snippet Name:");
            snippet.SnippetName = EditorGUILayout.TextField(snippet.SnippetName);
            EditorGUILayout.EndHorizontal();
    
            if (GUILayout.Button("Edit Graph"))
            {
                GraphWindow.OpenActive(GraphReference.New(snippet, true));
            }
    
            // Apply changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(snippet);
            }
        }
    } 
}