using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Community.Utility;
using Mono.Cecil;


#if PACKAGE_INPUT_SYSTEM_EXISTS
using Unity.VisualScripting.InputSystem;
using UnityEngine.InputSystem;
#endif

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(ScriptGraphAsset))]
    public sealed class ScriptGraphAssetGenerator : BaseGraphGenerator<ScriptGraphAsset>
    {
        protected override FlowGraph GetFlowGraph()
        {
            return Data.graph;
        }

        protected override string GetGraphName()
        {
            return Data.graph.title?.Length > 0 ? Data.graph.title : Data.name.LegalMemberName();
        }

        protected override GraphPointer GetGraphPointer()
        {
            return Data.GetReference();
        }
    }
}
