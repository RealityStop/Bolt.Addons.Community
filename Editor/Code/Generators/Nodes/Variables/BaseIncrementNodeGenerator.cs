using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class BaseIncrementDecrementNodeGenerator<TUnit> : LocalVariableGenerator where TUnit : IncrementNode
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        protected TUnit Unit => (TUnit)unit;
        protected string preVariable;
        protected string postVariable;

        protected abstract string OperationKeyword { get; }
        protected abstract string MethodName { get; }

        public BaseIncrementDecrementNodeGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.kind == VariableKind.Scene)
                yield return "UnityEngine.SceneManagement";
            yield return "Unity.VisualScripting";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!Unit.assign.hasValidConnection)
            {
                writer.Error($"ControlInput {Unit.assign.key} requires connection");
                return;
            }

            if (Unit.kind == VariableKind.Flow || Unit.kind == VariableKind.Graph)
            {
                if (output == Unit.preIncrement)
                {
                    writer.Write(preVariable.VariableHighlight());
                    return;
                }
                if (output == Unit.postIncrement)
                {
                    writer.Write(postVariable.VariableHighlight());
                    return;
                }
            }

            if (output == Unit.preIncrement)
                writer.Write($"{variableName}.pre".VariableHighlight());
            else if (output == Unit.postIncrement)
                writer.Write($"{variableName}.post".VariableHighlight());
            else
                base.GenerateValueInternal(output, data, writer);
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
            bool validKind = Unit.kind != VariableKind.Flow && Unit.kind != VariableKind.Graph;

            if (!validKind)
            {
                using (writer.CodeDiagnosticScope($"{Unit.kind} Variables do not support connected names", CodeDiagnosticKind.Error))
                    writer.Error($"Could not generate Variable");
                GenerateExitControl(Unit.assigned, data, writer);
                return;
            }

            variableName = data.AddLocalNameInScope($"{MethodName.ToLower()}Value");

            writer.WriteIndented();
            writer.Write("var".ConstructHighlight());
            writer.Space();
            writer.Write(variableName.VariableHighlight());
            writer.Write(" = ");
            writer.CallCSharpUtilityMethod(MethodName, writer.Action(w =>
            {
                GetVariableKindAccessor(Unit.kind, string.Empty, data, writer, out variableType);
                if (variableType == typeof(object)) variableType = typeof(float);
                w.Write($".Get<{variableType.As().CSharpName(false, true)}>(");
                GenerateValue(Unit.name, data, w);
                w.Write(")");
            }));
            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented();
            GetVariableKindAccessor(Unit.kind, string.Empty, data, writer, out variableType);
            writer.Write(".Set(");
            GenerateValue(Unit.name, data, writer);
            writer.ParameterSeparator();
            writer.Write(variableName.VariableHighlight());
            writer.Write(".");
            writer.Write("post".VariableHighlight());
            writer.Write(");");
            writer.NewLine();

            GenerateExitControl(Unit.assigned, data, writer);
        }

        private void GenerateDirectNameCodeInternal(ControlGenerationData data, CodeWriter writer)
        {
            var name = Unit.defaultValues[Unit.name.key] as string;
            var legalName = name.LegalMemberName();
            variableName = legalName;

            if (Unit.kind == VariableKind.Flow || Unit.kind == VariableKind.Graph)
            {
                if (!data.ContainsNameInAnyScope(legalName))
                {
                    writer.WriteIndented();
                    writer.Write("var".ConstructHighlight());
                    writer.Space();
                    writer.Write(legalName.VariableHighlight());
                    writer.Write(" = ");
                    writer.Write("0".NumericHighlight());
                    writer.Write(";");
                    writer.NewLine();
                }
                preVariable = data.AddLocalNameInScope($"pre{legalName}").VariableHighlight();
                postVariable = data.AddLocalNameInScope($"post{legalName}").VariableHighlight();

                writer.WriteIndented();
                writer.Write("var".ConstructHighlight());
                writer.Space();
                writer.Write(preVariable);
                writer.Write(" = ");
                writer.Write(legalName.VariableHighlight());
                writer.Write(";");
                writer.NewLine();

                writer.WriteIndented();
                writer.Write(legalName.VariableHighlight());
                writer.Write(OperationKeyword);
                writer.Write(";");
                writer.NewLine();

                writer.WriteIndented();
                writer.Write("var".ConstructHighlight());
                writer.Space();
                writer.Write(postVariable);
                writer.Write(" = ");
                writer.Write(legalName.VariableHighlight());
                writer.Write(";");
                writer.NewLine();

                GenerateExitControl(Unit.assigned, data, writer);
                return;
            }

            var localName = data.AddLocalNameInScope($"{MethodName.ToLower()}{legalName}").VariableHighlight();

            writer.WriteIndented();
            writer.Write("var".ConstructHighlight());
            writer.Space();
            writer.Write(localName);
            writer.Write(" = ");
            writer.CallCSharpUtilityMethod(MethodName, writer.Action(w =>
            {
                GetVariableKindAccessor(Unit.kind, name, data, writer, out variableType);
                if (variableType == typeof(object)) variableType = typeof(float);
                w.Write($".Get<{variableType.As().CSharpName(false, true)}>(");
                w.Write(name.As().Code(false));
                w.Write(")");
            }));
            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented();
            GetVariableKindAccessor(Unit.kind, name, data, writer, out _);
            writer.Write(".Set(");
            writer.Write(name.As().Code(false));
            writer.ParameterSeparator();
            writer.Write(localName);
            writer.Write(".");
            writer.Write("post".VariableHighlight());
            writer.Write(");");
            writer.NewLine();

            variableName = localName;
            GenerateExitControl(Unit.assigned, data, writer);
        }

        protected void GetVariableKindAccessor(VariableKind kind, string name, ControlGenerationData data, CodeWriter writer, out Type resolvedType)
        {
            resolvedType = typeof(object);
            var variables = writer.GetTypeNameHighlighted(typeof(Unity.VisualScripting.Variables));

            switch (kind)
            {
                case VariableKind.Object:
                    var target = GetTarget(data);
                    if (target != null && VisualScripting.Variables.Object(target).IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.Object(target), name);
                    writer.Write($"{variables}.Object").Parentheses(w => GenerateValue(Unit.@object, data, w));
                    break;
                case VariableKind.Scene:
                    if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.ActiveScene, name);
                    writer.Write(GetSceneKind(data, variables));
                    break;
                case VariableKind.Application:
                    if (VisualScripting.Variables.Application.IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.Application, name);
                    writer.Write($"{variables}.{"Application".VariableHighlight()}");
                    break;
                case VariableKind.Saved:
                    if (VisualScripting.Variables.Saved.IsDefined(name))
                        resolvedType = ResolveVariableType(VisualScripting.Variables.Saved, name);
                    writer.Write($"{variables}.{"Saved".VariableHighlight()}");
                    break;
            }
        }

        private string GetSceneKind(ControlGenerationData data, string variables)
        {
            return typeof(Component).IsAssignableFrom(data.ScriptType) ? variables + ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")" : variables + "." + "ActiveScene".VariableHighlight();
        }

        private GameObject GetTarget(ControlGenerationData data)
        {
            var objInput = Unit.@object;
            if (!objInput.hasValidConnection && Unit.defaultValues[objInput.key] == null && data.TryGetGameObject(out var gameObject))
                return gameObject;
            if (!objInput.hasValidConnection && Unit.defaultValues[objInput.key] != null)
                return Unit.defaultValues[objInput.key].ConvertTo<GameObject>();
            if (objInput.hasValidConnection && data.TryGetGraphPointer(out var graphPointer) && CanPredictConnection(objInput, data))
            {
                try
                {
                    return Flow.Predict(objInput.GetPesudoSource(), graphPointer.AsReference()) as GameObject;
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError(ex);
                }
            }
            return null;
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