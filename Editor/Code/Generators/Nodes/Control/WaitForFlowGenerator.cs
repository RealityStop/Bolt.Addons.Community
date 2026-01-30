using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForFlow))]
    public class WaitForFlowGenerator : VariableNodeGenerator
    {
        private WaitForFlow Unit => unit as WaitForFlow;
        public override AccessModifier AccessModifier => AccessModifier.None;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "waitForFlow" + count;

        public override Type Type => typeof(WaitForFlowLogic);

        public override object DefaultValue => new WaitForFlowLogic(Unit.inputCount, Unit.resetOnExit);

        public override bool HasDefaultValue => true;

        public WaitForFlowGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.reset)
            {
                writer.WriteIndented("if ".ControlHighlight()).Parentheses(inner =>
                inner.InvokeMember(Name.VariableHighlight(), "Enter", Unit.controlInputs.ToList().IndexOf(input).As().Code(false))).NewLine();
                writer.WriteLine("{");
                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.exit, data, writer);
                }
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteIndented();
                writer.InvokeMember(Name.VariableHighlight(), "Reset");

                writer.WriteEnd(EndWriteOptions.LineEnd);
            }
        }
    }
}