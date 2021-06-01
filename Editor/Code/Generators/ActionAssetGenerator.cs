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
    [CodeGenerator(typeof(ActionAsset))]
    public sealed class ActionAssetGenerator : CodeGenerator<ActionAsset>
    {
        public override string Generate(int indent)
        {
            var output = string.Empty;
            NamespaceGenerator @namespace = NamespaceGenerator.Namespace(null);
            ClassGenerator @class = null;

            if (string.IsNullOrEmpty(Data.title)) return output;

            if (!string.IsNullOrEmpty(Data.category))
            {
                @namespace = NamespaceGenerator.Namespace(Data.category);
            }

            @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title.LegalMemberName(), typeof(object)).ImplementInterface(typeof(IAction));

            @class.AddField(FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, Data.type.type, "callback"));

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

            var remappedGenerics = string.Empty;
             
            for (int i = 0; i < constParams.Count; i++)
            {
                assignParams += constParams[i].Name.EnsureNonConstructName().Replace("&", string.Empty);
                stringConstructorParameters.Add(constParams[i].Name.EnsureNonConstructName().Replace("&", string.Empty));
                if (i < constParams.Count - 1)
                {
                    assignParams += ", ";
                }
            }

            var remappedGeneric = Data.type.type.Name.EnsureNonConstructName();

            for (int i = 0; i < Data.generics.Count; i++)
            {
                remappedGenerics += Data.generics[i].type.type.As().CSharpName().Replace("&", string.Empty);
                invokeString += $"({ Data.generics[i].type.type.As().CSharpName().Replace("&", string.Empty)})parameters[{i.ToString()}]";
                if (i < Data.generics.Count - 1)
                {
                    remappedGenerics += ", ";
                    invokeString += ", ";
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

            @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(TypeParam), "parameters", false).SingleStatementGetter(AccessModifier.Public, "new".ConstructHighlight() + " TypeParam".TypeHighlight() + "[] {" + properties.Replace("&", string.Empty) + "}").AddTypeIndexer(""));

            @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(string), "DisplayName", false).SingleStatementGetter(AccessModifier.Public, $@"""{remappedGeneric.RemoveHighlights().RemoveMarkdown()}""".StringHighlight()));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(Type), "GetDelegateType").Body("return typeof".ConstructHighlight() + "(" + remappedGeneric + ");"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(object), "GetDelegate").Body("return".ConstructHighlight() + " callback;"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Invoke").AddParameter(ParameterGenerator.Parameter("parameters", typeof(object[]), Integrations.Continuum.CSharp.ParameterModifier.None, isParameters:true)).Body($"callback({invokeString});"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Initialize").
                AddParameter(ParameterGenerator.Parameter("flow", typeof(Flow), Integrations.Continuum.CSharp.ParameterModifier.None)).
                AddParameter(ParameterGenerator.Parameter("unit", typeof(ActionUnit), Integrations.Continuum.CSharp.ParameterModifier.None)).
                AddParameter(ParameterGenerator.Parameter("flowAction", typeof(Action), Integrations.Continuum.CSharp.ParameterModifier.None))
                .Body("callback = " + "new ".ConstructHighlight() + remappedGeneric + "(" + LambdaGenerator.SingleLine(stringConstructorParameters, (stringConstructorParameters.Count > 0 ? "unit.AssignParameters(flow, " + assignParams + "); " + "flowAction.Invoke();" : "flowAction.Invoke();")).Generate(0) + ");"));

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

            if (!string.IsNullOrEmpty(Data.lastCompiledName))
            {
                var currentName = Data.category + (string.IsNullOrEmpty(Data.category) ? string.Empty : ".") + Data.title;
                if (Data.lastCompiledName != currentName)
                {
                    @class.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
                }
            }

            @namespace.AddClass(@class);

            return @namespace.Generate(indent);
        }

    }
}
