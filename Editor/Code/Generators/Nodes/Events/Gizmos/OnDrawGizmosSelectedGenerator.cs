using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnDrawGizmosSelected))]
    public class OnDrawGizmosSelectedGenerator : MethodNodeGenerator
    {
        public OnDrawGizmosSelectedGenerator(Unit unit) : base(unit) { }
        private OnDrawGizmosSelected Unit => unit as OnDrawGizmosSelected;

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnDrawGizmosSelected";

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeClickableForThisUnit(CodeUtility.ToolTip($"{Name} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {Name}", ""));
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}