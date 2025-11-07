using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnButtonInput))]
    public class OnButtonInputGenerator : UpdateMethodNodeGenerator
    {
        private OnButtonInput Unit => unit as OnButtonInput;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnButtonInput" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnButtonInputGenerator(Unit unit) : base(unit) { }

        public override string GenerateUpdateCode(ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if ".ControlHighlight() + "(") + CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("GetButtonAction"), GenerateValue(Unit.buttonName, data), GenerateValue(Unit.action, data)) + MakeClickableForThisUnit(")") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit((Unit.coroutine ? $"StartCoroutine({Name}())" : Name + "()") + ";") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
            return output;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}