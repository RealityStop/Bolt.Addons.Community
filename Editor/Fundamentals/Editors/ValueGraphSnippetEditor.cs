using System;
using System.Collections.Generic;
using System.Linq;
using GluonGui.Dialog;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [CustomEditor(typeof(ValueGraphSnippet))]
    public class ValueGraphSnippetEditor : Editor
    {
        private Metadata type;
        void OnEnable()
        {
            if (type == null)
            {
                type = Metadata.FromProperty(serializedObject.FindProperty("sourceType"));
            }
        }
        public override void OnInspectorGUI()
        {
            var snippet = (ValueGraphSnippet)target;
    
            if (!snippet.graph.units.Contains(snippet.SourceUnit()))
            {
                snippet.graph.units.Add(snippet.SourceUnit());
            }
    
            snippet.sourceUnit.sourceType = snippet.sourceType.type;
            snippet.sourceUnit.Define();
    
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Snippet Name:");
            snippet.SnippetName = EditorGUILayout.TextField(snippet.SnippetName);
            EditorGUILayout.EndHorizontal();
    
            Inspector.BeginBlock(type["type"], new Rect());
            LudiqGUI.InspectorLayout(type["type"], new GUIContent("Source Type:"));
            Inspector.EndBlock(type["type"]);
    
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