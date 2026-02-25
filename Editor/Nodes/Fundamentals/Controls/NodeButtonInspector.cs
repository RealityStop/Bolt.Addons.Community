using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(NodeButton))]
    public class NodeButtonInspector : Inspector
    {
        public NodeButtonInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            return 16;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
#if UNITY_2021_1_OR_NEWER
            BeginBlock(metadata, position);
#else
            BeginBlock(metadata, position, GUIContent.none);
#endif

            var buttonPosition = new Rect(
                position.x,
                position.y,
                position.width + 8,
                16
                );

            if (GUI.Button(buttonPosition, "Trigger", new GUIStyle(UnityEditor.EditorStyles.miniButton)))
            {
                var attribute = metadata.GetAttribute<NodeButtonAttribute>(true);

                if (attribute != null)
                {
                    var method = attribute.action;

                    object typeObject = metadata.parent.value;
                    GraphReference reference = GraphWindow.activeReference;
                    typeObject.GetType().GetMethod(method).Invoke(typeObject, new object[1] { reference });

                }
            }

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
            }
        }
    }
}