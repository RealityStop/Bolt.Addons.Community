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
    /// Used for generating nodes that require a Method and the awake method to function.
    /// For example OnButtonClick.
    /// </summary>
    public abstract class AwakeMethodNodeGenerator : MethodNodeGenerator, IRequireAwakeCode
    {
        protected AwakeMethodNodeGenerator(Unit unit) : base(unit)
        {
        }
        
        public override MethodModifier MethodModifier => MethodModifier.None;
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override string Name => unit.GetType().DisplayName().Replace(" ", "") + count;
        public override Type ReturnType => unit is IEventUnit @event ? @event.coroutine ? typeof(IEnumerator) : typeof(void) : typeof(void);
        public abstract void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer);
        protected override sealed void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", WriteOptions.IndentedNewLineAfter);
                return;
            }

            GenerateCode(input, data, writer);
        }

        protected abstract void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer);
    }
}