using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(InheritedFieldUnit))]
    public class InheritedFieldUnitGenerator : NodeGenerator<InheritedFieldUnit>
    {
        public InheritedFieldUnitGenerator(Unit unit) : base(unit)
        {
        }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.actionDirection == ActionDirection.Get)
            {
                writer.Write("this".ConstructHighlight());
                writer.Write(".");
                writer.Write(Unit.member.name.VariableHighlight());
            }
            else
            {
                base.GenerateValueInternal(output, data, writer);
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("this".ConstructHighlight());
            writer.Write(".");
            writer.Write(Unit.member.name.VariableHighlight());
            writer.Write(" = ");
            GenerateValue(Unit.value, data, writer);
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}