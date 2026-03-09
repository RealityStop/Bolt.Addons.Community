using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(Vector2Int))]
    public class Vector2IntInspector : Inspector
    {
        public Vector2IntInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var value = (Vector2Int)metadata.value;

            BeginBlock(metadata, position);
            var newValue = EditorGUI.Vector2IntField(position, label, value);
            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = newValue;
            }
        }
    }
}