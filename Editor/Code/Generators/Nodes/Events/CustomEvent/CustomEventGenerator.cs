using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CustomEvent))]
    public class CustomEventGenerator : MethodNodeGenerator
    {
        public CustomEventGenerator(Unit unit) : base(unit)
        {
        }

        private CustomEvent Unit => unit as CustomEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.argumentPorts;

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "CustomEvent" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new() { new TypeParam(typeof(CustomEventArgs), "args") };

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.target && !input.hasValidConnection)
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
            return base.GenerateValue(input, data);
        }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.argumentPorts.Contains(output))
            {
                var callCode = CodeBuilder.CallCSharpUtilityExtensitionMethod(Unit, MakeClickableForThisUnit("args".VariableHighlight()), MakeClickableForThisUnit(nameof(CSharpUtility.GetArgument)), Unit.argumentPorts.IndexOf(output).As().Code(false, Unit), MakeClickableForThisUnit("typeof".ConstructHighlight() + "(" + (data.GetExpectedType() ?? typeof(object)).As().CSharpName(false, true) + ")"));
                var code = Unit.CreateClickableString().Ignore(callCode).Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType() != typeof(object));
                return code;
            }
            return base.GenerateValue(output, data);
        }
    }
}