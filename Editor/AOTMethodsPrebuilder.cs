using System;
using Unity.VisualScripting.Community;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine.Events;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;

public class AOTMethodsPrebuilder : IPreprocessBuildWithReport
{
    public int callbackOrder => 1;

    private List<TypeGroup> typesToSupport = new();

    public void OnPreprocessBuild(BuildReport report)
    {
        GenerateScript();
        AssetDatabase.Refresh();
    }


    private void GenerateScript()
    {
        EditorUtility.DisplayProgressBar("Generating Aot Support", "Finding Script Graphs and Scenes...", 0f);
        typesToSupport = GetTypesForAOTMethods().Distinct().ToList();
        typesToSupport.RemoveAll(type => type.types.Count == 0);
        string scriptContent = GenerateScriptContent(typesToSupport);
        string path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/Scripts/AotSupportMethods.cs";
        Debug.Log("Generated OnUnityEvent Support script at : " + path + "\nEnsure that the script stays here");
        HUMIO.Ensure(path).Path();
        File.WriteAllText(path, scriptContent);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    private string GenerateScriptContent(List<TypeGroup> typeGroups)
    {
        HashSet<string> namespaces = new HashSet<string>();

        foreach (var typeGroup in typeGroups)
        {
            foreach (var type in typeGroup.types)
            {
                string namespaceName = type.Namespace;
                if (!string.IsNullOrEmpty(namespaceName) && !IsIncludedNamespace(namespaceName) && AllowedNameSpace(namespaceName))
                {
                    namespaces.Add(namespaceName);
                }
            }

        }

        string namespaceDeclarations = string.Join(Environment.NewLine, namespaces.Select(ns => $"using {ns};"));
        string scriptContent = $"{namespaceDeclarations}\n";
        scriptContent += @"using System;
using UnityEngine.Events;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;

public static class AotSupportMethods
{
";

        foreach (var typeGroup in typeGroups)
        {
            string methodParameters = string.Join(", ", typeGroup.types.Select(type => CSharpName(type, true)));

            string methodReturnType = "UnityAction<" + methodParameters + ">";

            scriptContent += $"    public static {methodReturnType} {string.Join("_", typeGroup.types.Select(type => CSharpName(type, false)))}Handler(GraphReference reference, OnUnityEvent onUnityEvent)\n";
            scriptContent += "    {\n";

            // Construct argument list
            List<string> args = new List<string>();
            for (int i = 0; i < typeGroup.types.Count; i++)
            {
                args.Add("arg" + i);
            }

            // Construct event data initialization
            string eventData = "";
            for (int i = 0; i < typeGroup.types.Count; i++)
            {
                eventData += $"Value{i} = arg{i},{(i != typeGroup.types.Count - 1 ? "\n" : "")}\t\t\t\t";
            }

            // Append method body
            scriptContent += @$"        return ({string.Join(", ", args)}) => 
        {{
            onUnityEvent.Trigger(reference, new EventData
            {{  
                {eventData}
            }});
        }};
";
            scriptContent += "    }\n\n";
        }

        scriptContent += "}\n";

        return scriptContent;
    }


    private string CSharpName(Type type, bool lowerCase)
    {
        if (lowerCase)
        {
            if (type == null) return "null";
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(object) && type.BaseType == null) return "object";
            if (type == typeof(object[])) return "object[]";

            if (type.IsGenericType)
            {
                string genericTypeName = $"{type.Namespace}.{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(arg => CSharpName(arg, true)))}>";
                return string.Concat(genericTypeName.Where(c => char.IsLetter(c) || c == '<' || c == '>' || c == '.' || c == ','));
            }

            return type.Namespace + "." + type.Name;
        }
        else
        {
            if (type == null) return "null";
            if (type == typeof(int)) return "Int";
            if (type == typeof(string)) return "String";
            if (type == typeof(float)) return "Float";
            if (type == typeof(double)) return "Double";
            if (type == typeof(bool)) return "Bool";
            if (type == typeof(byte)) return "Byte";
            if (type == typeof(object) && type.BaseType == null) return type.Namespace + "Object";
            if (type == typeof(object[])) return "ObjectArray";

            string typeName = string.Concat(type.Name.Split('.').Select(word => char.ToUpper(word[0]) + word.Substring(1)));

            if (type.IsGenericType)
            {
                typeName = typeName.Replace("`", "Generic");
            }

            if (type.IsArray)
            {
                typeName = typeName.Replace("[]", "Array");
            }

            typeName = string.Concat(typeName.Where(c => char.IsLetterOrDigit(c) || c == '_'));

            return typeName;
        }
    }

    public List<TypeGroup> GetTypesForAOTMethods()
    {
        List<TypeGroup> types = new List<TypeGroup>();

        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => type.Inherits(typeof(UnityEventBase)))))
        {
            if (type.BaseType.IsGenericType && !type.BaseType.GetGenericArguments().Any(arg => arg.IsGenericTypeParameter) && type.BaseType.GetGenericArguments().All(_type => _type.IsPublic && AllowedNameSpace(_type.Namespace)))
            {
                types.Add(new TypeGroup(type.BaseType.GetGenericArguments()));
            }
        }

        return types;
    }

    private bool IsIncludedNamespace(string _namespace)
    {
        if (string.IsNullOrEmpty(_namespace)) return false;
        return _namespace == "System" || _namespace == "UnityEngine.Events" || _namespace == "Unity.VisualScripting" || _namespace == "Unity.VisualScripting.Community";
    }

    private bool AllowedNameSpace(string _namespace)
    {
        if (string.IsNullOrEmpty(_namespace)) return false;
        if (_namespace.Contains("UnityEditor") || _namespace.Contains("NUnit")) return false;
        return true;
    }
}
