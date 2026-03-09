using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ReturnAllObjectsToPoolNode))]
    public class ReturnAllObjectsToPoolNodeGenerator : NodeGenerator<ReturnAllObjectsToPoolNode>
    {
        public ReturnAllObjectsToPoolNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            writer.Write(typeof(ObjectPool).As().CSharpName(false, true));
            writer.Write(".");
            writer.Write("ReturnAllObjects");
            writer.Parentheses(w =>
            {
                GenerateValue(Unit.Pool, data, w);
            });
            writer.Write(";");
            writer.NewLine();

            GenerateExitControl(Unit.Exit, data, writer);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Pool && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                writer.Write("gameObject".VariableHighlight()).GetComponent(typeof(ObjectPool));
                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }
    }
}