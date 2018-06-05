using System;
using Ludiq;
using Bolt;
using UnityEngine;
using UnityEditor;

namespace Lasm.BoltAddons.VariableTags.Editor
{
    
    public class VariableTagUnitInspector : Inspector
    {
        public VariableTagUnitInspector(Metadata metadata) : base(metadata)
        {
            
        }

        protected override float GetHeight(float width, GUIContent label)
        {
           return this.GetHeight(width, label);
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {

            }
        }
    }
}
