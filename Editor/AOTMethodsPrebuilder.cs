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
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Reflection;
using ParameterModifier = Unity.VisualScripting.Community.Libraries.CSharp.ParameterModifier;
using UnityEngine.UI;

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
            EditorUtility.ClearProgressBar();
            typesToSupport.RemoveAll(type => type.types.Count == 0);
            string scriptContent = GenerateScriptContent(typesToSupport);
            var BasePath = Path.Combine(Application.dataPath, AssetCompiler.GeneratedPath);
            var ScriptsPath = Path.Combine(BasePath, "Scripts");
            Debug.Log("Generated OnUnityEvent Support script at : " + ScriptsPath + "\nEnsure that the script stays here");
            File.WriteAllText(Application.dataPath + "/" + AssetCompiler.GeneratedPath + "/Scripts" + "/AotSupportMethods.cs", scriptContent);
            AssetDatabase.Refresh();
        }

        private readonly List<string> includedNamespaces = new List<string>
        {
            "System",
            "UnityEngine.Events",
            "Unity.VisualScripting",
            "Unity.VisualScripting.Community"
        };
        private string GenerateScriptContent(List<TypeGroup> typeGroups)
        {
            HashSet<string> namespaces = new HashSet<string>(includedNamespaces);

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

            var @namespace = NamespaceGenerator.Namespace("Unity.VisualScripting.Community.Generated");
            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.Static, "AotSupportMethods", null, "", namespaces.ToList());
            var processedGroups = new HashSet<TypeGroup>();
            foreach (var typeGroup in typeGroups)
            {
                if (!processedGroups.Add(typeGroup))
                    continue;
                string methodParameters = string.Join(", ", typeGroup.types.Select(type => type.CSharpFullName(true)));
                string methodReturnType = "UnityAction<" + methodParameters + ">";
                var method = MethodGenerator.Method(AccessModifier.Public, MethodModifier.Static, methodReturnType, "UnityEngine.Events", string.Join("_", typeGroup.types.Select(type => type.HumanName(false).Replace(" ", ""))) + "Handler");
                method.AddParameter(ParameterGenerator.Parameter("reference", typeof(GraphReference), ParameterModifier.None));
                method.AddParameter(ParameterGenerator.Parameter("onUnityEvent", typeof(OnUnityEvent), ParameterModifier.None));
                method.SetWarning($"Method generated from {typeGroup.parent.CSharpFullName()}");
                List<string> args = new List<string>();
                for (int i = 0; i < typeGroup.types.Count; i++)
                {
                    args.Add("arg" + i);
                }

                string eventData = "";
                for (int i = 0; i < typeGroup.types.Count; i++)
                {
                    eventData += $"Value{i} = arg{i},{(i != typeGroup.types.Count - 1 ? "\n" : "")}\t\t";
                }


                var body = @$"return ({string.Join(", ", args)}) => 
{{
    onUnityEvent.Trigger(reference, new EventData
    {{  
        {eventData}
    }});
}};";
                method.Body(body);
                @class.AddMethod(method);
            }
            @namespace.AddClass(@class);
            return @namespace.GenerateClean(0);
        }

        private List<TypeGroup> GetTypesForAOTMethods()
        {
            List<TypeGroup> types = new List<TypeGroup>();

            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(type => typeof(UnityEventBase).IsAssignableFrom(type))).ToList();

            int count = allTypes.Count;
            for (int i = 0; i < count; i++)
            {
                var currentType = allTypes[i];
                var type = currentType.IsGenericType ? currentType : currentType.BaseType;
                float progress = (float)i / count;

                if (type.IsPublic && type.IsGenericType && !type.GetGenericArguments().Any(arg => arg.IsGenericTypeParameter) && type.GetGenericArguments().All(_type => _type.IsPublic && AllowedNameSpace(_type.Namespace)))
                {
                    EditorUtility.DisplayProgressBar("Generating Aot Support Methods for OnUnityEvent", $"Found {type.HumanName(true)}...", progress);

                    types.Add(new TypeGroup(currentType, type.GetGenericArguments()));
                }
            }

            return types;
        }

        private bool IsIncludedNamespace(string _namespace)
        {
            if (string.IsNullOrEmpty(_namespace)) return false;
            return includedNamespaces.Contains(_namespace);
        }

        private bool AllowedNameSpace(string _namespace)
        {
            if (string.IsNullOrEmpty(_namespace)) return false;
            if (_namespace.Contains("UnityEditor") || _namespace.Contains("NUnit")) return false;
            return true;
        }

        private class TypeGroup : IEquatable<TypeGroup>
        {
            public TypeGroup(Type parent, params Type[] types)
            {
                this.parent = parent;
                this.types = types.ToList();
            }

            public Type parent;
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