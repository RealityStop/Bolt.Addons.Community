using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnUnityEvent))]
    public class OnUnityEventGenerator : AwakeMethodNodeGenerator
    {
        private OnUnityEvent Unit => unit as OnUnityEvent;

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.valueOutputs.ToList();

        public override List<TypeParam> Parameters
        {
            get
            {
                List<TypeParam> @params = new List<TypeParam>();
                foreach (var output in Unit.valueOutputs)
                {
                    @params.Add(new TypeParam(output.type, output.key));
                }
                return @params;
            }
        }

        public OnUnityEventGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(output.key.VariableHighlight());
        }

        public override string GenerateAwakeCode(ControlGenerationData data, int indent)
        {
            string output = CodeBuilder.Indent(indent) + GenerateValue(Unit.UnityEvent, data);
            string parameters = string.Join(", ", Unit.valueOutputs.Select(v => v.key.VariableHighlight()));
            output += MakeClickableForThisUnit(".AddListener((" + parameters + ") => " + (Unit.coroutine ? $"StartCoroutine({Name}({parameters}))" : Name + $"({parameters})") + ");") + "\n";
            return output;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}