using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SelectOnFlow))]
    public class SelectOnFlowGenerator : MethodNodeGenerator
    {
        public SelectOnFlowGenerator(SelectOnFlow unit) : base(unit)
        {
        }

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;
        private string _name;

        public override string Name
        {
            get => string.IsNullOrEmpty(_name) ? $"SelectOnFlow{count}" : _name;
        }
        private SelectOnFlow Unit => unit as SelectOnFlow;
        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(object), "value") };

        public override ControlOutput OutputPort => Unit.exit;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.selection };

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.graph.groups.Any(_group => _group.position.Contains(Unit.position)))
            {
                _name = Unit.graph.groups.First(_group => _group.position.Contains(Unit.position)).label;
            }
            else
            {
                _name = "";
            }
            writer.WriteIndented();
            writer.Write(Name + "(");
            GenerateValue(Unit.branches[input], data, writer);
            writer.Write(");");
            writer.NewLine();
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("value");
        }
        public override void GeneratedMethodCode(ControlGenerationData data, CodeWriter writer)
        {
            data.CreateSymbol(unit, typeof(object));
            GenerateChildControl(Unit.exit, data, writer);
        }
    }
}