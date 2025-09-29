using Unity.VisualScripting.Community.CSharp;
using Unity.VisualScripting.Community.Libraries.CSharp;
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
            {
                if (metadata["Unit"].value is Unit unit)
                {
                    if (GUI.Button(position, "Preview Action"))
                    {
                        var data = new ControlGenerationData(typeof(object), LudiqGraphsEditorUtility.editedContext.value.reference);

                        string code;

                        if (unit is DelegateNode delegateNode)
                        {
                            code = delegateNode.GenerateValue(delegateNode.@delegate, data).RemoveMarkdown();
                        }
                        else
                        {
                            code = unit.GenerateControl(null, data, 0).RemoveMarkdown();
                        }

                        CSharpPreviewWindow.OpenWithCode(code, false);
                    }
                }
                else
                {
                    GUI.Label(position, "Action");
                }
            }
            else
            {
                GUI.Label(position, "No Action");
            }
        }
    }
}