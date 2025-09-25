using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(HDRColor))]
    [RenamedFrom("Unity.VisualScripting.HDRColorInspector")]
    public class HDRColorInspector : Inspector
    {
        public HDRColorInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            position = BeginLabeledBlock(metadata, position, label);

            HDRColor hdrColor = (HDRColor)metadata.value;
            var newValue = EditorGUI.ColorField(position, new(), hdrColor.color, true, true, true);

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();

                // Update the color value in the HDRColor struct
                hdrColor.color = newValue;
                metadata.value = hdrColor;
            }
        }
    }
}
