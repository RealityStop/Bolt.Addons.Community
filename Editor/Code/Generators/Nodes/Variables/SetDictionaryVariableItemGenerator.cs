using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetDictionaryVariableItem))]
    public class SetDictionaryVariableItemGenerator : LocalVariableGenerator
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private SetDictionaryVariableItem Unit => unit as SetDictionaryVariableItem;
        public SetDictionaryVariableItemGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine.SceneManagement";
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.name.hasValidConnection)
                GenerateConnectedNameCodeInternal(data, writer);
            else
                GenerateDirectNameCodeInternal(data, writer);
        }

        private void GenerateConnectedNameCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            var name = data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(Unit.name, data) ? Flow.Predict<string>(Unit.name, graphPointer.AsReference()) : Unit.defaultValues[Unit.name.key] as string;

            switch (Unit.kind)
            {
                case VariableKind.Flow:
                case VariableKind.Graph:
                    using (writer.CodeDiagnosticScope($"{Unit.kind} Variables do not support connected names", CodeDiagnosticKind.Error))
                        writer.WriteIndented().Error($"Could not generate {Unit.kind} Variable").NewLine();
                    GenerateExitControl(Unit.assigned, data, writer);
                    return;
                case VariableKind.Object:
                    variableType = ResolveVariableType(VisualScripting.Variables.Object(GetTarget(data)), name);
                    break;
                case VariableKind.Scene:
                    variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                    break;
                case VariableKind.Application:
                    variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                    break;
                case VariableKind.Saved:
                    variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                    break;
            }

            writer.WriteIndented();
            var dictVar = data.AddLocalNameInScope("dictionaryVariable", variableType).VariableHighlight();
            writer.Write("var".ConstructHighlight());
            writer.Space();
            writer.Write(dictVar);
            writer.Write(" = ");
            WriteKind(writer, data);
            writer.Write(".GetDictionaryVariable");
            writer.Parentheses(inside => GenerateValue(Unit.name, data, inside));
            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented();
            writer.Write(dictVar);
            writer.Brackets(inside => GenerateValue(Unit.key, data, inside));
            writer.Write(" = ");

            using (data.Expect(GetDictionaryValueType(variableType)))
                GenerateValue(Unit.newValue, data, writer);

            writer.Write(";");
            writer.NewLine();

            data.CreateSymbol(Unit, variableType);
            GenerateExitControl(Unit.assigned, data, writer);
        }

        private void WriteKind(CodeWriter writer, ControlGenerationData data)
        {
            switch (Unit.kind)
            {
                case VariableKind.Object:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables)) + ".Object").Parentheses(w => GenerateValue(Unit.@object, data, w));
                    break;
                case VariableKind.Scene:
                    writer.Write(GetSceneKind(data, writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables))));
                    break;
                case VariableKind.Application:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables)) + "." + "Application".VariableHighlight());
                    break;
                case VariableKind.Saved:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables)) + "." + "Saved".VariableHighlight());
                    break;
            }
        }

        private void GenerateDirectNameCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            var name = Unit.defaultValues[Unit.name.key] as string;

            if (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved)
            {
                switch (Unit.kind)
                {
                    case VariableKind.Object:
                        {
                            var target = GetTarget(data);
                            variableType = ResolveVariableType(VisualScripting.Variables.Object(target), name);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                    case VariableKind.Scene:
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                    case VariableKind.Application:
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Application, name);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                    case VariableKind.Saved:
                        {
                            variableType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                            GenerateConnectedKindCodeInternal(data, writer);
                            return;
                        }
                }
            }

            var _name = data.GetVariableName(name.LegalMemberName());
            if (data.ContainsNameInAnyScope(_name))
            {
                variableName = _name.LegalMemberName();
                variableType = data.GetVariableType(_name);
                writer.WriteIndented();
                writer.Write(variableName.VariableHighlight());
                writer.Brackets(inside => GenerateValue(Unit.key, data, inside));
                writer.Write(" = ");
                using (data.Expect(GetDictionaryValueType(variableType)))
                    GenerateValue(Unit.newValue, data, writer);
                writer.Write(";");
                writer.NewLine();
                data.CreateSymbol(Unit, variableType);
                GenerateExitControl(Unit.assigned, data, writer);
            }
            else
            {
                writer.WriteIndented().Error($"Variable not found").NewLine();
                GenerateExitControl(Unit.assigned, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!Unit.assign.hasValidConnection)
            {
                writer.Error($"ControlInput {Unit.assign.key} requires connection on {Unit.GetType()}");
                return;
            }
            if (output == Unit.output && (Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved))
            {
                GenerateValue(Unit.newValue, data, writer);
                return;
            }
            else if (output == Unit.output && !Unit.name.hasValidConnection)
            {
                if (data.ContainsNameInAnyScope(variableName))
                {
                    writer.Write(variableName.VariableHighlight() + "[");
                    GenerateValue(Unit.key, data, writer);
                    writer.Write("]");
                    return;
                }
                else
                {
                    writer.Error($"Could not find variable with name \"{variableName}\"");
                    return;
                }
            }
            else
            {
                GenerateValue(Unit.newValue, data, writer);
            }
        }


        private void GenerateConnectedKindCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            var dictVar = data.AddLocalNameInScope("dictionaryVariable", variableType).VariableHighlight();
            writer.Write("var".ConstructHighlight());
            writer.Space();
            writer.Write(dictVar);
            writer.Write(" = ");
            WriteKind(writer, data);
            writer.Write(".GetDictionaryVariable");
            writer.Parentheses(inside => GenerateValue(Unit.name, data, inside));
            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented();
            writer.Write(dictVar);
            writer.Brackets(inside => GenerateValue(Unit.key, data, inside));
            writer.Write(" = ");
            using (data.Expect(GetDictionaryValueType(variableType)))
                GenerateValue(Unit.newValue, data, writer);
            writer.Write(";");
            writer.NewLine();

            data.CreateSymbol(Unit, variableType);
            GenerateExitControl(Unit.assigned, data, writer);
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "ActiveScene".VariableHighlight();
        }
        private Type GetDictionaryValueType(Type type)
        {
            if (type.IsGenericType && typeof(IDictionary).IsAssignableFrom(type))
                return type.GetGenericArguments()[1];
            return typeof(object);
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

        private static Type GetCachedType(string typeId)
        {
            if (!typeCache.TryGetValue(typeId, out var type))
            {
                type = Type.GetType(typeId) ?? typeof(object);
                typeCache[typeId] = type;
            }
            return type;
        }

        private Type ResolveVariableType(VariableDeclarations declarations, string name)
        {
            if (!declarations.IsDefined(name))
                return typeof(object);

            var declaration = declarations.GetDeclaration(name);

#if VISUAL_SCRIPTING_1_7
            var id = declaration.typeHandle.Identification;
            return string.IsNullOrEmpty(id) ? typeof(object) : GetCachedType(id);
#else
            return declaration.value != null ? declaration.value.GetType() : typeof(object);
#endif
        }
    }
}