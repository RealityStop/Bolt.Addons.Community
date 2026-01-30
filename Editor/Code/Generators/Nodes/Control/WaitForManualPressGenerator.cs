using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForManualPress))]
    public class WaitForManualPressGenerator : VariableNodeGenerator, IRequireMethods
    {
        private WaitForManualPress Unit => unit as WaitForManualPress;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "waitForPress" + count;

        public override Type Type => typeof(bool);

        public override object DefaultValue => false;

        public override bool HasDefaultValue => false;

        public WaitForManualPressGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.SetVariable(Name, writer.BoolString(true));

            writer.YieldReturn(writer.Action(() => writer.New(typeof(WaitWhile), "() => " + Name.VariableHighlight())), WriteOptions.IndentedNewLineAfter);

            GenerateExitControl(Unit.output, data, writer);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), data.AddMethodName("WaitForPress"));
            method.Body(w => w.Write($"{Name.VariableHighlight()} = {"false".ConstructHighlight()};").NewLine());
            var attribute = AttributeGenerator.Attribute<ContextMenu>();
            attribute.AddParameter($"Trigger_{Name}");
            method.AddAttribute(attribute);

            yield return method;
        }
    }
}