using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Gate))]
    public class GateGenerator : VariableNodeGenerator
    {
        private Gate Unit => unit as Gate;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "gate" + count;

        public override Type Type => typeof(GateLogic);

        public override object DefaultValue => new GateLogic();

        public override bool HasDefaultValue => true;

        public GateGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.enter)
            {
                writer.WriteIndented();
                writer.Write("if".ControlHighlight());
                writer.Parentheses(inner =>
                {
                    writer.InvokeMember(Name.VariableHighlight(), "IsOpen", writer.Action(() => GenerateValue(Unit.initialState, data, writer)));
                });
                writer.NewLine();
                writer.WriteLine("{");
                using (writer.Indented())
                {
                    GenerateChildControl(Unit.exit, data, writer);
                }
                writer.WriteLine("}");
            }
            else if (input == Unit.open)
            {
                writer.WriteIndented();
                writer.InvokeMember(Name.VariableHighlight(), "Open");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.close)
            {
                writer.WriteIndented();
                writer.InvokeMember(Name.VariableHighlight(), "Close");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.toggle)
            {
                writer.WriteIndented();
                writer.InvokeMember(Name.VariableHighlight(), "Toggle");
                writer.Write(";");
                writer.NewLine();
            }
        }
    }
}