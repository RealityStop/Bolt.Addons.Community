using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [CodeGenerator(typeof(DelegateAsset))]
    [Serializable]
    public sealed class DelegateAssetGenerator : CodeGenerator<DelegateAsset>
    {
        private bool IsAction => Data.type.type.GetMethod("Invoke").ReturnType == typeof(void);
        private bool IsFunc => Data.type.type.GetMethod("Invoke").ReturnType != typeof(void);
        private Type DelegateType => Data.type.type.GetMethod("Invoke").ReturnType == typeof(void) ? typeof(IAction) : typeof(IFunc);

        public override ControlGenerationData CreateGenerationData()
        {
            return new ControlGenerationData(DelegateType, null);
        }

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            NamespaceGenerator @namespace = NamespaceGenerator.Namespace("Unity.VisualScripting.Community.Generated");

            if (Data != null)
            {
                var title = GetCompoundTitle();
                var delegateType = DelegateType;

                Data.title = title;

                if (!string.IsNullOrEmpty(Data.category))
                {
                    @namespace = NamespaceGenerator.Namespace(Data.category);
                }

                var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, title, typeof(object)).ImplementInterface(delegateType);
                if (Data.type.type == typeof(Action))
                {
                    @class.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter("Unity.VisualScripting.Community.Generated._GenericAction"));
                }
                var properties = string.Empty;
                var method = Data.type.type.GetMethod("Invoke");
                var parameterUsings = new List<string>();
                var parameters = method.GetParameters();

                for (int i = 0; i < parameters.Length; i++)
                {
                    properties += " new".ConstructHighlight() + " TypeParam".TypeHighlight() + $"() {{ {"name".VariableHighlight()} = " + $@"""{parameters[i].Name}""".StringHighlight() + $", {"type".VariableHighlight()} = " + "typeof".ConstructHighlight() + "(" + (parameters[i].ParameterType.IsGenericParameter ? Data.type.type.As().CSharpName() : parameters[i].ParameterType.As().CSharpName()) + ") }";
                    if (!parameterUsings.Contains(parameters[i].ParameterType.Namespace)) parameterUsings.Add(parameters[i].ParameterType.Namespace);
                    if (i < parameters.Length - 1) properties += ", ";
                }

                @class.AddUsings(parameterUsings);
                @class.AddUsings("Unity.VisualScripting.Community.Libraries.Humility".Yield().ToList());
                if (string.IsNullOrEmpty(properties))
                {
                    properties += " ";
                }
                else
                {
                    properties = " " + properties + " ";
                }

                var stringConstructorParameters = new List<string>();
                var constParams = parameters.ToListPooled();
                var assignParams = string.Empty;

                var constParamsLength = constParams.Count;
                var invokeString = string.Empty;

                for (int i = constParamsLength - 1; i >= 0; i--)
                {
                    if (constParams[i].Name == "object" || constParams[i].Name == "method") constParams.Remove(constParams[i]);
                }


                for (int i = 0; i < constParams.Count; i++)
                {
                    assignParams += constParams[i].Name.VariableHighlight().EnsureNonConstructName().Replace("&", string.Empty);
                    stringConstructorParameters.Add(constParams[i].Name.VariableHighlight().EnsureNonConstructName().Replace("&", string.Empty));
                    if (i < constParams.Count - 1)
                    {
                        assignParams += ", ";
                    }
                }

                if (Data.type.type.GetGenericArguments().Length - 1 >= 0 || IsAction)
                {
                    var generics = Data.type.type.GetGenericArguments();
                    for (int i = 0; i < (IsAction ? generics.Length : Mathf.Clamp(generics.Length - 1, 0, generics.Length)); i++)
                    {
                        if (i < generics.Length - 1 || IsAction)
                        {
                            invokeString += $"({generics[i].As().CSharpName().Replace("&", string.Empty)}){"parameters".VariableHighlight()}[{i}]";
                            if (i < generics.Length - (IsAction ? 1 : 2)) invokeString += ", ";
                        }
                    }
                }

                var type = Data.type.type.As().CSharpName().EnsureNonConstructName();

                @class.AddField(FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, type, Data.type.type.Namespace, "callback"));
                @class.AddField(FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, type, Data.type.type.Namespace, "instance"));

                @class.AddField(FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(bool), "_initialized"));

                @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(Unit), "Unit", false).SingleStatementGetter(AccessModifier.Public, null).SingleStatementSetter(AccessModifier.Public, null));

                @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(TypeParam[]), "parameters", false).SingleStatementGetter(AccessModifier.Public, w => w.Write("new".ConstructHighlight() + " TypeParam".TypeHighlight() + "[] {" + properties.Replace("&", string.Empty) + "}")));

                @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(string), "DisplayName", false).SingleStatementGetter(AccessModifier.Public, w => w.Write($@"""{type.RemoveHighlights().RemoveMarkdown().Replace("<", " (").Replace(">", ")")}""".StringHighlight())));

                @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(bool), "initialized", false).SingleStatementGetter(AccessModifier.Public, w => w.Write("_initialized".VariableHighlight())).SingleStatementSetter(AccessModifier.Public, w => w.Write($"{"_initialized".VariableHighlight()} = {"value".ConstructHighlight()}")));

                if (IsFunc) @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(Type), "ReturnType", false).SingleStatementGetter(AccessModifier.Public, w => w.Write("typeof".ConstructHighlight() + "(" + Data.type.type.GetGenericArguments().Last().As().CSharpName() + ")").NewLine()));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(Type), "GetDelegateType").Body(w => w.WriteIndented("return ".ControlHighlight() + "typeof".ConstructHighlight() + "(" + type + ");").NewLine()));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(object), "GetDelegate").Body(w => w.WriteIndented("return".ControlHighlight() + $" {"callback".VariableHighlight()};").NewLine()));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, IsAction ? typeof(void) : typeof(object), IsAction ? "Invoke" : "DynamicInvoke").AddParameter(ParameterGenerator.Parameter("parameters", typeof(object[]), Libraries.CSharp.ParameterModifier.Params)).Body(w => w.WriteIndented($"{(IsAction ? string.Empty : "return").ControlHighlight()} {"callback".VariableHighlight()}({invokeString});").NewLine()));

                if (!IsAction) @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, Data.type.type.GetGenericArguments().Last(), "Invoke").AddParameter(ParameterGenerator.Parameter("parameters", typeof(object[]), Libraries.CSharp.ParameterModifier.Params)).Body(w => w.WriteIndented($"{(IsAction ? string.Empty : "return").ControlHighlight()} {"callback".VariableHighlight()}({invokeString});").NewLine()));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Initialize").
                    AddParameter(ParameterGenerator.Parameter("flow", typeof(Flow), Libraries.CSharp.ParameterModifier.None)).
                    AddParameter(ParameterGenerator.Parameter("unit", IsAction ? typeof(ActionNode) : typeof(FuncNode), Libraries.CSharp.ParameterModifier.None)).
                    AddParameter(ParameterGenerator.Parameter(IsAction ? "flowAction" : "flowFunc", IsAction ? typeof(Action) : typeof(Func<object>), Libraries.CSharp.ParameterModifier.None)).
                    Body(w =>
                    {
                        w.WriteIndented($"SetInstance({"flow".VariableHighlight()}, {"unit".VariableHighlight()}, " + (IsAction ? "flowAction" : "flowFunc").VariableHighlight() + "); \n" + CodeBuilder.Indent(writer.IndentLevel) + $"{"callback".VariableHighlight()} = " + "new ".ConstructHighlight() + type + "(");
                        LambdaGenerator.SingleLine(stringConstructorParameters, (!IsAction ? "return".ControlHighlight() + " " : string.Empty) + $"{"instance".VariableHighlight()}({assignParams});").Generate(w, data);
                        w.Write(");\n" + CodeBuilder.Indent(writer.IndentLevel) + $"{"initialized".VariableHighlight()} = " + "true".ConstructHighlight() + ";\n");
                    }));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "SetInstance").
                    AddParameter(ParameterGenerator.Parameter("flow", typeof(Flow), Libraries.CSharp.ParameterModifier.None)).
                    AddParameter(ParameterGenerator.Parameter("unit", IsAction ? typeof(ActionNode) : typeof(FuncNode), Libraries.CSharp.ParameterModifier.None)).
                    AddParameter(ParameterGenerator.Parameter(IsAction ? "flowAction" : "flowFunc", IsAction ? typeof(Action) : typeof(Func<object>), Libraries.CSharp.ParameterModifier.None)).
                    Body(w =>
                    {
                        w.WriteIndented($"{"instance".VariableHighlight()} = " + "new ".ConstructHighlight() + type + "(");
                        LambdaGenerator.SingleLine(stringConstructorParameters, stringConstructorParameters.Count > 0 ? $"{"unit".VariableHighlight()}.AssignParameters({"flow".VariableHighlight()}, " + assignParams + "); " + (IsAction ? $"{"flowAction".VariableHighlight()}();" : "return ".ControlHighlight() + "(" + Data.type.type.GetGenericArguments().Last().As().CSharpName() + ")" + $"{"flowFunc".VariableHighlight()}();") : (IsAction ? $"{"flowAction".VariableHighlight()}();" : "return ".ControlHighlight() + "(" + Data.type.type.GetGenericArguments().Last().As().CSharpName() + ")" + $"{"flowFunc".VariableHighlight()}();")).Generate(w, data);
                        w.Write(");\n");
                    }));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Bind").
                    AddParameter(ParameterGenerator.Parameter("other", typeof(IDelegate), Libraries.CSharp.ParameterModifier.None))
                    .Body(w => w.WriteIndented($"{"callback".VariableHighlight()} += (" + type + $"){"other".VariableHighlight()}.GetDelegate();\n")));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Unbind").
                    AddParameter(ParameterGenerator.Parameter("other", typeof(IDelegate), Libraries.CSharp.ParameterModifier.None))
                    .Body(w => w.WriteIndented(
                    $"{"callback".VariableHighlight()} -= (" + type + $"){"other".VariableHighlight()}.GetDelegate();\n"
                    )));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Bind").
                    AddParameter(ParameterGenerator.Parameter("other", Data.type.type, Libraries.CSharp.ParameterModifier.None))
                    .Body(w => w.WriteIndented(
                    $"{"callback".VariableHighlight()} += {"other".VariableHighlight()};\n"
                )));

                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Unbind").
                    AddParameter(ParameterGenerator.Parameter("other", Data.type.type, Libraries.CSharp.ParameterModifier.None))
                    .Body(w => w.WriteIndented(
                    $"{"callback".VariableHighlight()} -= {"other".VariableHighlight()};\n"
                    )));
                string warning = $"\"Delegate {{".StringHighlight() + $"{"other".VariableHighlight()}.GetType().As().CSharpName({"false".ConstructHighlight()}, {"false".ConstructHighlight()}, {"false".ConstructHighlight()})" + $"}} is not of type {{".StringHighlight() + $"{"callback".VariableHighlight()}.GetType().As().CSharpName({"false".ConstructHighlight()}, {"false".ConstructHighlight()}, {"false".ConstructHighlight()})" + "}.\"".StringHighlight();
                @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Combine").
                    AddParameter(ParameterGenerator.Parameter("other", typeof(Delegate), Libraries.CSharp.ParameterModifier.None))
                    .Body(w => w.WriteIndented(
                        $"{"if".ControlHighlight()} (!({"other".VariableHighlight()} {"is".ConstructHighlight()} {type})) {"throw".ControlHighlight()} {"new".ConstructHighlight()} {"Exception".TypeHighlight()}(" + "$".StringHighlight() + warning + ");" + "\n" +
                        CodeBuilder.Indent(writer.IndentLevel) + $"{"callback".VariableHighlight()} = (" + type + $"){"Delegate".TypeHighlight()}.Combine({"callback".VariableHighlight()}, {"other".VariableHighlight()});\n"
                    )));

                @class.AddUsings(new List<string>() { Data.type.type.Namespace, "System" });

                @class.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));

                foreach (var name in Data.lastCompiledNames)
                {
                    if (!string.IsNullOrEmpty(Data.GetFullTypeName()) && name != Data.GetFullTypeName())
                    {
                        if (!string.IsNullOrEmpty(name))
                            @class.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                    }
                }

                @namespace.AddClass(@class);

                @namespace.Generate(writer, data);
                data.Dispose();
            }
        }

        private string GetCompoundTitle()
        {
            return (Data.type.type == typeof(Action) ? "Generic_" : string.Empty) + Data.type.type.HumanName(true).LegalMemberName();
        }
    }
}