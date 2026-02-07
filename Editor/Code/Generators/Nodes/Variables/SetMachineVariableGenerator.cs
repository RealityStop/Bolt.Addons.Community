using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetMachineVariableNode))]
    public class SetMachineVariableNodeGenerator : LocalVariableGenerator
    {
        private SetMachineVariableNode Unit => unit as SetMachineVariableNode;
        public SetMachineVariableNodeGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.name.hasValidConnection)
            {
                using (writer.CodeDiagnosticScope("Name can not have a connected value!", CodeDiagnosticKind.Error))
                    writer.Error("Name can not have a connected value!");
                return;
            }

            var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
            variableType = GetVariableType(name, data, true);

            data.CreateSymbol(Unit, variableType);

            if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] == null)
            {
                if (!data.ContainsNameInAnyScope(name))
                {
                    using (writer.CodeDiagnosticScope($"Variable '{name}' could not be found in this script ensure it's defined in the Graph Variables.", CodeDiagnosticKind.Error))
                        writer.Error($"Error Generating GetMachineVariableNode.");
                    return;
                }

                writer.Write(data.GetVariableName(name).VariableHighlight());
                return;
            }

            GenerateValue(Unit.target, data, writer);
            writer.Write(".");
            GenerateValue(Unit.name, data, writer);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.name.hasValidConnection)
            {
                using (writer.CodeDiagnosticScope("Name can not have a connected value!", CodeDiagnosticKind.Error))
                    writer.Error("Name can not have a connected value!");
                GenerateExitControl(Unit.exit, data, writer);
                return;
            }

            var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();

            if (string.IsNullOrEmpty(name))
            {
                writer.Error("Variable name is empty");
                return;
            }

            variableType = GetVariableType(name, data, true);
            data.CreateSymbol(Unit, variableType);

            if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] == null)
            {
                if (!data.ContainsNameInAnyScope(name))
                {
                    using (writer.CodeDiagnosticScope($"Variable '{name}' could not be found in this script ensure it's defined in the Graph Variables.", CodeDiagnosticKind.Error))
                        writer.Error($"Error Generating GetMachineVariableNode.");
                    GenerateExitControl(Unit.exit, data, writer);
                    return;
                }

                writer.WriteIndented();
                writer.Write(data.GetVariableName(name).VariableHighlight());
                writer.Write(" = ");
                using (data.Expect(variableType))
                    GenerateValue(Unit.value, data, writer);
                writer.Write(";");
                writer.NewLine();
                GenerateExitControl(Unit.exit, data, writer);
                return;
            }

            writer.WriteIndented();
            GenerateValue(Unit.target, data, writer);
            writer.Write(".");
            GenerateValue(Unit.name, data, writer);
            writer.Write(" = ");
            using (data.Expect(variableType))
                GenerateValue(Unit.value, data, writer);
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
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
            var target = GetTarget(data);
            ControlGenerationData targetData = data;
            if (target != null && CodeGenerator.GetSingleDecorator(GetTarget(data).gameObject) is ICreateGenerationData generationData)
            {
                targetData = generationData.GetGenerationData();
            }
            Type targetType = null;
            return targetData?.TryGetVariableType(targetData?.GetVariableName(name), out targetType) ?? false ? targetType : data.GetExpectedType() ?? typeof(object);
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
        private SMachine GetTarget(ControlGenerationData data)
        {
            if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] == null && data.TryGetGraphPointer(out var graphPointer))
            {
                var reference = graphPointer.AsReference();
                return reference.rootObject is SMachine scriptMachine ? scriptMachine : null;
            }
            else if (!Unit.target.hasValidConnection && Unit.defaultValues[Unit.target.key] != null)
            {
                return Unit.defaultValues[Unit.target.key].ConvertTo<SMachine>();
            }
            else
            {
                if (data.TryGetGraphPointer(out var _graphPointer))
                {
                    if (Unit.target.hasValidConnection && CanPredictConnection(Unit.target, data))
                    {
                        try
                        {
                            return Flow.Predict<SMachine>(Unit.target.GetPesudoSource(), _graphPointer.AsReference());
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

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.target && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
                return;

            if (input == Unit.target)
            {
                if (!input.hasValidConnection)
                {
                    var machine = Unit.defaultValues[Unit.target.key] as SMachine;
                    var machineName = GetMachineName(machine);
                    data.TryGetGameObject(out var gameObject);
                    if (machine.gameObject.GetComponents<MonoBehaviour>().Any(m => m.GetType().Name == machineName))
                    {
                        if (gameObject != machine.gameObject)
                        {
                            writer.Object(machine.gameObject).Dot();
                        }
                        writer.Write($"GetComponent<{machineName.TypeHighlight()}>()");
                        return;
                    }
                    else
                    {
                        using (writer.CodeDiagnosticScope($"{machineName} could not be found on the target GameObject. Ensure that the ScriptMachine is compiled into a C# script and attached to the correct GameObject.", CodeDiagnosticKind.Info))
                        {
                            writer.Comment("Note: (Hover for more info)");
                            writer.Write($"GetComponent<{machineName.TypeHighlight()}>()");
                        }
                    }
                }

                var target = GetTarget(data);
                if (target != null)
                {
                    var machineName = GetMachineName(target);
                    data.TryGetGameObject(out var gameObject);
                    if (target.GetComponents<MonoBehaviour>().Any(m => m.GetType().Name == machineName))
                    {
                        if (gameObject != target)
                        {
                            writer.Object(target).Dot();
                        }
                        writer.Write($"GetComponent<{machineName.TypeHighlight()}>()");
                        return;
                    }
                    else
                    {
                        using (writer.CodeDiagnosticScope($"{machineName} could not be found on the target GameObject. Ensure that the ScriptMachine is compiled into a C# script and attached to the correct GameObject.", CodeDiagnosticKind.Info))
                        {
                            writer.Comment("Note: (Hover for more info)");
                            writer.Write($"GetComponent<{machineName.TypeHighlight()}>()");
                        }
                    }
                }
                writer.Error("Could not find target for variable");
                return;
            }

            if (input == Unit.name && !input.hasValidConnection)
            {
                writer.GetVariable(Unit.defaultValues[input.key].ToString());
                return;
            }
            base.GenerateValueInternal(input, data, writer);
        }

        private string GetMachineName(SMachine machine)
        {
            return !string.IsNullOrEmpty(machine.nest?.graph.title)
                ? machine.nest.graph.title.LegalMemberName()
                : machine.gameObject.name.Capitalize().First().Letter() + $"_{typeof(SMachine).Name}_" + Array.IndexOf(machine.gameObject.GetComponents<SMachine>(), machine);
        }
    }
}