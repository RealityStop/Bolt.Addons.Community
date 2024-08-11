using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetVariable))]
    public class GetVariableGenerator : LocalVariableGenerator<GetVariable>
    {
        public GetVariableGenerator(Unit unit) : base(unit)
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

            if (Unit.name.hasValidConnection || (Unit.@object != null && Unit.@object.hasValidConnection))
            {
                var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
                var kind = string.Empty;
                var type = string.Empty;
                switch (Unit.kind)
                {
                    case VariableKind.Flow:
                        return "/* Flow Variables are not supported */";
                    case VariableKind.Graph:
                        return "/* Graph Variables do not support connected names */";
                    case VariableKind.Object:
                        kind = $".Object({GenerateValue(Unit.@object, data)})";
                        break;
                    case VariableKind.Scene:
                        kind = $".Scene({"SceneManager".TypeHighlight()}.GetActiveScene())";
                        break;
                    case VariableKind.Application:
                        kind = "." + "Application".VariableHighlight();
                        break;
                    case VariableKind.Saved:
                        kind = "." + "Saved".VariableHighlight();
                        break;
                }

                return $"{variables}{kind}.Get({GenerateValue(Unit.name, data)})";
            }
            else
            {
                var name = Unit.defaultValues[Unit.name.key] as string;

                var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
                switch (Unit.kind)
                {
                    case VariableKind.Scene:
                        var type = VisualScripting.Variables.Scene(SceneManager.GetActiveScene()).IsDefined(name) && VisualScripting.Variables.Scene(SceneManager.GetActiveScene()).Get(name) != null ? "<" + Type.GetType(VisualScripting.Variables.Scene(SceneManager.GetActiveScene()).GetDeclaration(name).typeHandle.Identification).As().CSharpName(false, true) + ">" : string.Empty;
                        return $"{variables}.Scene({"SceneManager".TypeHighlight()}.GetActiveScene()).Get{type}({GenerateValue(Unit.name, data)})";
                    case VariableKind.Application:
                        var _type = VisualScripting.Variables.Application.IsDefined(name) && VisualScripting.Variables.Application.Get(name) != null ? "<" + Type.GetType(VisualScripting.Variables.Application.GetDeclaration(name).typeHandle.Identification).As().CSharpName(false, true) + ">" : string.Empty;
                        return $"{variables}.Application.Get{_type}({GenerateValue(Unit.name, data)})";
                    case VariableKind.Saved:
                        var __type = VisualScripting.Variables.Saved.IsDefined(name) && VisualScripting.Variables.Saved.Get(name) != null ? "<" + Type.GetType(VisualScripting.Variables.Saved.GetDeclaration(name).typeHandle.Identification).As().CSharpName(false, true) + ">" : string.Empty;
                        return $"{variables}.Saved.Get{__type}({GenerateValue(Unit.name, data)})";
                }
                return data.GetVariableName(name).VariableHighlight();

            }
        }


        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return "gameObject".VariableHighlight();
            }

            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input, data);
            }
            else if (input.hasDefaultValue)
            {
                return unit.defaultValues[input.key].As().Code(true, true, true, "", false);
            }
            else
            {
                return $"/* \"{input.key} Requires Input\" */";
            }
        }
    }

}