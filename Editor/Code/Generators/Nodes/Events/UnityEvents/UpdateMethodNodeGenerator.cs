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
    /// For example OnKeyboardInput.
    /// </summary>
    public abstract class UpdateMethodNodeGenerator : MethodNodeGenerator
    {
        protected UpdateMethodNodeGenerator(Unit unit) : base(unit)
        {
        }

        public override MethodModifier MethodModifier => MethodModifier.None;
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override string Name => unit.GetType().DisplayName().Replace(" ", "") + count;
        public abstract void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer);

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", WriteOptions.IndentedNewLineAfter);
                return;
            }

            using (writer.NewScope(data))
            {
                foreach (var param in Parameters)
                {
                    data.AddLocalNameInScope(param.name, param.type);
                }
                GenerateCode(input, data, writer);
            }
        }

        protected abstract void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer);
    }
}