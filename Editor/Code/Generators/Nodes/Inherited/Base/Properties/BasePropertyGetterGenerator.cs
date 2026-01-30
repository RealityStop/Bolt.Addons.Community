using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BasePropertyGetterUnit))]
    public class BasePropertyGetterGenerator : NodeGenerator<BasePropertyGetterUnit>
    {
        public BasePropertyGetterGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write("base".ConstructHighlight());
            writer.Write(".");
            writer.Write(Unit.member.name);
        }
    }
}
