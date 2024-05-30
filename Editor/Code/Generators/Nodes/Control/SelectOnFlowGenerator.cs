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
    public class SelectOnFlowGenerator : MethodNodeGenerator<SelectOnFlow>
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

        public override Type Type => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(object), "value") };

        public override string MethodBody => GetNextUnit(Unit.exit, Data, 0);

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
            output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, Name + (Unit.branches[input].hasValidConnection ? "(" + CodeUtility.MakeSelectable(Unit.branches[input].connection.source.unit as Unit, GenerateValue(Unit.branches[input])) + ")" : "(" + GenerateValue(Unit.branches[input])) + ");") + "\n";
            return output;
        }

        public override string GenerateValue(ValueOutput output)
        {
            return CodeUtility.MakeSelectable(Unit, "value".VariableHighlight());
        }
    }
}