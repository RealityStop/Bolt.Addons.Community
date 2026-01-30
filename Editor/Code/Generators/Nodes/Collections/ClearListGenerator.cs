using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ClearList))]
    public class ClearListGenerator : NodeGenerator<ClearList>
    {
        public ClearListGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.listInput, data, writer);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember(writer.Action(w =>
            {
                w.WriteIndented();
                GenerateValue(Unit.listInput, data, w);
            }), "Clear").WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}