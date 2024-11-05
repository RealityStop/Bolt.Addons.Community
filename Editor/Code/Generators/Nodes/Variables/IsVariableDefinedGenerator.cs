using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(IsVariableDefined))]
    public class IsVariableDefinedGenerator : LocalVariableGenerator<IsVariableDefined>
    {
        public IsVariableDefinedGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (Unit.kind == VariableKind.Scene)
            {
                NameSpace = "UnityEngine.SceneManagement";
            }
            else
            {
                NameSpace = string.Empty;
            }

            var variables = MakeSelectableForThisUnit(typeof(VisualScripting.Variables).As().CSharpName(true, true));
            var kind = string.Empty;
            switch (Unit.kind)
            {
                case VariableKind.Flow:
                    return MakeSelectableForThisUnit("/* Flow Variables are not supported */");
                case VariableKind.Graph:
                    return MakeSelectableForThisUnit("/* Graph Variables do not support connected names */");
                case VariableKind.Object:
                    kind = MakeSelectableForThisUnit($".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeSelectableForThisUnit(")")}";
                    break;
                case VariableKind.Scene:
                    kind = MakeSelectableForThisUnit($".Scene({"SceneManager".TypeHighlight()}.GetActiveScene())");
                    break;
                case VariableKind.Application:
                    kind = MakeSelectableForThisUnit("." + "Application".VariableHighlight());
                    break;
                case VariableKind.Saved:
                    kind = MakeSelectableForThisUnit("." + "Saved".VariableHighlight());
                    break;
            }

            return $"{variables}{kind}{MakeSelectableForThisUnit(".IsDefined(")}{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(")")}";
        }


        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return MakeSelectableForThisUnit("gameObject".VariableHighlight());
            }
            return base.GenerateValue(input, data);
        }
    }
}