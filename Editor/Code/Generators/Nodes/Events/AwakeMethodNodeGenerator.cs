using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    /// <summary>
    /// Used for generating nodes that require the awake method to function.
    /// For example OnButtonClick.
    /// </summary>
    public abstract class AwakeMethodNodeGenerator : MethodNodeGenerator
    {
        protected AwakeMethodNodeGenerator(Unit unit) : base(unit)
        {
        }
        public override MethodModifier MethodModifier => MethodModifier.None;
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override Type ReturnType => typeof(void);
        public override string Name => unit.GetType().DisplayName() + count;
        public abstract string GenerateAwakeCode(ControlGenerationData data, int indent);
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if(!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeSelectableForThisUnit(CodeUtility.ToolTip($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", ""));
            foreach (var param in Parameters)
            {
                data.AddLocalNameInScope(param.name, param.type);
            }
            return GetNextUnit(OutputPort, data, indent);
        }
    } 
}