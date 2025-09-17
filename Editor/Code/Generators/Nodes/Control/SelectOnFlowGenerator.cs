using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community
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

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(0, "value") };

        public override string MethodBody => GetNextUnit(OutputPort, Data, 0);

        public override int GenericCount => 1;

        public override ControlOutput OutputPort => Unit.exit;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() {Unit.selection};

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            Data = data;
            if (Unit.graph.groups.Any(_group => _group.position.Contains(Unit.position)))
            {
                _name = Unit.graph.groups.First(_group => _group.position.Contains(Unit.position)).label;
            }
            else
            {
                _name = "";
            }
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(Name + "(") + GenerateValue(Unit.branches[input], data) + MakeClickableForThisUnit(");") + "\n";
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("value".VariableHighlight());
        }
    }
}