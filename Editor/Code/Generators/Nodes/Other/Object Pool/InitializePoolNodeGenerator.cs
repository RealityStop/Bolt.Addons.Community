using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(InitializePoolNode))]
    public class InitializePoolNodeGenerator : LocalVariableGenerator
    {
        private InitializePoolNode Unit => unit as InitializePoolNode;

        public InitializePoolNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();

            variableName = data.AddLocalNameInScope("pool", typeof(ObjectPool), true);

            writer.CreateVariable("var".ConstructHighlight(), variableName, writer.Action(() =>
            {
                writer.InvokeMember(typeof(ObjectPool), "CreatePool");
                GenerateValue(Unit.InitialPoolSize, data, writer);
                writer.ParameterSeparator();
                GenerateValue(Unit.Prefab, data, writer);

                if (Unit.CustomParent)
                {
                    writer.ParameterSeparator();
                    GenerateValue(Unit.parent, data, writer);
                }
            }), WriteOptions.Indented, EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.Initialized, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable(variableName);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.parent && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                writer.GetVariable("gameObject");
                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }
    }
}