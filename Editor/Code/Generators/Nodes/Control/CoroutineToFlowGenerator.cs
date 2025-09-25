using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CoroutineToFlow))]
    public class CoroutineToFlowGenerator : NodeGenerator<CoroutineToFlow>
    {
        public CoroutineToFlowGenerator(Unit unit) : base(unit) { }
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            // This node will work like a sequence node because I don't think there is a way to
            // Stop the coroutine for a specific method and run the code there.
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"{typeof(CoroutineToFlow).DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {typeof(CoroutineToFlow).DisplayName()}", ""));
            var builder = Unit.CreateClickableString();
            builder.Ignore(GetNextUnit(Unit.Converted, data, indent));
            builder.Ignore(GetNextUnit(Unit.Corutine, data, indent));
            return builder;
        }
    }
}