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
    /// Used for generating nodes that require a Method and the awake method to function.
    /// For example OnButtonClick.
    /// </summary>
    public abstract class AwakeMethodNodeGenerator : MethodNodeGenerator
    {
        protected AwakeMethodNodeGenerator(Unit unit) : base(unit)
        {
        }
        // Not used for AwakeMethodNodeGenerator
        public sealed override string MethodBody => "";

        public override MethodModifier MethodModifier => MethodModifier.None;
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override string Name => unit.GetType().DisplayName().Replace(" ", "") + count;
        public override Type ReturnType => unit is IEventUnit @event ? @event.coroutine ? typeof(IEnumerator) : typeof(void) : typeof(void);
        public abstract string GenerateAwakeCode(ControlGenerationData data, int indent);
        public override sealed string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeUtility.ToolTip($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", ""));
            foreach (var param in Parameters)
            {
                data.AddLocalNameInScope(param.name, param.type);
            }
            return GenerateCode(input, data, indent);
        }

        protected abstract string GenerateCode(ControlInput input, ControlGenerationData data, int indent);
    }
}