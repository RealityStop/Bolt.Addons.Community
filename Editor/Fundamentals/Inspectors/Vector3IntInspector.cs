using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(Vector3Int))]
    public class Vector3IntInspector : Inspector
    {
        public Vector3IntInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var value = (Vector3Int)metadata.value;

            BeginBlock(metadata, position);
            var newValue = EditorGUI.Vector3IntField(position, label, value);
            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = newValue;
            }
        }
    }
}