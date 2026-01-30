using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class EventListenerMethodGenerator<TUnit> : AwakeMethodNodeGenerator where TUnit : Unit
    {
        protected EventListenerMethodGenerator(Unit unit) : base(unit) { }
        protected TUnit Unit => unit as TUnit;
        public override Type ReturnType => IsCoroutine() ? typeof(IEnumerator) : typeof(void);
        public override List<ValueOutput> OutputValues => new List<ValueOutput>();
        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override AccessModifier AccessModifier => AccessModifier.None;

        protected abstract bool IsCoroutine();
        protected abstract string GetListenerSetupCode();

        public override void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) 
            {
                writer.WriteErrorDiagnostic($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}");
                return;
            }
            
            writer.WriteIndented();
            if (GetTargetValueInput() != null)
                GenerateValue(GetTargetValueInput(), data, writer);
            writer.Write(GetListenerSetupCode());
            writer.NewLine();
        }

        protected abstract ValueInput GetTargetValueInput();

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(OutputPort, data, writer);
        }
    }
}