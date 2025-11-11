using UnityEditor;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(TimeSpan))]
    public class TimeSpanInspector : Inspector
    {
        public TimeSpanInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var value = (TimeSpan)metadata.value;
            BeginBlock(metadata, position);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            bool useCompactLabels = adaptiveWidth;

            var row1Rect = new Rect(position.x, position.y, position.width, lineHeight);
            int[] row1Values = { value.Days, value.Hours, value.Minutes };

            var row1Labels = new GUIContent[]
            {
                new GUIContent(useCompactLabels ? "D" : "Days"),
                new GUIContent(useCompactLabels ? "H" : "Hours"),
                new GUIContent(useCompactLabels ? "M" : "Minutes")
            };
            EditorGUI.MultiIntField(row1Rect, row1Labels, row1Values);

            var row2Rect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);
            int[] row2Values = { value.Seconds, value.Milliseconds };

            var row2Labels = new GUIContent[]
            {
                new GUIContent(useCompactLabels ? "S" : "Sec"),
                new GUIContent(useCompactLabels ? "Ms" : "Ms")
            };
            EditorGUI.MultiIntField(row2Rect, row2Labels, row2Values);

            row1Values[1] = Mathf.Clamp(row1Values[1], 0, 23);
            row1Values[2] = Mathf.Clamp(row1Values[2], 0, 59);
            row2Values[0] = Mathf.Clamp(row2Values[0], 0, 59);
            row2Values[1] = Mathf.Clamp(row2Values[1], 0, 999);

            TimeSpan newValue;
            try
            {
                newValue = new TimeSpan(
                    row1Values[0],
                    row1Values[1],
                    row1Values[2],
                    row2Values[0],
                    row2Values[1]
                );
            }
            catch
            {
                newValue = value;
            }

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = newValue;
            }
        }

        public override float GetAdaptiveWidth()
        {
            string[] labels = { "Days", "Hours", "Minutes", "Sec", "Ms" };
            float labelWidth = 0f;
            foreach (var l in labels)
                labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent(l)).x);

            float estimatedFields = 5 * 50f + 20f;
            return Mathf.Max(240f, labelWidth * 3 + estimatedFields);
        }
    }
}