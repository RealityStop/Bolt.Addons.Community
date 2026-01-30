using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable(output.key);
        }

        public override void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            GenerateValue(Unit.UnityEvent, data, writer);
            string parameters = string.Join(", ", Unit.valueOutputs.Select(v => v.key.VariableHighlight()));
            writer.Write(".AddListener((" + parameters + ") => " + (Unit.coroutine ? $"StartCoroutine({Name}({parameters}))" : Name + $"({parameters})") + ");");
            writer.NewLine();
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }
    }
}