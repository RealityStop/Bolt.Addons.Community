using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Bolt.Addons.Integrations.Continuum.Humility;
using Bolt.Addons.Integrations.Continuum.CSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Code.Editor
{
    [CodeGenerator(typeof(DelegateAsset))]
    public sealed class DelegateAssetGenerator : CodeGenerator<DelegateAsset>
    {
        public override string Generate(int indent)
        {
            var output = string.Empty;
            NamespaceGenerator @namespace = NamespaceGenerator.Namespace(null);
            ClassGenerator @class = null;

            var gens = string.Empty;
            var gen = Data.type.type.Name.EnsureNonConstructName().LegalMemberName();

            for (int i = 0; i < Data.generics.Count; i++)
            {
                gens += Data.generics[i].type.type.As().CSharpName().RemoveHighlights().RemoveMarkdown().Replace("&", string.Empty);
            }

            Data.title = gen + gens;

            if (!string.IsNullOrEmpty(Data.category))
            {
                @namespace = NamespaceGenerator.Namespace(Data.category);
            }

            var interfaceType = Data.type.type.GetMethod("Invoke").ReturnType == typeof(void) ? typeof(IAction) : typeof(IFunc);
            var isAction = Data.type.type.GetMethod("Invoke").ReturnType == typeof(void) ? true : false;
            @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title, typeof(object)).ImplementInterface(interfaceType);
            
            var properties = string.Empty;

            var method = Data.type.type.GetMethod("Invoke");
            var parameterUsings = new List<string>();
            var parameters = method.GetParameters();
            var parameterNames = new List<string>();
            var something = new Dictionary<ParameterInfo, string>();

            for (int i = 0; i < parameters.Length; i++)
            {
                properties += " new".ConstructHighlight() + " TypeParam".TypeHighlight() + "() { name = " + $@"""{parameters[i].Name}""".StringHighlight() + ", type = " + "typeof".ConstructHighlight() + "(" + (parameters[i].ParameterType.IsGenericParameter ? Data.generics[i].type.type.As().CSharpName() : parameters[i].ParameterType.As().CSharpName()) + ") }";
                if (!parameterUsings.Contains(parameters[i].ParameterType.Namespace)) parameterUsings.Add(parameters[i].ParameterType.Namespace);
                if (i < parameters.Length - 1) properties += ", ";
            }

            @class.AddUsings(parameterUsings);

            if (string.IsNullOrEmpty(properties))
            {
                properties += " ";
            }
            else
            {
                properties = " " + properties + " ";

            }

            var displayName = string.IsNullOrEmpty(Data.displayName) ? Data.type.type.As().CSharpName().RemoveHighlights().RemoveMarkdown() : Data.displayName;

            var constructors = Data.type.type.GetConstructors();
            var constructorParameters = constructors[constructors.Length - 1 > 0 ? 1 : 0];
            var stringConstructorParameters = new List<string>();
            var constParams = parameters.ToListPooled();
            var assignParams = string.Empty;

            if (constParams.Count <= 0)
            {
            }

            var constParamsLength = constParams.Count;
            var invokeString = string.Empty;

            for (int i = constParamsLength - 1; i >= 0; i--)
            {
                if (constParams[i].Name == "object" || constParams[i].Name == "method") constParams.Remove(constParams[i]);
            }

             
            for (int i = 0; i < constParams.Count; i++)
            {
                assignParams += constParams[i].Name.EnsureNonConstructName().Replace("&", string.Empty);
                stringConstructorParameters.Add(constParams[i].Name.EnsureNonConstructName().Replace("&", string.Empty));
                if (i < constParams.Count - 1)
                {
                    assignParams += ", ";
                }
            }

            var remappedGenerics = string.Empty;
            var remappedGeneric = Data.type.type.Name.EnsureNonConstructName();

            for (int i = 0; i < Data.generics.Count; i++)
            {
                remappedGenerics += Data.generics[i].type.type.As().CSharpName().Replace("&", string.Empty);
                if (i < Data.generics.Count - 1)
                {
                    remappedGenerics += ", ";
                }
            }

            if (Data.generics.Count - 1 >= 0 || isAction)
            {
                for (int i = 0; i < (isAction ? Data.generics.Count : Mathf.Clamp(Data.generics.Count-1, 0, Data.generics.Count)); i++)
                {
                    if (i < Data.generics.Count - 1 || isAction)
                    {
                        invokeString += $"({ Data.generics[i].type.type.As().CSharpName().Replace("&", string.Empty)})parameters[{i.ToString()}]";
                        if (i < Data.generics.Count - (isAction ? 1 : 2)) invokeString += ", ";
                    }
                }
            }

            if (Data.generics.Count > 0)
            {
                remappedGeneric = remappedGeneric.Contains("`") ? remappedGeneric.Remove(remappedGeneric.IndexOf("`"), remappedGeneric.Length - remappedGeneric.IndexOf("`")) : remappedGeneric;
                remappedGeneric = remappedGeneric.TypeHighlight() + "<" + remappedGenerics + ">";
            }
            else
            {
                remappedGeneric.TypeHighlight(); 
            }

            @class.AddField(FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, remappedGeneric, Data.type.type.Namespace, "callback"));

            @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(TypeParam), "parameters", false).SingleStatementGetter(AccessModifier.Public, "new".ConstructHighlight() + " TypeParam".TypeHighlight() + "[] {" + properties.Replace("&", string.Empty) + "}").AddTypeIndexer(""));

            @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(string), "DisplayName", false).SingleStatementGetter(AccessModifier.Public, $@"""{remappedGeneric.RemoveHighlights().RemoveMarkdown().Replace("<", " (").Replace(">", ")")}""".StringHighlight()));

            if (!isAction) @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(Type), "ReturnType", false).SingleStatementGetter(AccessModifier.Public, "typeof".ConstructHighlight() + "(" + Data.generics[Data.generics.Count > 1 ? Data.generics.Count - 2 : Data.generics.Count - 1].type.type.As().CSharpName() + ")"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(Type), "GetDelegateType").Body("return typeof".ConstructHighlight() + "(" + remappedGeneric + ");"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(object), "GetDelegate").Body("return".ConstructHighlight() + " callback;"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, interfaceType == typeof(IAction) ? typeof(void) : typeof(object), "Invoke").AddParameter(ParameterGenerator.Parameter("parameters", typeof(object[]), Integrations.Continuum.CSharp.ParameterModifier.None, isParameters:true)).Body($"{(isAction ? string.Empty : "return").ConstructHighlight()} callback({invokeString});"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Initialize").
                AddParameter(ParameterGenerator.Parameter("flow", typeof(Flow), Integrations.Continuum.CSharp.ParameterModifier.None)).
                AddParameter(ParameterGenerator.Parameter("unit", isAction ? typeof(ActionUnit) : typeof(FuncUnit), Integrations.Continuum.CSharp.ParameterModifier.None)).
                AddParameter(ParameterGenerator.Parameter(isAction ? "flowAction" : "flowFunc", isAction ? typeof(Action) : typeof(Func<object>), Integrations.Continuum.CSharp.ParameterModifier.None))
                .Body("callback = " + "new ".ConstructHighlight() + remappedGeneric + "(" + LambdaGenerator.SingleLine(stringConstructorParameters, (stringConstructorParameters.Count > 0 ? "unit.AssignParameters(flow, " + assignParams + "); " + (isAction ? "flowAction.Invoke();" : "return ".ConstructHighlight() + "(" + Data.generics[Data.generics.Count - 1].type.type.As().CSharpName() + ")" + "flowFunc();") : (isAction ? "flowAction.Invoke();" : "return ".ConstructHighlight() + "(" + Data.generics[Data.generics.Count-1].type.type.As().CSharpName() + ")" + "flowFunc();"))).Generate(0) + ");"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Bind").
                AddParameter(ParameterGenerator.Parameter("other", typeof(IDelegate), Integrations.Continuum.CSharp.ParameterModifier.None))
                .Body(
                "callback += (" + remappedGeneric + ")other.GetDelegate();"
                ));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Unbind").
                AddParameter(ParameterGenerator.Parameter("other", typeof(IDelegate), Integrations.Continuum.CSharp.ParameterModifier.None))
                .Body(
                "callback -= (" + remappedGeneric + ")other.GetDelegate();"
                ));

            @class.AddUsings(new List<string>() { Data.type.type.Namespace, "System" });

            @class.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));

            var currentName = Data.title;
            if (Data.lastCompiledName != currentName && !string.IsNullOrEmpty(Data.lastCompiledName))
            {
                @class.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
                Data.lastCompiledName = currentName;
            }

            var genericNamespace = new List<string>();
            for (int i = 0; i < Data.generics.Count; i++)
            {
                if(!genericNamespace.Contains(Data.generics[i].type.type.Namespace))genericNamespace.Add(Data.generics[i].type.type.Namespace);
            }
            @class.AddUsings(genericNamespace);
            @class.name = Data.title;
            @namespace.AddClass(@class);

            return @namespace.Generate(indent);
        }

    }
}
