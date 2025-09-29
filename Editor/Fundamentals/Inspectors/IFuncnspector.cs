using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(IFunc))]
    public class FuncInspector : Inspector
    {
        public FuncInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (metadata.value != null)
                GUI.Label(position, "Func");
            else
                GUI.Label(position, "No Func");
        }
    }
}
