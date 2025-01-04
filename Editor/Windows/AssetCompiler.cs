using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System;
using System.IO;

namespace Unity.VisualScripting.Community
{
    public static class AssetCompiler
    {
        private static readonly Dictionary<Assembly, bool> _editorAssemblyCache = new Dictionary<Assembly, bool>();

        [MenuItem("Addons/Compile All")]
        public static void Compile()
        {
            if (EditorUtility.DisplayDialog("Compile All Assets", "Compile all Graph, Class, Struct, Interace, Enum and Delegate Assets? This will clear all scripts in the 'Unity.VisualScripting.Community.Generated/Scripts, /Enums, /Interfaces, /Delegates' folder make sure to backup any scripts.", "Yes", "No"))
            {
                DoCompileAll();
            }
        }

        private static void DoCompileAll()
        {
            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";
            var interfacePath = scriptsPath + "Interfaces/";
            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();
            HUMIO.Ensure(interfacePath).Path();

            HUMIO.Delete(oldPath);
            HUMIO.Delete(delegatesPath);
            HUMIO.Delete(csharpPath);
            HUMIO.Delete(enumPath);
            HUMIO.Delete(interfacePath);

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();
            HUMIO.Ensure(interfacePath).Path();

            var classes = HUMAssets.Find().Assets().OfType<ClassAsset>().ToList();
            var structs = HUMAssets.Find().Assets().OfType<StructAsset>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var delegates = HUMAssets.Find().Assets().OfType<DelegateAsset>();
            var interfaces = HUMAssets.Find().Assets().OfType<InterfaceAsset>();
            var scriptGraphAssets = HUMAssets.Find().Assets().OfType<ScriptGraphAsset>();

            for (int i = 0; i < classes.Count; i++)
            {
                var asset = classes[i];
                var fullPath = csharpPath + classes[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                if (!asset.lastCompiledNames.Contains(name))
                    asset.lastCompiledNames.Add(name);
                var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + asset.title.LegalMemberName() + ".cs");
                var variables = asset.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(variable.type) && variable.scope == AccessModifier.Public);
                var variableNames = variables.Select(variable => variable.FieldName).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.defaultValue).ToArray();
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
                scriptImporter.SetIcon(asset.icon);
            }

