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
        private bool IsAction => Data.type.type.GetMethod("Invoke").ReturnType == typeof(void);
        private bool IsFunc => Data.type.type.GetMethod("Invoke").ReturnType != typeof(void);
        private Type DelegateType => Data.type.type.GetMethod("Invoke").ReturnType == typeof(void) ? typeof(IAction) : typeof(IFunc);

        public override string Generate(int indent)
        {
            NamespaceGenerator @namespace = NamespaceGenerator.Namespace(null);

            var title = GetCompoundTitle();
            var delegateType = DelegateType;

            Data.title = title;
            
            if (!string.IsNullOrEmpty(Data.category))
            {
                @namespace = NamespaceGenerator.Namespace(Data.category);
            }

            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, title, typeof(object)).ImplementInterface(delegateType);
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

            if (Data.generics.Count - 1 >= 0 || IsAction)
            {
                for (int i = 0; i < (IsAction ? Data.generics.Count : Mathf.Clamp(Data.generics.Count-1, 0, Data.generics.Count)); i++)
                {
                    if (i < Data.generics.Count - 1 || IsAction)
                    {
                        invokeString += $"({ Data.generics[i].type.type.As().CSharpName().Replace("&", string.Empty)})parameters[{i.ToString()}]";
                        if (i < Data.generics.Count - (IsAction ? 1 : 2)) invokeString += ", ";
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

            if (IsFunc) @class.AddProperty(PropertyGenerator.Property(AccessModifier.Public, PropertyModifier.None, typeof(Type), "ReturnType", false).SingleStatementGetter(AccessModifier.Public, "typeof".ConstructHighlight() + "(" + Data.generics[Data.generics.Count > 1 ? Data.generics.Count - 2 : Data.generics.Count - 1].type.type.As().CSharpName() + ")"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(Type), "GetDelegateType").Body("return typeof".ConstructHighlight() + "(" + remappedGeneric + ");"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(object), "GetDelegate").Body("return".ConstructHighlight() + " callback;"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, IsAction ? typeof(void) : typeof(object), "Invoke").AddParameter(ParameterGenerator.Parameter("parameters", typeof(object[]), Integrations.Continuum.CSharp.ParameterModifier.None, isParameters:true)).Body($"{(IsAction ? string.Empty : "return").ConstructHighlight()} callback({invokeString});"));

            @class.AddMethod(MethodGenerator.Method(AccessModifier.Public, MethodModifier.None, typeof(void), "Initialize").
                AddParameter(ParameterGenerator.Parameter("flow", typeof(Flow), Integrations.Continuum.CSharp.ParameterModifier.None)).
                AddParameter(ParameterGenerator.Parameter("unit", IsAction ? typeof(ActionUnit) : typeof(FuncUnit), Integrations.Continuum.CSharp.ParameterModifier.None)).
                AddParameter(ParameterGenerator.Parameter(IsAction ? "flowAction" : "flowFunc", IsAction ? typeof(Action) : typeof(Func<object>), Integrations.Continuum.CSharp.ParameterModifier.None))
                .Body("callback = " + "new ".ConstructHighlight() + remappedGeneric + "(" + LambdaGenerator.SingleLine(stringConstructorParameters, (stringConstructorParameters.Count > 0 ? "unit.AssignParameters(flow, " + assignParams + "); " + (IsAction ? "flowAction.Invoke();" : "return ".ConstructHighlight() + "(" + Data.generics[Data.generics.Count - 1].type.type.As().CSharpName() + ")" + "flowFunc();") : (IsAction ? "flowAction.Invoke();" : "return ".ConstructHighlight() + "(" + Data.generics[Data.generics.Count-1].type.type.As().CSharpName() + ")" + "flowFunc();"))).Generate(0) + ");"));

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

            if (Data.lastCompiledName != Data.title && !string.IsNullOrEmpty(Data.lastCompiledName))
            {
                @class.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
            }

            var genericNamespace = new List<string>();

            for (int i = 0; i < Data.generics.Count; i++)
            {
                if(!genericNamespace.Contains(Data.generics[i].type.type.Namespace))genericNamespace.Add(Data.generics[i].type.type.Namespace);
            }

            @class.AddUsings(genericNamespace);

            @namespace.AddClass(@class);

            return @namespace.Generate(indent);
        }


        private string GetCompoundTitle()
        {
            var gens = string.Empty;
            var gen = Data.type.type.Name.RemoveAfterFirst("`".ToCharArray()[0]).LegalMemberName();

            for (int i = 0; i < Data.generics.Count; i++)
            {
                gens += Data.generics[i].type.type.As().CSharpName().RemoveHighlights().RemoveMarkdown().Replace("&", string.Empty);
            }

            return gen + gens;
        }

    }
}
