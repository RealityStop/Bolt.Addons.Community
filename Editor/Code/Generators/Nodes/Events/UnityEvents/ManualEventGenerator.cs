using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ManualEvent))]
    public class ManualEventGenerator : MethodNodeGenerator
    {
        ManualEvent Unit => unit as ManualEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "ManualEvent" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new();

        public override List<AttributeDeclaration> Attributes
        {
            get
            {
                var declaration = new AttributeDeclaration();
                declaration.SetType(typeof(ContextMenu));
                declaration.AddParameter("itemName", typeof(string), "Trigger_" + Name);
                return new() { declaration };
            }
        }

        public ManualEventGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}