using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnMove))]
    public class OnMoveGenerator : InterfaceNodeGenerator
    {
        private OnMove Unit => unit as OnMove;
        public override List<Type> InterfaceTypes => new List<Type>(1) { typeof(IMoveHandler) };

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.data };

        public override AccessModifier AccessModifier => AccessModifier.Public;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnMove";

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>()
        {
            new TypeParam(typeof(AxisEventData), "eventData")
        };

        public OnMoveGenerator(Unit unit) : base(unit) { }

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