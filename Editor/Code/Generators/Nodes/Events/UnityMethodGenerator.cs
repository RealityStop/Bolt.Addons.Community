using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public abstract class UnityMethodGenerator<TEventUnit, TArgs> : MethodNodeGenerator where TEventUnit : EventUnit<TArgs>
    {
        protected UnityMethodGenerator(Unit unit) : base(unit)
        {
        }
    
        protected TEventUnit Unit => unit as TEventUnit;
        public override ControlOutput OutputPort => Unit.trigger;
        public override MethodModifier MethodModifier => MethodModifier.None;
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override Type ReturnType => typeof(void);
        public override string Name => typeof(TEventUnit).DisplayName();
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if(!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeClickableForThisUnit(CodeUtility.ToolTip($"{typeof(TEventUnit).DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {typeof(TEventUnit).DisplayName()}", ""));
            return GetNextUnit(Unit.trigger, data, indent);
        }
    } 
}