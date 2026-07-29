using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(Vector3Int))]
    public class Vector3IntInspector : VectorInspector
    {
        public Vector3IntInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var value = (Vector3Int)metadata.value;

            BeginBlock(metadata, position);
            Vector3Int newValue;

            if (position.width <= Styles.compactThreshold)
            {
                newValue = CompactVector3IntField(position, GUIContent.none, (Vector3Int)metadata.value);
            }
            else
            {
                newValue = EditorGUI.Vector3IntField(position, label, value);
            }

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = newValue;
            }
        }

        public static Vector3Int CompactVector3IntField(Rect position, GUIContent label, Vector3Int value)
        {
            position = EditorGUI.PrefixLabel(position, label);

            float totalSpacing = LudiqStyles.compactHorizontalSpacing * 2;
            float elementWidth = (position.width - totalSpacing) / 3f;

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

            var zPosition = new Rect(
                yPosition.xMax + LudiqStyles.compactHorizontalSpacing,
                position.y,
                elementWidth,
                EditorGUIUtility.singleLineHeight
            );

            return new Vector3Int(
                LudiqGUI.DraggableIntField(xPosition, value.x),
                LudiqGUI.DraggableIntField(yPosition, value.y),
                LudiqGUI.DraggableIntField(zPosition, value.z)
            );
        }
    }
}