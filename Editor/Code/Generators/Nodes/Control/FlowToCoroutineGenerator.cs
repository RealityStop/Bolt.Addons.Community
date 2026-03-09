using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(FlowToCoroutine))]
    public class FlowToCoroutineGenerator : MethodNodeGenerator
    {
        public FlowToCoroutineGenerator(Unit unit) : base(unit) { }

        private FlowToCoroutine Unit => unit as FlowToCoroutine;

        public override ControlOutput OutputPort => Unit.Converted;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "FlowToCoroutine" + count;

        public override Type ReturnType => typeof(IEnumerator);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override void GeneratedMethodCode(ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.Converted, data, writer);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic($"{typeof(CoroutineToFlow).DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour",
                $"Could not generate {typeof(CoroutineToFlow).DisplayName()}", WriteOptions.IndentedNewLineAfter);
                return;
            }

            writer.WriteIndented("StartCoroutine").Parentheses(i => i.Write(Name + "()")).WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.normalFlow, data, writer);
        }
    }
}