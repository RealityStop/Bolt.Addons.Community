using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(Vector2Int))]
    public class Vector2IntInspector : VectorInspector
    {
        public Vector2IntInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var value = (Vector2Int)metadata.value;

            BeginBlock(metadata, position);
            Vector2Int newValue;

            if (position.width <= Styles.compactThreshold)
            {
                newValue = CompactVector2IntField(position, GUIContent.none, (Vector2Int)metadata.value);
            }
            else
            {
                newValue = EditorGUI.Vector2IntField(position, label, value);
            }

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = newValue;
            }
        }

        public static Vector2Int CompactVector2IntField(Rect position, GUIContent label, Vector2Int value)
        {
            position = EditorGUI.PrefixLabel(position, label);

            float totalSpacing = LudiqStyles.compactHorizontalSpacing;
            float elementWidth = (position.width - totalSpacing) / 2f;

            var xPosition = new Rect(
                position.x,
                position.y,
                elementWidth,
                EditorGUIUtility.singleLineHeight
            );

            var yPosition = new Rect(
                xPosition.xMax + LudiqStyles.compactHorizontalSpacing,
                position.y,
                elementWidth,
                EditorGUIUtility.singleLineHeight
            );

            return new Vector2Int(
                LudiqGUI.DraggableIntField(xPosition, value.x),
                LudiqGUI.DraggableIntField(yPosition, value.y)
            );
        }
    }
}