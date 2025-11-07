using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForManualPress))]
    [RequiresMethods]
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
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = MakeClickableForThisUnit(Name.VariableHighlight() + " = " + "true".ConstructHighlight() + ";") + "\n";
            output += MakeClickableForThisUnit(typeof(WaitWhile).Create("() => " + Name.VariableHighlight()).YieldReturn()) + "\n";
            output += GetNextUnit(Unit.output, data, indent);
            return  output;
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), data.AddMethodName("WaitForPress"));
            method.Body(MakeClickableForThisUnit($"{Name.VariableHighlight()} = {"false".ConstructHighlight()};") + "\n");
            var attribute = AttributeGenerator.Attribute<ContextMenu>();
            attribute.AddParameter($"Trigger_{Name}");
            method.AddAttribute(attribute);

            yield return method;
        }
    }
}