            for (int i = 0; i < structs.Count; i++)
            {
                var asset = structs[i];
                var fullPath = csharpPath + structs[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(StructAssetGenerator.GetSingleDecorator(structs[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                if (!asset.lastCompiledNames.Contains(name))
                    asset.lastCompiledNames.Add(name);
                var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + asset.title.LegalMemberName() + ".cs");
                scriptImporter.SetIcon(asset.icon);
            }

            for (int i = 0; i < enums.Count; i++)
            {
                var asset = enums[i];
                var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                if (!asset.lastCompiledNames.Contains(name))
                    asset.lastCompiledNames.Add(name);
            }

            for (int i = 0; i < interfaces.Count; i++)
            {
                var asset = interfaces[i];
                var fullPath = interfacePath + asset.title.LegalMemberName() + ".cs";
                HUMIO.Save(InterfaceAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                if (!asset.lastCompiledNames.Contains(name))
                    asset.lastCompiledNames.Add(name);
                var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(interfacePath + asset.title.LegalMemberName() + ".cs");
                scriptImporter.SetIcon(asset.icon);
            }

            for (int i = 0; i < delegates.Count; i++)
            {
                var asset = delegates[i];
                var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";
                var code = DelegateAssetGenerator.GetSingleDecorator(delegates[i]).GenerateClean(0);

                HUMIO.Save(code).Custom(fullPath).Text(false);
                var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
                if (!asset.lastCompiledNames.Contains(name))
                    asset.lastCompiledNames.Add(name);
            }

            for (int i = 0; i < scriptGraphAssets.Count; i++)
            {
                var asset = scriptGraphAssets[i];
                if (asset.graph != null || asset.graph.units.Any(unit => unit is GraphInput or GraphOutput)) continue;
                var fullPath = csharpPath + (!string.IsNullOrEmpty(asset.graph.title) ? asset.graph.title.LegalMemberName() : asset.name.LegalMemberName()) + ".cs";
                HUMIO.Save(ScriptGraphAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Addons/Compile Selected")]
        public static void CompileSelected()
        {
            var codeAssets = Selection.GetFiltered<CodeAsset>(SelectionMode.Assets).ToList();
            var graphAssets = Selection.GetFiltered<ScriptGraphAsset>(SelectionMode.Assets).ToList();

            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var editorPath = path + "Editor/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";
            var interfacePath = scriptsPath + "Interfaces/";

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(editorPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();
            HUMIO.Ensure(interfacePath).Path();

            HUMIO.Delete(oldPath);

            foreach (var asset in codeAssets)
            {
                if (asset is ClassAsset classAsset)
                {
                    var fullPath = string.Empty;
                    if (classAsset.inheritsType && IsEditorAssembly(classAsset.inherits.type.Assembly, new HashSet<string>()))
                    {
                        fullPath = editorPath + classAsset.title.LegalMemberName() + ".cs";
                    }
                    else
                    {
                        fullPath = csharpPath + classAsset.title.LegalMemberName() + ".cs";
                    }

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classAsset).GenerateClean(0)).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + classAsset.title.LegalMemberName() + ".cs");
                    var variables = classAsset.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(variable.type) && variable.scope == AccessModifier.Public);
                    var variableNames = variables.Select(variable => variable.FieldName).ToArray();
                    var variableValues = variables.Select(variable => (UnityEngine.Object)variable.defaultValue).ToArray();
                    scriptImporter.SetDefaultReferences(variableNames, variableValues);
                    scriptImporter.SetIcon(classAsset.icon);
                }
                else if (asset is StructAsset structAsset)
                {
                    var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(StructAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath("Assets/Unity.VisualScripting.Community.Generated/Scripts/Objects/" + asset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(structAsset.icon);
                }
                else if (asset is EnumAsset)
                {
                    var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                }
                else if (asset is DelegateAsset)
                {
                    var generator = DelegateAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                }
                else if (asset is InterfaceAsset interfaceAsset)
                {
                    var generator = InterfaceAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = interfacePath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(interfacePath + interfaceAsset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(interfaceAsset.icon);
                }
            }

            foreach (var scriptGraphAsset in graphAssets)
            {
                string fullPath = csharpPath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = ScriptGraphAssetGenerator.GetSingleDecorator(scriptGraphAsset).GenerateClean(0);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + GetGraphName(scriptGraphAsset) + ".cs");
                var variables = scriptGraphAsset.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var variableNames = variables.Select(variable => variable.name).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).ToArray();
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CompileAsset(UnityEngine.Object targetasset)
        {
            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var editorPath = path + "Editor/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";
            var interfacePath = scriptsPath + "Interfaces/";

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(editorPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();
            HUMIO.Ensure(interfacePath).Path();

            HUMIO.Delete(oldPath);

            if (targetasset is CodeAsset asset)
            {
                if (asset is ClassAsset classAsset)
                {
                    string fullPath;
                    if (classAsset.inheritsType && IsEditorAssembly(classAsset.GetInheritedType().Assembly, new HashSet<string>()))
                    {
                        fullPath = editorPath + classAsset.title.LegalMemberName() + ".cs";
                    }
                    else
                    {
                        fullPath = csharpPath + classAsset.title.LegalMemberName() + ".cs";
                    }

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classAsset).GenerateClean(0)).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + classAsset.title.LegalMemberName() + ".cs");
                    var variables = classAsset.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(variable.type) && variable.scope == AccessModifier.Public);
                    var variableNames = variables.Select(variable => variable.FieldName).ToArray();
                    var variableValues = variables.Select(variable => (UnityEngine.Object)variable.defaultValue).ToArray();
                    scriptImporter.SetDefaultReferences(variableNames, variableValues);
                    scriptImporter.SetIcon(classAsset.icon);
                }
                else if (asset is StructAsset structAsset)
                {
                    var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(StructAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + structAsset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(structAsset.icon);
                }
                else if (asset is EnumAsset)
                {
                    var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else if (asset is DelegateAsset)
                {
                    var generator = DelegateAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else if (asset is InterfaceAsset interfaceAsset)
                {
                    var generator = InterfaceAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = interfacePath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(interfacePath + interfaceAsset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(interfaceAsset.icon);
                }
            }
            else if (targetasset is ScriptGraphAsset scriptGraphAsset)
            {
                string fullPath = csharpPath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = ScriptGraphAssetGenerator.GetSingleDecorator(scriptGraphAsset).GenerateClean(0);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpPath + GetGraphName(scriptGraphAsset) + ".cs");
                var variables = scriptGraphAsset.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var variableNames = variables.Select(variable => variable.name).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).ToArray();
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
            }
        }

        private static string GetGraphName(ScriptGraphAsset graphAsset)
        {
            if (!string.IsNullOrEmpty(graphAsset.graph.title))
            {
                return graphAsset.graph.title;
            }
            else if (!string.IsNullOrEmpty(graphAsset.name))
            {
                return graphAsset.name;
            }
            else
                return "Unnamed Graph";
        }

        private static bool IsEditorAssembly(Assembly assembly, HashSet<string> visited)
        {
            if (_editorAssemblyCache.TryGetValue(assembly, out var isEditor))
            {
                return isEditor;
            }

            var name = assembly.GetName().Name;
            if (visited.Contains(name))
            {
                return false;
            }

            visited.Add(name);

            if (IsSpecialCaseRuntimeAssembly(name))
            {
                _editorAssemblyCache.Add(assembly, false);
                return false;
            }

            if (IsVisualScriptingRuntimeAssembly(name))
            {
                _editorAssemblyCache.Add(assembly, false);
                return false;
            }

            if (Attribute.IsDefined(assembly, typeof(AssemblyIsEditorAssembly)))
            {
                _editorAssemblyCache.Add(assembly, true);
                return true;
            }

            if (IsUserAssembly(name))
            {
                _editorAssemblyCache.Add(assembly, false);
                return false;
            }

            if (IsUnityEditorAssembly(name))
            {
                _editorAssemblyCache.Add(assembly, true);
                return true;
            }

            AssemblyName[] listOfAssemblyNames = assembly.GetReferencedAssemblies();
            foreach (var dependencyName in listOfAssemblyNames)
            {
                try
                {
                    Assembly dependency = Assembly.Load(dependencyName);

                    if (IsEditorAssembly(dependency, visited))
                    {
                        _editorAssemblyCache.Add(assembly, true);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }

            _editorAssemblyCache.Add(assembly, false);

            return false;
        }

        private static bool IsSpecialCaseRuntimeAssembly(string assemblyName)
        {
            return assemblyName == "UnityEngine.UI" ||
                assemblyName == "Unity.TextMeshPro";
        }

        private static bool IsUserAssembly(string name)
        {
            return
                name == "Assembly-CSharp" ||
                name == "Assembly-CSharp-firstpass";
        }

        private static bool IsVisualScriptingRuntimeAssembly(string name)
        {
            return
                name == "Unity.VisualScripting.Flow" ||
                name == "Unity.VisualScripting.Core" || name == "Unity.VisualScripting.State";
        }

        private static bool IsUnityEditorAssembly(string name)
        {
            return
                name == "Assembly-CSharp-Editor" ||
                name == "Assembly-CSharp-Editor-firstpass" ||
                name == "UnityEditor" ||
                name == "UnityEditor.CoreModule";
        }
    }
}