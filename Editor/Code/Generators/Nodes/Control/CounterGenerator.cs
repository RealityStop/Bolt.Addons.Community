using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CounterNode))]
    public class CounterNodeGenerator : VariableNodeGenerator
    {
        public CounterNodeGenerator(CounterNode unit) : base(unit)
        {
        }
        
        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        private CounterNode Unit => unit as CounterNode;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "Counter" + count;

        public override Type Type => typeof(CounterLogic);

        public override object DefaultValue => new CounterLogic();

        public override bool HasDefaultValue => true;

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = Name;
            if (input == Unit.enter)
            {
                writer.WriteIndented();
                writer.InvokeMember(variableName.VariableHighlight(), "Update");
                writer.WriteEnd(EndWriteOptions.LineEnd);

                GenerateExitControl(Unit.exit, data, writer);
            }
            else if (input == Unit.reset)
            {
                writer.WriteIndented();
                writer.InvokeMember(variableName.VariableHighlight(), "Reset");
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetMember(variableName.VariableHighlight(), "Count");
        }
    }
}