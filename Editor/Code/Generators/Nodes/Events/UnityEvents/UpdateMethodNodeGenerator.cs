using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    /// <summary>
    /// Used for generating nodes that require a Method and the Update method to function.
    /// For example OnButtonClick.
    /// </summary>
    public abstract class UpdateMethodNodeGenerator : MethodNodeGenerator
    {
        protected UpdateMethodNodeGenerator(Unit unit) : base(unit)
        {
        }

        /// <summary>
        /// Not used for UpdateMethodNodeGenerator
        /// </summary>
        public sealed override string MethodBody => null;
        public override MethodModifier MethodModifier => MethodModifier.None;
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override string Name => unit.GetType().DisplayName().Replace(" ", "") + count;
        public abstract string GenerateUpdateCode(ControlGenerationData data, int indent);
        public override sealed string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", ""));
            foreach (var param in Parameters)
            {
                data.AddLocalNameInScope(param.name, param.type);
            }
            return GenerateCode(input, data, indent);
        }

        protected abstract string GenerateCode(ControlInput input, ControlGenerationData data, int indent);
    }
}