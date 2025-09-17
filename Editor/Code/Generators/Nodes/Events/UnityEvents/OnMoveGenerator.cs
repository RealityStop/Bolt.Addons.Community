using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnMove))]
    public class OnMoveGenerator : InterfaceNodeGenerator
    {
        private OnMove Unit => unit as OnMove;
        public override List<Type> InterfaceTypes => new List<Type>() { typeof(IMoveHandler) };

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.data };

        public override AccessModifier AccessModifier => AccessModifier.Public;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnMove";

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>()
        {
            new TypeParam(typeof(AxisEventData), "eventData")
        };

        public OnMoveGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("eventData".VariableHighlight());
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeClickableForThisUnit(CodeUtility.ToolTip($"{Name} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {Name}", ""));
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}