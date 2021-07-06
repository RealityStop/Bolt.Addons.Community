using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Utility.Editor
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