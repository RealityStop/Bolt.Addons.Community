using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnMultiKeyPress))]
    [RequiresVariables]
    public class OnMultiKeyPressGenerator : UpdateMethodNodeGenerator, IRequireVariables
    {
        private OnMultiKeyPress Unit => unit as OnMultiKeyPress;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnMultiKeyPress" + count;

        private string name = "onMultiKeyPress";

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new();

        public OnMultiKeyPressGenerator(Unit unit) : base(unit) { }

        public override string GenerateUpdateCode(ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if ".ControlHighlight() + "(" + name.VariableHighlight() + "." + "Check(" + Unit.Delay.As().Code(false) + ", ") + GenerateValue(Unit.Key1, data) + MakeClickableForThisUnit(", ") + GenerateValue(Unit.Key2, data) + MakeClickableForThisUnit(", ") + GenerateValue(Unit.Action, data) + MakeClickableForThisUnit("))") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit((Unit.coroutine ? $"StartCoroutine({Name}())" : Name + "()") + ";") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
            return output;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name.LegalMemberName(), typeof(OnMultiKeyPressLogic));
            var variable = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(OnMultiKeyPressLogic), name);
            variable.Default(new OnMultiKeyPressLogic());
            variable.SetNewlineLiteral(false);
            yield return variable;
        }

    }
}