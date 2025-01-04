using System;
using System.Collections.Generic;
using System.Linq;
using GluonGui.Dialog;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(ValueGraphSnippet))]
    public class ValueGraphSnippetEditor : Editor
    {
        private Metadata type;
        private Metadata arguments;
        void OnEnable()
        {
            if (type == null)
            {
                type = Metadata.FromProperty(serializedObject.FindProperty("sourceType"));
            }

            if (arguments == null)
            {
                arguments = Metadata.FromProperty(serializedObject.FindProperty("snippetArguments"));
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
            EditorGUILayout.PrefixLabel("Snippet Name:");
            snippet.SnippetName = EditorGUILayout.TextField(snippet.SnippetName).Replace(" ", "");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Source Type:");
            if(GUILayout.Button(new GUIContent((type["type"].value as Type).DisplayName(), (type["type"].value as Type).Icon()[IconSize.Small])))
            {
                TypeBuilderWindow.ShowWindow(GUILayoutUtility.GetLastRect(), type["type"], true);
            }
            EditorGUILayout.EndHorizontal();

            Inspector.BeginBlock(arguments, new Rect());
            LudiqGUI.InspectorLayout(arguments, new GUIContent("Snippet Arguments:"));
            Inspector.EndBlock(arguments);

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