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
        public GetVariableGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            SetNamespaceBasedOnVariableKind();

            if (Unit.name.hasValidConnection || (Unit.@object != null && Unit.@object.hasValidConnection))
            {
                return GenerateConnectedVariableCode(data);
            }
            else
            {
                return GenerateDisconnectedVariableCode(data);
            }
        }

        private void SetNamespaceBasedOnVariableKind()
        {
            NameSpace = Unit.kind == VariableKind.Scene ? "UnityEngine.SceneManagement" : string.Empty;
        }

        private string GenerateConnectedVariableCode(ControlGenerationData data)
        {
            var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
            var kindSuffix = GetKindSuffix(data);

            return $"{variables}{kindSuffix}.Get({GenerateValue(Unit.name, data)})";
        }

        private string GetKindSuffix(ControlGenerationData data)
        {
            return Unit.kind switch
            {
                VariableKind.Object => $".Object({GenerateValue(Unit.@object, data)})",
                VariableKind.Scene => $".Scene({"SceneManager".TypeHighlight()}.GetActiveScene())",
                VariableKind.Application => ".Application".VariableHighlight(),
                VariableKind.Saved => ".Saved".VariableHighlight(),
                _ => string.Empty,
            };
        }

        private string GenerateDisconnectedVariableCode(ControlGenerationData data)
        {
            var name = Unit.defaultValues[Unit.name.key] as string;
            var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
            var variableType = GetVariableTypeForDisconnected(name, data);

            if (Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                var typeString = variableType != null ? $"<{variableType.As().CSharpName(false, true)}>" : string.Empty;
                var code = $"{variables}{GetKindSuffix(data)}.Get{typeString}({GenerateValue(Unit.name, data)})";
                return new ValueCode(code, data.GetExpectedType(), data.GetExpectedType() != null && !IsVariableDefined(name) && !data.TryGetVariableType(data.GetVariableName(name), out Type targetType));
            }
            else
            {
                return data.GetVariableName(name).VariableHighlight();
            }
        }

        private Type GetVariableTypeForDisconnected(string name, ControlGenerationData data)
        {
            var isDefined = IsVariableDefined(name);
            if (isDefined)
            {
                var declaration = GetVariableDeclaration(name);
                return declaration.typeHandle != null ? Type.GetType(declaration.typeHandle.Identification) : null;
            }
            return data.TryGetVariableType(data.GetVariableName(name), out Type targetType) ? targetType : data.GetExpectedType() ?? typeof(object);
        }

        private bool IsVariableDefined(string name)
        {
            return Unit.kind switch
            {
                VariableKind.Scene => VisualScripting.Variables.Scene(SceneManager.GetActiveScene()).IsDefined(name),
                VariableKind.Application => VisualScripting.Variables.Application.IsDefined(name),
                VariableKind.Saved => VisualScripting.Variables.Saved.IsDefined(name),
                _ => false,
            };
        }

        private VariableDeclaration GetVariableDeclaration(string name)
        {
            return Unit.kind switch
            {
                VariableKind.Scene => VisualScripting.Variables.Scene(SceneManager.GetActiveScene()).GetDeclaration(name),
                VariableKind.Application => VisualScripting.Variables.Application.GetDeclaration(name),
                VariableKind.Saved => VisualScripting.Variables.Saved.GetDeclaration(name),
                _ => null,
            };
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection)
            {
                return "gameObject".VariableHighlight();
            }
            return base.GenerateValue(input, data);
        }
    }
}
