using Unity.VisualScripting.Community.CSharp;
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
            {
                if (metadata["Unit"].value is Unit unit)
                {
                    if (GUI.Button(position, "Preview Func"))
                    {
                        var data = new ControlGenerationData(typeof(object), LudiqGraphsEditorUtility.editedContext.value.reference);

                        CodeWriter code = new CodeWriter();

                        if (unit is DelegateNode delegateNode)
                        {
                            delegateNode.GenerateValue(delegateNode.@delegate, code, data);
                        }
                        else
                        {
                            unit.GenerateControl(null, data, code);
                        }

                        CSharpPreviewWindow.OpenWithCode(code, false);
                    }
                }
                else
                {
                    GUI.Label(position, "Func");
                }
            }
            else
            {
                GUI.Label(position, "No Func");
            }
        }
    }
}
