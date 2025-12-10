using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
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
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "ActiveScene".VariableHighlight();
        }

        private void SetNamespaceBasedOnVariableKind()
        {
            NameSpaces = Unit.kind == VariableKind.Scene ? "UnityEngine.SceneManagement" : string.Empty;
        }

        private string GenerateConnectedVariableCode(ControlGenerationData data)
        {
            variableType = GetVariableType(data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : "", data, true);
            if (data.GetExpectedType() != null && variableType != null && data.GetExpectedType().IsAssignableFrom(variableType))
                data.SetCurrentExpectedTypeMet(true, variableType);
            else
                data.SetCurrentExpectedTypeMet(false, variableType);
            var typeString = variableType != null ? $"<{variableType.As().CSharpName(false, true)}>" : string.Empty;
            var kind = GetKind(data, typeString);

            data.CreateSymbol(Unit, variableType ?? typeof(object));
            return kind + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(")")}";
        }

        private string GetKind(ControlGenerationData data, string typeString)
        {
            var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
            return Unit.kind switch
            {
                VariableKind.Object => MakeClickableForThisUnit(variables + $".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeClickableForThisUnit($").Get{typeString}(")}",
                VariableKind.Scene => MakeClickableForThisUnit(GetSceneKind(data, variables) + $".Get{typeString}("),
                VariableKind.Application => MakeClickableForThisUnit(variables + "." + "Application".VariableHighlight() + $".Get{typeString}("),
                VariableKind.Saved => MakeClickableForThisUnit(variables + "." + "Saved".VariableHighlight() + $".Get{typeString}("),
                _ => string.Empty,
            };
        }

        private string GenerateDisconnectedVariableCode(ControlGenerationData data)
        {
            var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
            variableType = GetVariableType(name, data, true);
            if (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                var typeString = variableType != null ? $"<{variableType.As().CSharpName(false, true)}>" : string.Empty;
                var expectedType = data.GetExpectedType();
                var hasExpectedType = expectedType != null;
                Type targetType = null;

#if VISUAL_SCRIPTING_1_7
                var isExpectedType =
                    (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType))
                    || (hasExpectedType && IsVariableDefined(data, name) &&
                        !string.IsNullOrEmpty(GetVariableDeclaration(data, name).typeHandle.Identification) &&
                        expectedType.IsAssignableFrom(Type.GetType(GetVariableDeclaration(data, name).typeHandle.Identification)))
                    || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                        expectedType.IsAssignableFrom(targetType));
#else
                var isExpectedType =
                    (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType))
                    || (hasExpectedType && IsVariableDefined(data, name) &&
                        GetVariableDeclaration(data, name).value != null &&
                        expectedType.IsAssignableFrom(GetVariableDeclaration(data, name).value.GetType()))
                    || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                        expectedType.IsAssignableFrom(targetType));
#endif
                data.SetCurrentExpectedTypeMet(isExpectedType, variableType);
                var code = GetKind(data, typeString) + $"{GenerateValue(Unit.name, data)}{MakeClickableForThisUnit(")")}";
                return code;
            }
            else
            {
                data.CreateSymbol(Unit, variableType);
                var expectedType = data.GetExpectedType();
                var hasExpectedType = expectedType != null;
                Type targetType = null;
                var declaration = IsVariableDefined(data, name) ? GetVariableDeclaration(data, name) : null;

#if VISUAL_SCRIPTING_1_7
                var isExpectedType =
                    (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType))
                    || (hasExpectedType && declaration != null &&
                        !string.IsNullOrEmpty(declaration.typeHandle.Identification) &&
                        expectedType.IsAssignableFrom(Type.GetType(declaration.typeHandle.Identification)))
                    || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                        expectedType.IsAssignableFrom(targetType));
#else
                var isExpectedType =
                    (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType))
                    || (hasExpectedType && declaration != null &&
                        declaration.value != null &&
                        expectedType.IsAssignableFrom(declaration.value.GetType()))
                    || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                        expectedType.IsAssignableFrom(targetType));
#endif
                data.SetCurrentExpectedTypeMet(isExpectedType, variableType);
                return MakeClickableForThisUnit(data.GetVariableName(name).VariableHighlight());
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
#if VISUAL_SCRIPTING_1_7
                    return declaration?.typeHandle.Identification != null
                        ? Type.GetType(declaration.typeHandle.Identification)
                        : null;
#else
                    return declaration?.value != null
                        ? declaration.value.GetType()
                        : null;
#endif

                }
            }
            var type = data.TryGetVariableType(data.GetVariableName(name), out Type targetType) ? targetType : data.GetExpectedType();

            if (type == null && data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet())
            {
                return data.GetExpectedType();
            }
            return type ?? typeof(object);
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
                VariableKind.Graph => unit.graph.variables.GetDeclaration(name),
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
                            return Flow.Predict<GameObject>(Unit.@object.GetPesudoSource(), graphPointer.AsReference());
                        }
                        catch (InvalidOperationException ex)
                        {
                            Debug.LogError(ex);
                            return null;
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
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
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