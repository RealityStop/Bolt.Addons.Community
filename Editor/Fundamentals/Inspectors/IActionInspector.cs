using UnityEditor;
using UnityEngine;
namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(IAction))]
    public class IActionInspector : Inspector
    {
        public IActionInspector(Metadata metadata) : base(metadata)
        {
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (metadata.value != null)
                GUI.Label(position, "Action");
            else
                GUI.Label(position, "No Action");
        }
    }
}