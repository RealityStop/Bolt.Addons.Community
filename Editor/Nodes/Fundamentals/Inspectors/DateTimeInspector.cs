using UnityEditor;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(DateTime))]
    public class DateTimeInspector : Inspector
    {
        public DateTimeInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var value = (DateTime)metadata.value;

            BeginBlock(metadata, position);

            float lineHeight = EditorGUIUtility.singleLineHeight;

            var dateRect = new Rect(position.x, position.y, position.width, lineHeight);
            int[] dateValues = new int[] { value.Year, value.Month, value.Day };
            GUIContent[] dateLabels = adaptiveWidth ? new GUIContent[] { new GUIContent("Y"), new GUIContent("M"), new GUIContent("D") } : new GUIContent[] { new GUIContent("Year"), new GUIContent("Month"), new GUIContent("Day") };
            EditorGUI.MultiIntField(dateRect, dateLabels, dateValues);

            var timeRect = new Rect(position.x, position.y + lineHeight + 2, position.width, lineHeight);
            int[] timeValues = new int[] { value.Hour, value.Minute, value.Second };
            GUIContent[] timeLabels = adaptiveWidth ? new GUIContent[] { new GUIContent("H"), new GUIContent("M"), new GUIContent("S") } : new GUIContent[] { new GUIContent("Hour"), new GUIContent("Min"), new GUIContent("Sec") };
            EditorGUI.MultiIntField(timeRect, timeLabels, timeValues);

            dateValues[1] = Mathf.Clamp(dateValues[1], 1, 12);
            dateValues[2] = Mathf.Clamp(dateValues[2], 1, DateTime.DaysInMonth(dateValues[0], dateValues[1]));
            timeValues[0] = Mathf.Clamp(timeValues[0], 0, 23);
            timeValues[1] = Mathf.Clamp(timeValues[1], 0, 59);
            timeValues[2] = Mathf.Clamp(timeValues[2], 0, 59);

            var newValue = new DateTime(dateValues[0], dateValues[1], dateValues[2], timeValues[0], timeValues[1], timeValues[2]);

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = newValue;
            }
        }

        public override float GetAdaptiveWidth()
        {
            return 240f;
        }
    }
}