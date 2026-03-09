using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BasePropertySetterUnit))]
    public class BasePropertySetterGenerator : NodeGenerator<BasePropertySetterUnit>
    {
        public BasePropertySetterGenerator(Unit unit) : base(unit)
        {
        }


        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("base".ConstructHighlight());
            writer.Write(".");
            writer.Write(Unit.member.name);
            writer.Write(" = ");
            GenerateValue(Unit.value, data, writer);
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}