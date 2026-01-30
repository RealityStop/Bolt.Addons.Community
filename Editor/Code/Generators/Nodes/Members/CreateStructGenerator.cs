using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CreateStruct))]
    public class CreateStructGenerator : LocalVariableGenerator
    {
        private CreateStruct Unit => unit as CreateStruct;
        public CreateStructGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!data.scopeGeneratorData.ContainsKey(Unit))
                writer.New(Unit.type);
            else
                writer.GetVariable(variableName);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = data.AddLocalNameInScope(Unit.type.As().CSharpName(false, false, false) + "_Variable", Unit.type);
            variableType = Unit.type;

            if (!data.scopeGeneratorData.ContainsKey(Unit))
                data.scopeGeneratorData.Add(Unit, variableName);

            writer.CreateVariable(Unit.type, variableName, writer.Action(() => writer.New(Unit.type)), WriteOptions.Indented, EndWriteOptions.LineEnd);
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}