using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(fileName = "New Editor Window", menuName = "Visual Scripting/Community/Editor/Editor Window")][Inspectable]
    public sealed class EditorWindowAsset : Macro<FlowGraph>
    {
        [Serialize][Inspectable]
        public CustomVariables variables = new CustomVariables();

        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}