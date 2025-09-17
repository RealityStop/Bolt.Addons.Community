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
    public class IsVariableDefinedGenerator : LocalVariableGenerator
    {
        private IsVariableDefined Unit => unit as IsVariableDefined;
        public IsVariableDefinedGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (Unit.kind == VariableKind.Scene)
            {
                NameSpaces = "UnityEngine.SceneManagement";
            }
            else
            {
                NameSpaces = string.Empty;
            }

            var variables = MakeClickableForThisUnit(typeof(VisualScripting.Variables).As().CSharpName(true, true));
            var kind = string.Empty;
            switch (Unit.kind)
            {
                case VariableKind.Flow:
                    return MakeClickableForThisUnit("/* Flow Variables are not supported */".WarningHighlight());
                case VariableKind.Graph:
                    return MakeClickableForThisUnit("/* Graph Variables do not support connected names */".WarningHighlight());
                case VariableKind.Object:
                    kind = MakeClickableForThisUnit($".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeClickableForThisUnit(")")}";
                    break;
                case VariableKind.Scene:
                    kind = MakeClickableForThisUnit($".Scene({"SceneManager".TypeHighlight()}.GetActiveScene())");
                    break;
                case VariableKind.Application:
                    kind = MakeClickableForThisUnit("." + "Application".VariableHighlight());
                    break;
                case VariableKind.Saved:
                    kind = MakeClickableForThisUnit("." + "Saved".VariableHighlight());
                    break;
            }

            return $"{variables}{kind}{MakeClickableForThisUnit(".IsDefined(")}{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(")")}";
        }


        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
            }
            return base.GenerateValue(input, data);
        }
    }
}