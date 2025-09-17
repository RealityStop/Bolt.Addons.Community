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
    [CodeGenerator(typeof(GameObject))]
    public sealed class GameObjectGenerator : BaseGraphGenerator<GameObject>
    {
        public ScriptMachine[] components = new ScriptMachine[0];
        public ScriptMachine current;
        protected override FlowGraph GetFlowGraph()
        {
            return current.nest?.graph;
        }

        protected override string GetGraphName()
        {
            return (current.nest.graph.title?.Length > 0 ? current.nest.graph.title : Data.name + "_ScriptMachine_" + components.ToList().IndexOf(current)).LegalMemberName();
        }

        protected override GraphPointer GetGraphPointer()
        {
            return current.GetReference();
        }

        protected override bool IsValid()
        {
            if (Data == null) return false;
            CodeBuilder.Indent(1);
            components = Data != null ? Data.GetComponents<ScriptMachine>() : null;
            if (components == null || components.Length == 0) return false;

            if (current == null)
            {
                current = components[0];
            }
            if (current == null || current.GetReference() == null) return false;

            return true;
        }
    }
}
