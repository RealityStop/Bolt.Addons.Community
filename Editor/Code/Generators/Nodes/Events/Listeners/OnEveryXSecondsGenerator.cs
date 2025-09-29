using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnEveryXSeconds))]
    [RequiresVariables]
    public class OnEveryXSecondsGenerator : UpdateMethodNodeGenerator, IRequireVariables
    {
        public OnEveryXSecondsGenerator(Unit unit) : base(unit) { }

        public OnEveryXSeconds Unit => unit as OnEveryXSeconds;

        private string name = "onEveryXSeconds";

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override string GenerateUpdateCode(ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if ".ControlHighlight() + "(" + name.VariableHighlight() + ".Update(") + GenerateValue(Unit.seconds, data) + MakeClickableForThisUnit(", ") + GenerateValue(Unit.unscaledTime, data) + MakeClickableForThisUnit(")") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit((Unit.coroutine ? $"StartCoroutine({Name}())" : Name + "()") + ";") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            return output;
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name.LegalMemberName(), typeof(OnEveryXSecondsLogic));
            var variable = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(OnEveryXSecondsLogic), name);
            variable.Default(new OnEveryXSecondsLogic());
            variable.SetNewlineLiteral(false);
            yield return variable;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}