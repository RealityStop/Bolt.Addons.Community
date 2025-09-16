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
    /// Used for generating nodes that require a variable and the awake method to function.
    /// </summary>
    public abstract class AwakeVariableNodeGenerator : VariableNodeGenerator
    {
        protected AwakeVariableNodeGenerator(Unit unit) : base(unit)
        {
        }
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override string Name => unit.GetType().DisplayName().Replace(" ", "") + count;
        public abstract string GenerateAwakeCode(ControlGenerationData data, int indent);
        public sealed override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeUtility.ToolTip($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", ""));
            return GenerateCode(input, data, indent);
        }

        protected abstract string GenerateCode(ControlInput input, ControlGenerationData data, int indent);
    } 
}