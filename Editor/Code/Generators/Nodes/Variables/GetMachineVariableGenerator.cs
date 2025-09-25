using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetMachineVariableNode))]
    public class GetMachineVariableNodeGenerator : LocalVariableGenerator
    {
        private GetMachineVariableNode Unit => unit as GetMachineVariableNode;
        public GetMachineVariableNodeGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.name.hasValidConnection)
            {
                return MakeClickableForThisUnit(CodeUtility.ErrorTooltip("Name can not have a connected value!", "Error Generating GetMachineVariableNode", "null".ConstructHighlight()));
            }
            else
            {
                return GenerateDisconnectedVariableCode(data);
            }
        }

        private string GenerateDisconnectedVariableCode(ControlGenerationData data)
        {
            var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
            variableType = GetVariableType(name, data, true);

            data.CreateSymbol(Unit, variableType);
            var expectedType = data.GetExpectedType();
            var hasExpectedType = expectedType != null;
            var isExpectedType = (hasExpectedType && variableType != null && expectedType.IsAssignableFrom(variableType)) || (hasExpectedType && IsVariableDefined(data, name) && !string.IsNullOrEmpty(GetVariableDeclaration(data, name).typeHandle.Identification) && expectedType.IsAssignableFrom(Type.GetType(GetVariableDeclaration(data, name).typeHandle.Identification))) || (hasExpectedType && data.TryGetVariableType(data.GetVariableName(name), out Type targetType) && expectedType.IsAssignableFrom(targetType));
            data.SetCurrentExpectedTypeMet(isExpectedType, variableType);
            if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] == null)
            {
                if (!data.ContainsNameInAnyScope(name))
                    return MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"Variable '{name}' could not be found in this script ensure it's defined in the Graph Variables.", "Error Generating GetMachineVariableNode", ""));
                return MakeClickableForThisUnit(data.GetVariableName(name).VariableHighlight());
            }
            else
                return GenerateValue(Unit.target, data) + MakeClickableForThisUnit(".") + GenerateValue(Unit.name, data);
        }

        private Type GetVariableType(string name, ControlGenerationData data, bool checkDecleration)
        {
            if (checkDecleration)
            {
                var isDefined = IsVariableDefined(data, name);
                if (isDefined)
                {
                    var declaration = GetVariableDeclaration(data, name);
                    return declaration?.typeHandle.Identification != null ? Type.GetType(declaration.typeHandle.Identification) : null;
                }
            }
            var target = GetTarget(data);
            ControlGenerationData targetData = null;
            if (target != null && CodeGenerator.GetSingleDecorator(GetTarget(data).gameObject) is ICreateGenerationData generationData)
            {
                targetData = generationData.GetGenerationData();
            }
            return targetData?.TryGetVariableType(targetData?.GetVariableName(name), out Type targetType) ?? false ? targetType : data.GetExpectedType() ?? typeof(object);
        }

        private bool IsVariableDefined(ControlGenerationData data, string name)
        {
            var target = GetTarget(data);
            return target != null && VisualScripting.Variables.Graph(target.GetReference()).IsDefined(name);
        }

        private VariableDeclaration GetVariableDeclaration(ControlGenerationData data, string name)
        {
            var target = GetTarget(data);
            if (target != null)
            {
                return VisualScripting.Variables.Graph(target.GetReference()).GetDeclaration(name);
            }
            return null;
        }
        private ScriptMachine GetTarget(ControlGenerationData data)
        {
            if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] == null && data.TryGetGraphPointer(out var graphPointer))
            {
                var reference = graphPointer.AsReference();
                return reference.rootObject is ScriptMachine scriptMachine ? scriptMachine : null;
            }
            else if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] != null)
            {
                return Unit.defaultValues[Unit.target.key].ConvertTo<ScriptMachine>();
            }
            else
            {
                if (data.TryGetGraphPointer(out var _graphPointer))
                {
                    if (Unit.target.hasValidConnection && CanPredictConnection(Unit.target, data))
                    {
                        try
                        {
                            return Flow.Predict<ScriptMachine>(Unit.target.GetPesudoSource(), _graphPointer.AsReference());
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
            if (input == Unit.target && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return "";
            }

            if (input == Unit.target)
            {
                if (!input.hasValidConnection)
                {
                    var machine = Unit.defaultValues[Unit.target.key] as ScriptMachine;
                    var machineName = GetMachineName(machine);
                    data.TryGetGameObject(out var gameObject);
                    if (machine.gameObject.GetComponents<MonoBehaviour>().Any(m => m.GetType().Name == machineName))
                        return (gameObject != machine.gameObject ? machine.gameObject.As().Code(false, Unit) + MakeClickableForThisUnit(".") : "") + MakeClickableForThisUnit($"GetComponent<{machineName.TypeHighlight()}>()");
                    else
                        return MakeClickableForThisUnit(CodeUtility.InfoTooltip($"{machineName} could not be found on the target GameObject. Ensure that the ScriptMachine is compiled into a C# script and attached to the correct GameObject.",
                        (gameObject != machine.gameObject ? machine.gameObject.As().Code(false) + "." : "") + $"GetComponent<{machineName.TypeHighlight()}>()"));
                }

                var target = GetTarget(data);
                if (target != null)
                {
                    var machineName = GetMachineName(target);
                    data.TryGetGameObject(out var gameObject);
                    if (target.gameObject.GetComponents<MonoBehaviour>().Any(m => m.GetType().Name == machineName))
                        return (gameObject != target.gameObject ? target.gameObject.As().Code(false, Unit) + MakeClickableForThisUnit(".") : "") + MakeClickableForThisUnit($"GetComponent<{machineName.TypeHighlight()}>()");
                    else
                        return MakeClickableForThisUnit(CodeUtility.InfoTooltip($"{machineName} could not be found on the target GameObject. Ensure that the ScriptMachine is compiled into a C# script and attached to the correct GameObject.",
                        (gameObject != target.gameObject ? target.gameObject.As().Code(false) + "." : "") + $"GetComponent<{machineName.TypeHighlight()}>()"));
                }
                return MakeClickableForThisUnit($"/* Could not find target for variable {CodeUtility.CleanCode(GenerateValue(Unit.name, data)).RemoveHighlights().RemoveMarkdown()} */".WarningHighlight());
            }

            if (input == Unit.name && !input.hasValidConnection)
            {
                return MakeClickableForThisUnit(Unit.defaultValues[input.key].ToString().VariableHighlight());
            }
            return base.GenerateValue(input, data);
        }

        private string GetMachineName(ScriptMachine machine)
        {
            return !string.IsNullOrEmpty(machine.nest?.graph.title)
                ? machine.nest.graph.title.LegalMemberName()
                : machine.gameObject.name.Capitalize().First().Letter() + "_ScriptMachine_" + Array.IndexOf(machine.gameObject.GetComponents<ScriptMachine>(), machine);
        }
    }
}