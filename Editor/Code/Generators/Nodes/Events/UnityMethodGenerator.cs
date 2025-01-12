using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

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
    public override string Name => Unity.VisualScripting.Community.NameUtility.DisplayName(typeof(TEventUnit));
    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        if(!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeSelectableForThisUnit(CodeUtility.ToolTip($"{Unity.VisualScripting.Community.NameUtility.DisplayName(typeof(TEventUnit))} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {Unity.VisualScripting.Community.NameUtility.DisplayName(typeof(TEventUnit))}", ""));
        return GetNextUnit(Unit.trigger, data, indent);
    }
}