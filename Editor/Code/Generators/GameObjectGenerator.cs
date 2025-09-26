using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using UnityEngine;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(GameObject))]
    public sealed class GameObjectGenerator : BaseGraphGenerator<GameObject>
    {
        public SMachine[] components = new SMachine[0];
        public SMachine current;

        private readonly Dictionary<object, ControlGenerationData> datas = new Dictionary<object, ControlGenerationData>();

        public override ControlGenerationData GetGenerationData()
        {
            if (datas.TryGetValue(current, out var data) && !data.isDisposed)
            {
                return data;
            }
            
            datas[current] = CreateGenerationData();
            return datas[current];
        }

        protected override FlowGraph GetFlowGraph()
        {
            return current.nest?.graph;
        }

        protected override string GetGraphName()
        {
            return (current.nest.graph.title?.Length > 0 ? current.nest.graph.title : Data.name + $"_{typeof(SMachine).Name}_" + components.ToList().IndexOf(current)).LegalMemberName();
        }

        protected override GraphPointer GetGraphPointer()
        {
            return current.GetReference();
        }

        protected override bool IsValid()
        {
            if (Data == null) return false;
            CodeBuilder.Indent(1);
            components = Data != null ? Data.GetComponents<SMachine>() : null;
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
