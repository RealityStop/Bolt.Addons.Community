using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using UnityEngine.EventSystems;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class PointerEventUnitGenerator<TUnit, TInterface> : InterfaceNodeGenerator where TUnit : PointerEventUnit
    {
        protected TUnit Unit => (TUnit)unit;
        public override List<Type> InterfaceTypes => new List<Type>() { typeof(TInterface) };

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.data };

        public override AccessModifier AccessModifier => AccessModifier.Public;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => typeof(TUnit).DisplayName();

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>()
        {
            new TypeParam(typeof(PointerEventData), "eventData")
        };

        public PointerEventUnitGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("eventData");
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic($"{Name} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {Name}", WriteOptions.IndentedNewLineAfter);
                return;
            }

            GenerateChildControl(Unit.trigger, data, writer);
        }
    }
}