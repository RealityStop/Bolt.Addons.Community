using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetVariable))]
    public class GetVariableGenerator : LocalVariableGenerator
    {
        private GetVariable Unit => unit as GetVariable;
        public GetVariableGenerator(Unit unit) : base(unit)
        {
            SetNamespaceBasedOnVariableKind();
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            SetNamespaceBasedOnVariableKind();

            if (Unit.name.hasValidConnection || Unit.kind == VariableKind.Object)
            {
                if (Unit.kind == VariableKind.Object && !Unit.name.hasValidConnection)
                {
                    return GenerateDisconnectedVariableCode(data);
                }
                return GenerateConnectedVariableCode(data);
            }
            else
            {
                return GenerateDisconnectedVariableCode(data);
            }
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "Application".VariableHighlight();
        }

        private void SetNamespaceBasedOnVariableKind()
        {
            NameSpaces = Unit.kind == VariableKind.Scene ? "UnityEngine.SceneManagement" : string.Empty;
        }

        private string GenerateConnectedVariableCode(ControlGenerationData data)
        {
            variableType = GetVariableType(data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : "", data, false);
            if (data.GetExpectedType() == variableType)
                data.SetCurrentExpectedTypeMet(true, variableType);
            else
                data.SetCurrentExpectedTypeMet(false, variableType);
            var typeString = variableType != null ? $"<{variableType.As().CSharpName(false, true)}>" : string.Empty;
            var kind = GetKind(data, typeString);

            data.CreateSymbol(Unit, variableType ?? typeof(object), $"{kind}.Get{typeString}({GenerateValue(Unit.name, data)})");
            return kind + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(")")}";
        }

        private string GetKind(ControlGenerationData data, string typeString)
        {
            var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
            return Unit.kind switch
            {
<<<<<<< Updated upstream
                VariableKind.Object => MakeSelectableForThisUnit(variables + $".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeSelectableForThisUnit($").Get{typeString}(")}",
                VariableKind.Scene => MakeSelectableForThisUnit(variables + $"." + "ActiveScene".VariableHighlight() + $".Get{typeString}("),
                VariableKind.Application => MakeSelectableForThisUnit(variables + "." + "Application".VariableHighlight() + $".Get{typeString}("),
                VariableKind.Saved => MakeSelectableForThisUnit(variables + "." + "Saved".VariableHighlight()) + $".Get{typeString}(",
=======
                VariableKind.Object => MakeClickableForThisUnit(variables + $".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeClickableForThisUnit($").Get{typeString}(")}",
                VariableKind.Scene => MakeClickableForThisUnit(GetSceneKind(data, variables) + $".Get{typeString}("),
                VariableKind.Application => MakeClickableForThisUnit(variables + "." + "Application".VariableHighlight() + $".Get{typeString}("),
                VariableKind.Saved => MakeClickableForThisUnit(variables + "." + "Saved".VariableHighlight() + $".Get{typeString}("),
>>>>>>> Stashed changes
                _ => string.Empty,
            };
        }

        private string GenerateDisconnectedVariableCode(ControlGenerationData data)
        {
            var name = Unit.defaultValues[Unit.name.key] as string;
            variableType = GetVariableType(name, data, true);
            if (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                var typeString = variableType != null ? $"<{variableType.As().CSharpName(false, true)}>" : string.Empty;
                var isExpectedType = data.GetExpectedType() == null || (variableType != null && data.GetExpectedType().IsAssignableFrom(variableType)) || (IsVariableDefined(data, name) && !string.IsNullOrEmpty(GetVariableDeclaration(data, name).typeHandle.Identification) && Type.GetType(GetVariableDeclaration(data, name).typeHandle.Identification) == data.GetExpectedType()) || (data.TryGetVariableType(data.GetVariableName(name), out Type targetType) && targetType == data.GetExpectedType());
                data.SetCurrentExpectedTypeMet(isExpectedType, variableType);
                var code = GetKind(data, typeString) + $"{GenerateValue(Unit.name, data)}{MakeSelectableForThisUnit(")")}";
                return new ValueCode(code, variableType, !isExpectedType);
            }
            else
            {
                data.CreateSymbol(Unit, variableType, name);
                return MakeSelectableForThisUnit(data.GetVariableName(name).VariableHighlight());
            }
        }

        private Type GetVariableType(string name, ControlGenerationData data, bool checkDecleration)
        {
            if (checkDecleration)
            {
                var isDefined = IsVariableDefined(data, name);
                if (isDefined)
                {
                    var declaration = GetVariableDeclaration(data, name);
                    return declaration.typeHandle.Identification != null ? Type.GetType(declaration.typeHandle.Identification) : null;
                }
            }
            return data.TryGetVariableType(data.GetVariableName(name), out Type targetType) ? targetType : data.GetExpectedType() ?? typeof(object);
        }

        private bool IsVariableDefined(ControlGenerationData data, string name)
        {
            var target = Unit.kind == VariableKind.Object ? GetTarget(data) : null;
            return Unit.kind switch
            {
                VariableKind.Object => target != null && VisualScripting.Variables.Object(target).IsDefined(name),
                VariableKind.Scene => VisualScripting.Variables.ActiveScene.IsDefined(name),
                VariableKind.Application => VisualScripting.Variables.Application.IsDefined(name),
                VariableKind.Saved => VisualScripting.Variables.Saved.IsDefined(name),
                _ => false,
            };
        }

        private VariableDeclaration GetVariableDeclaration(ControlGenerationData data, string name)
        {
            var target = Unit.kind == VariableKind.Object ? GetTarget(data) : null;
            return Unit.kind switch
            {
                VariableKind.Object => target != null ? VisualScripting.Variables.Object(target).GetDeclaration(name) : null,
                VariableKind.Scene => VisualScripting.Variables.ActiveScene.GetDeclaration(name),
                VariableKind.Application => VisualScripting.Variables.Application.GetDeclaration(name),
                VariableKind.Saved => VisualScripting.Variables.Saved.GetDeclaration(name),
                _ => null,
            };
        }
        private GameObject GetTarget(ControlGenerationData data)
        {
            if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] == null && data.TryGetGameObject(out var gameObject))
            {
                return gameObject;
            }
            else if (!Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] != null)
            {
                return Unit.defaultValues[Unit.@object.key].ConvertTo<GameObject>();
            }
            else
            {
                if (data.TryGetGraphPointer(out var graphPointer))
                {
                    if (Unit.@object.hasValidConnection && CanPredictConnection(Unit.@object, data))
                    {
                        try
                        {
                            return Flow.Predict(Unit.@object.GetPesudoSource(), graphPointer.AsReference()) as GameObject;
                        }
                        catch (InvalidOperationException ex)
                        {
                            Debug.LogError(ex);
                            return null; // Don't break code view so just log the error and return null.
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }
        }
        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@object && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeSelectableForThisUnit("gameObject".VariableHighlight());
            }

            if (input == Unit.@object)
            {
                data.SetExpectedType(typeof(GameObject));
                var code = base.GenerateValue(input, data);
                data.RemoveExpectedType();
                return code;
            }
            return base.GenerateValue(input, data);
        }
    }
}