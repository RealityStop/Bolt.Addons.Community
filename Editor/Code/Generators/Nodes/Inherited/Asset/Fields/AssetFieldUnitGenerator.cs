using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AssetFieldUnit))]
    public class AssetFieldUnitGenerator : NodeGenerator<AssetFieldUnit>
    {
        public AssetFieldUnitGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.actionDirection == ActionDirection.Get)
            {
                writer.GetVariable(Unit.field.FieldName);
            }
            else
            {
                base.GenerateValueInternal(output, data, writer);
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented(Unit.field.FieldName.VariableHighlight());
            writer.Write(" = ");
            GenerateValue(Unit.value, data, writer);
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }

}