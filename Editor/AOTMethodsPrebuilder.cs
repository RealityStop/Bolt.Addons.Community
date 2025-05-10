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

namespace Unity.VisualScripting.Community
{
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
                string methodParameters = string.Join(", ", typeGroup.types.Select(type => type.CSharpFullName(true)));

                string methodReturnType = "UnityAction<" + methodParameters + ">";

                scriptContent += $"    public static {methodReturnType} {string.Join("_", typeGroup.types.Select(type => type.HumanName(false).Replace(" ", "")))}Handler(GraphReference reference, OnUnityEvent onUnityEvent)\n";
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

        public List<TypeGroup> GetTypesForAOTMethods()
        {
            List<TypeGroup> types = new List<TypeGroup>();

            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => type.Inherits(typeof(UnityEventBase)))))
            {
                if (type.IsPublic && type.BaseType.IsGenericType && !type.BaseType.GetGenericArguments().Any(arg => arg.IsGenericTypeParameter) && type.BaseType.GetGenericArguments().All(_type => _type.IsPublic && AllowedNameSpace(_type.Namespace)))
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

        public class TypeGroup : IEquatable<TypeGroup>
        {
            public TypeGroup(params Type[] types)
            {
                this.types = types.ToList();
            }

            public List<Type> types = new List<Type>();

            public bool Equals(TypeGroup other)
            {
                if (other == null || other.types.Count != types.Count)
                {
                    return false;
                }

                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i] != other.types[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                if (obj is TypeGroup other)
                {
                    return Equals(other);
                }
                return false;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    foreach (var type in types)
                    {
                        hash = hash * 23 + type.GetHashCode();
                    }
                    return hash;
                }
            }
        }
    }
}