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
    [NodeGenerator(typeof(GetDictionaryVariableItem))]
    public class GetDictionaryVariableItemGenerator : LocalVariableGenerator
    {
        private GetDictionaryVariableItem Unit => unit as GetDictionaryVariableItem;
        public GetDictionaryVariableItemGenerator(Unit unit) : base(unit)
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
            variableName = data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : "";
            variableType = GetVariableType(variableName, data, true);
            if (data.GetExpectedType().IsAssignableFrom(variableType))
                data.SetCurrentExpectedTypeMet(true, variableType);
            else
                data.SetCurrentExpectedTypeMet(false, variableType);
            var kind = GetKind(data);

            data.CreateSymbol(Unit, variableType ?? typeof(object));
            var builder = Unit.CreateClickableString();
            return builder.CallCSharpUtilityExtensitionMethod(kind, MakeClickableForThisUnit("GetDictionaryVariable"), GenerateValue(Unit.name, data)).Brackets(inner => inner.Ignore(GenerateValue(Unit.key, data)), false);
        }

        private string GetKind(ControlGenerationData data)
        {
            var variables = typeof(VisualScripting.Variables).As().CSharpName(true, true);
            return Unit.kind switch
            {
                VariableKind.Object => MakeClickableForThisUnit(variables + $".Object(") + $"{GenerateValue(Unit.@object, data)}{MakeClickableForThisUnit($")")}",
                VariableKind.Scene => MakeClickableForThisUnit(GetSceneKind(data, variables)),
                VariableKind.Application => MakeClickableForThisUnit(variables + "." + "Application".VariableHighlight()),
                VariableKind.Saved => MakeClickableForThisUnit(variables + "." + "Saved".VariableHighlight()),
                _ => string.Empty,
            };
        }

        private string GenerateDisconnectedVariableCode(ControlGenerationData data)
        {
            var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
            variableType = GetVariableType(name, data, true);
            if (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                var expectedType = data.GetExpectedType();
                var hasExpectedType = expectedType != null;
                Type targetType = null;
                var declaration = IsVariableDefined(data, name) ? GetVariableDeclaration(data, name) : null;

#if VISUAL_SCRIPTING_1_7
                var isExpectedType =
                    hasExpectedType &&
                    (
                        (variableType != null && expectedType.IsAssignableFrom(variableType)) ||
                        (declaration != null && !string.IsNullOrEmpty(declaration.typeHandle.Identification) &&
                            expectedType.IsAssignableFrom(Type.GetType(declaration.typeHandle.Identification))) ||
                        (data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                            expectedType.IsAssignableFrom(targetType))
    );
#else
                var isExpectedType =
                    hasExpectedType &&
                    (
                        (variableType != null && expectedType.IsAssignableFrom(variableType)) ||
                        (declaration != null && declaration.value != null &&
                            expectedType.IsAssignableFrom(declaration.value.GetType())) ||
                        (data.TryGetVariableType(data.GetVariableName(name), out targetType) &&
                            expectedType.IsAssignableFrom(targetType))
                    );
#endif
                data.SetCurrentExpectedTypeMet(isExpectedType, variableType);

                if (!IsVariableDefined(data, name)) return Unit.specifyFallback ? GenerateValue(Unit.fallback, data) : MakeClickableForThisUnit($"/* Could not find variable with name \"{variableName}\" */".WarningHighlight());

                var builder = Unit.CreateClickableString();
                return builder.CallCSharpUtilityExtensitionMethod(GetKind(data), MakeClickableForThisUnit("GetDictionaryVariable"), GenerateValue(Unit.name, data)).Brackets(inner => inner.Ignore(GenerateValue(Unit.key, data)), false);
            }
            else
            {
                variableName = data.GetVariableName(name);
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

                if (!data.ContainsNameInAnyScope(variableName)) return Unit.specifyFallback ? GenerateValue(Unit.fallback, data) : MakeClickableForThisUnit($"/* Could not find variable with name \"{variableName}\" */".WarningHighlight());

                var builder = Unit.CreateClickableString();
                return builder.Clickable(variableName.VariableHighlight()).Brackets(inner => inner.Ignore(GenerateValue(Unit.key, data)), false);
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
            Type targetType = null;
            return data.TryGetVariableType(data.GetVariableName(name), out targetType) ? targetType : data.GetExpectedType() ?? typeof(object);
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