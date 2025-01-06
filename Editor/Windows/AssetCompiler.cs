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
using UnityEditor.Compilation;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    public static class AssetCompiler
    {
        private static readonly Dictionary<System.Reflection.Assembly, bool> _editorAssemblyCache = new Dictionary<System.Reflection.Assembly, bool>();

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
            var basePath = "Assets" + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var scriptsRelativePath = basePath + "Scripts/";
            var editorPath = path + "Editor/";
            var editorRelativePath = basePath + "Editor/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";
            var interfacePath = scriptsPath + "Interfaces/";
            var csharpRelativePath = scriptsRelativePath + "Objects/";
            var interfaceRelativePath = scriptsRelativePath + "Interfaces/";
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
            var scriptMachines = FindObjectsOfTypeIncludingInactive<ScriptMachine>().Concat(HUMAssets.Find().Assets().OfType<GameObject>().SelectMany(obj => obj.GetComponents<ScriptMachine>())).ToList();

            for (int i = 0; i < classes.Count; i++)
            {
                var classAsset = classes[i];
                string fullPath;
                string relativePath;
                if (classAsset.inheritsType && IsEditorAssembly(classAsset.GetInheritedType().Assembly, new HashSet<string>()))
                {
                    fullPath = editorPath + classAsset.title.LegalMemberName() + ".cs";
                    relativePath = editorRelativePath + classAsset.title.LegalMemberName() + ".cs";
                }
                else
                {
                    fullPath = csharpPath + classAsset.title.LegalMemberName() + ".cs";
                    relativePath = csharpRelativePath + classAsset.title.LegalMemberName() + ".cs";
                }
                var code = ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0);
                var ObjectVariables = "\n";
                var values = CodeGeneratorValueUtility.GetAllValues(classAsset);
                foreach (var variable in values)
                {
                    if (variable.Value != null)
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                    else
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                }
                code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                var name = classAsset.category + (string.IsNullOrEmpty(classAsset.category) ? string.Empty : ".") + classAsset.title.LegalMemberName();
                if (!classAsset.lastCompiledNames.Contains(name))
                    classAsset.lastCompiledNames.Add(name);
                var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                var variables = classAsset.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(variable.type) && (variable.scope == AccessModifier.Public || variable.attributes.Any(a => a.GetAttributeType() == typeof(SerializeField))));
                var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                var variableNames = variables.Select(variable => variable.FieldName).Concat(ObjectNamesArray).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.defaultValue).Concat(ObjectValuesArray).ToArray();
                scriptImporter.SetIcon(classAsset.icon);
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
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
                var interfaceAsset = interfaces[i];
                var fullPath = interfacePath + interfaceAsset.title.LegalMemberName() + ".cs";
                HUMIO.Save(InterfaceAssetGenerator.GetSingleDecorator(interfaceAsset).GenerateClean(0)).Custom(fullPath).Text(false);
                var name = interfaceAsset.category + (string.IsNullOrEmpty(interfaceAsset.category) ? string.Empty : ".") + interfaceAsset.title.LegalMemberName();
                if (!interfaceAsset.lastCompiledNames.Contains(name))
                    interfaceAsset.lastCompiledNames.Add(name);
                var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(interfaceRelativePath + interfaceAsset.title.LegalMemberName() + ".cs");
                scriptImporter.SetIcon(interfaceAsset.icon);
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

            var attemptSubgraphs = !scriptGraphAssets.Any(asset => asset.graph.units.Any(unit => unit is GraphInput or GraphOutput)) || EditorUtility.DisplayDialog("Compile Subgraphs", "Attempt to compile subgraphs, this could cause errors in the generated script and will not generate anything connected to the Graph Input and Output.", "Yes", "No");

            for (int i = 0; i < scriptGraphAssets.Count; i++)
            {
                var scriptGraphAsset = scriptGraphAssets[i];
                if (scriptGraphAsset.graph != null || (scriptGraphAsset.graph.units.Any(unit => unit is GraphInput or GraphOutput) && !attemptSubgraphs)) continue;
                string fullPath = csharpPath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                string relativePath = csharpRelativePath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = ScriptGraphAssetGenerator.GetSingleDecorator(scriptGraphAsset).GenerateClean(0);
                var ObjectVariables = "\n";
                var values = CodeGeneratorValueUtility.GetAllValues(scriptGraphAsset);
                foreach (var variable in values)
                {
                    if (variable.Value != null)
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                    else
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                }
                code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                var variables = scriptGraphAsset.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                var variableNames = variables.Select(variable => variable.name).Concat(ObjectNamesArray).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).Concat(ObjectValuesArray).ToArray();
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
            }

            for (int i = 0; i < scriptMachines.Count; i++)
            {
                var scriptMachine = scriptMachines[i];
                if (scriptMachine.graph != null || (scriptMachine.graph.units.Any(unit => unit is GraphInput or GraphOutput) && !attemptSubgraphs)) continue;
                string fullPath = csharpPath + GetGraphName(scriptMachine).LegalMemberName() + ".cs";
                string relativePath = csharpRelativePath + GetGraphName(scriptMachine).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = GameObjectGenerator.GetSingleDecorator(scriptMachine.gameObject).GenerateClean(0);
                var ObjectVariables = "\n";
                var values = CodeGeneratorValueUtility.GetAllValues(scriptMachine);
                foreach (var variable in values)
                {
                    if (variable.Value != null)
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                    else
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                }
                code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
                var type = script.GetClass();
                var component = scriptMachine.gameObject.AddComponent(type);
                var variables = scriptMachine.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                var variableNames = variables.Select(variable => variable.name).Concat(ObjectNamesArray).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).Concat(ObjectValuesArray).ToArray();
                for (int index = 0; index < variableValues.Length; index++)
                {
                    type.GetFields().Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).FirstOrDefault(f => f.Name == variableNames[index])?.SetValue(component, variableValues[index]);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static IEnumerable<T> FindObjectsOfTypeIncludingInactive<T>()
        {
            var scene = SceneManager.GetActiveScene();

            if (scene.isLoaded)
            {
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (var result in rootGameObject.GetComponents<T>())
                    {
                        yield return result;
                    }
                    foreach (var result in rootGameObject.GetComponentsInChildren<T>(true))
                    {
                        yield return result;
                    }
                }
            }
        }

        [MenuItem("Addons/Compile Selected")]
        public static void CompileSelected()
        {
            var codeAssets = Selection.GetFiltered<CodeAsset>(SelectionMode.Assets).ToList();
            var graphAssets = Selection.GetFiltered<ScriptGraphAsset>(SelectionMode.Assets).ToList();
            var gameObjects = Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered).ToList();

            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var basePath = "Assets" + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var scriptsRelativePath = basePath + "Scripts/";
            var editorPath = path + "Editor/";
            var editorRelativePath = basePath + "Editor/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";
            var interfacePath = scriptsPath + "Interfaces/";
            var csharpRelativePath = scriptsRelativePath + "Objects/";
            var interfaceRelativePath = scriptsRelativePath + "Interfaces/";


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
                    string fullPath;
                    string relativePath;
                    if (classAsset.inheritsType && IsEditorAssembly(classAsset.GetInheritedType().Assembly, new HashSet<string>()))
                    {
                        fullPath = editorPath + classAsset.title.LegalMemberName() + ".cs";
                        relativePath = editorRelativePath + classAsset.title.LegalMemberName() + ".cs";
                    }
                    else
                    {
                        fullPath = csharpPath + classAsset.title.LegalMemberName() + ".cs";
                        relativePath = csharpRelativePath + classAsset.title.LegalMemberName() + ".cs";
                    }

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    var code = ClassAssetGenerator.GetSingleDecorator(classAsset).GenerateClean(0);
                    var ObjectVariables = "\n";
                    var values = CodeGeneratorValueUtility.GetAllValues(classAsset);
                    foreach (var variable in values)
                    {
                        if (variable.Value != null)
                            ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                        else
                            ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                    }
                    code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    HUMIO.Save(code).Custom(fullPath).Text(true);
                    var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                    var variables = classAsset.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(variable.type) && (variable.scope == AccessModifier.Public || variable.attributes.Any(a => a.GetAttributeType() == typeof(SerializeField))));
                    var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                    var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                    var variableNames = variables.Select(variable => variable.FieldName).Concat(ObjectNamesArray).ToArray();
                    var variableValues = variables.Select(variable => (UnityEngine.Object)variable.defaultValue).Concat(ObjectValuesArray).ToArray();
                    scriptImporter.SetIcon(classAsset.icon);
                    scriptImporter.SetDefaultReferences(variableNames, variableValues);

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
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpRelativePath + asset.title.LegalMemberName() + ".cs");
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
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(interfaceRelativePath + interfaceAsset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(interfaceAsset.icon);
                }
            }

            foreach (var scriptGraphAsset in graphAssets)
            {
                string fullPath = csharpPath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                string relativePath = csharpRelativePath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = ScriptGraphAssetGenerator.GetSingleDecorator(scriptGraphAsset).GenerateClean(0);
                var ObjectVariables = "\n";
                var values = CodeGeneratorValueUtility.GetAllValues(scriptGraphAsset);
                foreach (var variable in values)
                {
                    if (variable.Value != null)
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                    else
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                }
                code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                var variables = scriptGraphAsset.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                var variableNames = variables.Select(variable => variable.name).Concat(ObjectNamesArray).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).Concat(ObjectValuesArray).ToArray();
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
            }

            foreach (var scriptMachine in gameObjects.SelectMany(obj => obj.GetComponents<ScriptMachine>()))
            {
                string fullPath = csharpPath + GetGraphName(scriptMachine).LegalMemberName() + ".cs";
                string relativePath = csharpRelativePath + GetGraphName(scriptMachine).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = GameObjectGenerator.GetSingleDecorator(scriptMachine.gameObject).GenerateClean(0);
                var ObjectVariables = "\n";
                var values = CodeGeneratorValueUtility.GetAllValues(scriptMachine);
                foreach (var variable in values)
                {
                    if (variable.Value != null)
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                    else
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                }
                code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                HUMIO.Save(code).Custom(fullPath).Text(false);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
                var type = script.GetClass();
                var component = scriptMachine.gameObject.AddComponent(type);
                var variables = scriptMachine.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                var variableNames = variables.Select(variable => variable.name).Concat(ObjectNamesArray).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).Concat(ObjectValuesArray).ToArray();
                for (int index = 0; index < variableValues.Length; index++)
                {
                    type.GetFields().Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).FirstOrDefault(f => f.Name == variableNames[index])?.SetValue(component, variableValues[index]);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public static MonoImporter monoImporter;
        public static void CompileAsset(UnityEngine.Object targetasset)
        {
            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var basePath = "Assets" + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var scriptsRelativePath = basePath + "Scripts/";
            var editorPath = path + "Editor/";
            var editorRelativePath = basePath + "Editor/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";
            var interfacePath = scriptsPath + "Interfaces/";
            var csharpRelativePath = scriptsRelativePath + "Objects/";
            var interfaceRelativePath = scriptsRelativePath + "Interfaces/";

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
                    string relativePath;
                    if (classAsset.inheritsType && IsEditorAssembly(classAsset.GetInheritedType().Assembly, new HashSet<string>()))
                    {
                        fullPath = editorPath + classAsset.title.LegalMemberName() + ".cs";
                        relativePath = editorRelativePath + classAsset.title.LegalMemberName() + ".cs";
                    }
                    else
                    {
                        fullPath = csharpPath + classAsset.title.LegalMemberName() + ".cs";
                        relativePath = csharpRelativePath + classAsset.title.LegalMemberName() + ".cs";
                    }

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    var code = ClassAssetGenerator.GetSingleDecorator(classAsset).GenerateClean(0);
                    var ObjectVariables = "\n";
                    var values = CodeGeneratorValueUtility.GetAllValues(classAsset);
                    foreach (var variable in values)
                    {
                        if (variable.Value != null)
                            ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                        else
                            ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                    }
                    code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                    var name = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                    if (!asset.lastCompiledNames.Contains(name))
                        asset.lastCompiledNames.Add(name);
                    HUMIO.Save(code).Custom(fullPath).Text(true);
                    var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                    var variables = classAsset.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(variable.type) && (variable.scope == AccessModifier.Public || variable.attributes.Any(a => a.GetAttributeType() == typeof(SerializeField))));
                    var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                    var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                    var variableNames = variables.Select(variable => variable.FieldName).Concat(ObjectNamesArray).ToArray();
                    var variableValues = variables.Select(variable => (UnityEngine.Object)variable.defaultValue).Concat(ObjectValuesArray).ToArray();
                    scriptImporter.SetIcon(classAsset.icon);
                    scriptImporter.SetDefaultReferences(variableNames, variableValues);

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
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(csharpRelativePath + structAsset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(structAsset.icon);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
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
                    var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(interfaceRelativePath + interfaceAsset.title.LegalMemberName() + ".cs");
                    scriptImporter.SetIcon(interfaceAsset.icon);
                }
            }
            else if (targetasset is ScriptGraphAsset scriptGraphAsset)
            {
                string fullPath = csharpPath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                string relativePath = csharpRelativePath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = ScriptGraphAssetGenerator.GetSingleDecorator(scriptGraphAsset).GenerateClean(0);
                var ObjectVariables = "\n";
                var values = CodeGeneratorValueUtility.GetAllValues(scriptGraphAsset);
                foreach (var variable in values)
                {
                    if (variable.Value != null)
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                    else
                        ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                }
                code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                HUMIO.Save(code).Custom(fullPath).Text(true);
                var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                var variables = scriptGraphAsset.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                var variableNames = variables.Select(variable => variable.name).Concat(ObjectNamesArray).ToArray();
                var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).Concat(ObjectValuesArray).ToArray();
                scriptImporter.SetDefaultReferences(variableNames, variableValues);
            }
            else if (targetasset is GameObject gameObject)
            {
                index = 0;
                @object = gameObject;
                if (EditorUtility.DisplayDialog("Compile All Machines", "Compile all script machines attached to this GameObject?.", "Yes", "No"))
                {
                    var gen = (GameObjectGenerator)GameObjectGenerator.GetSingleDecorator(gameObject);
                    foreach (var scriptMachine in gameObject.GetComponents<ScriptMachine>())
                    {
                        var name = GetGraphName(scriptMachine);
                        string fullPath = csharpPath + name.LegalMemberName() + ".cs";
                        string relativePath = csharpRelativePath + name.LegalMemberName() + ".cs";
                        HUMIO.Delete(fullPath);
                        HUMIO.Ensure(fullPath).Path();
                        gen.current = scriptMachine;
                        var code = gen.GenerateClean(0);
                        var ObjectVariables = "\n";
                        var values = CodeGeneratorValueUtility.GetAllValues(scriptMachine);
                        foreach (var variable in values)
                        {
                            if (variable.Value != null)
                                ObjectVariables += CodeBuilder.Indent(1) + "public " + variable.Value.GetType().As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";
                            else
                                ObjectVariables += CodeBuilder.Indent(1) + "public " + typeof(UnityEngine.Object).As().CSharpName(false, true, false) + " " + variable.Key.LegalMemberName() + ";\n";

                        }
                        code = code.Insert(code.IndexOf("{") + 1, ObjectVariables);
                        HUMIO.Save(code).Custom(fullPath).Text(true);

                        var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
                        var type = scriptImporter.GetScript().GetClass();
                        var component = scriptMachine.gameObject.GetComponent(type) ?? scriptMachine.gameObject.AddComponent(type);
                        var variables = scriptMachine.graph.variables.Where(variable => typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(variable.typeHandle.Identification)));
                        var ObjectNamesArray = values.Select(v => v.Key.LegalMemberName()).ToArray();
                        var ObjectValuesArray = values.Select(v => v.Value).ToArray();
                        var variableNames = variables.Select(variable => variable.name).Concat(ObjectNamesArray).ToArray();
                        var variableValues = variables.Select(variable => (UnityEngine.Object)variable.value).Concat(ObjectValuesArray).ToArray();
                        for (int index = 0; index < variableValues.Length; index++)
                        {
                            type.GetFields().Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).FirstOrDefault(f => f.Name == variableNames[index])?.SetValue(component, variableValues[index]);
                        }
                    }
                }
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
                return "UnnamedGraph";
        }
        static int index = 0;
        static GameObject @object;
        private static string GetGraphName(ScriptMachine machine)
        {
            if (@object != machine.gameObject)
            {
                index = 0;
                @object = machine.gameObject;
            }
            return machine.nest.graph.title?.Length > 0 ? machine.nest.graph.title : machine.gameObject.name + "_ScriptMachine_" + index++;
        }

        private static bool IsEditorAssembly(System.Reflection.Assembly assembly, HashSet<string> visited)
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
                    System.Reflection.Assembly dependency = System.Reflection.Assembly.Load(dependencyName);

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

        [InitializeOnLoad]
        private static class ScriptMachineFetcher
        {
            static ScriptMachineFetcher()
            {
                CodeGeneratorValueUtility.requestMachine += (obj) =>
                {
                    var gen = GameObjectGenerator.GetSingleDecorator(obj) as GameObjectGenerator;
                    if (gen.current == null)
                    {
                        var component = obj.GetComponent<ScriptMachine>();
                        gen.current = component;
                        return component;
                    }
                    return gen.current;
                };
            }
        }
    }
}