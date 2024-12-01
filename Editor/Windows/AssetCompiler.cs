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
            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            HUMIO.Delete(oldPath);
            HUMIO.Delete(delegatesPath);
            HUMIO.Delete(csharpPath);
            HUMIO.Delete(enumPath);

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            var classes = HUMAssets.Find().Assets().OfType<ClassAsset>().ToList();
            var structs = HUMAssets.Find().Assets().OfType<StructAsset>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var delegates = HUMAssets.Find().Assets().OfType<DelegateAsset>();

            for (int i = 0; i < classes.Count; i++)
            {
                var fullPath = csharpPath + classes[i].title.LegalMemberName() + ".cs";

                var code = ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0);


                HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                classes[i].lastCompiledName = classes[i].category + (string.IsNullOrEmpty(classes[i].category) ? string.Empty : ".") + classes[i].title.LegalMemberName();
            }

            for (int i = 0; i < structs.Count; i++)
            {
                var fullPath = csharpPath + structs[i].title.LegalMemberName() + ".cs";

                var code = ClassAssetGenerator.GetSingleDecorator(structs[i]).GenerateClean(0);

                HUMIO.Save(StructAssetGenerator.GetSingleDecorator(structs[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                structs[i].lastCompiledName = structs[i].category + (string.IsNullOrEmpty(structs[i].category) ? string.Empty : ".") + structs[i].title.LegalMemberName();
            }

            for (int i = 0; i < enums.Count; i++)
            {
                var fullPath = enumPath + enums[i].title.LegalMemberName() + ".cs";
                var code = ClassAssetGenerator.GetSingleDecorator(enums[i]).GenerateClean(0);

                HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(enums[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                enums[i].lastCompiledName = enums[i].category + (string.IsNullOrEmpty(enums[i].category) ? string.Empty : ".") + enums[i].title.LegalMemberName();
            }

            for (int i = 0; i < delegates.Count; i++)
            {
                var fullPath = delegatesPath + delegates[i].title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";
                var code = ClassAssetGenerator.GetSingleDecorator(delegates[i]).GenerateClean(0);

                HUMIO.Save(code).Custom(fullPath).Text(false);
                delegates[i].lastCompiledName = delegates[i].category + (string.IsNullOrEmpty(delegates[i].category) ? string.Empty : ".") + delegates[i].title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Addons/Compile Selected")]
        public static void CompileSelected()
        {
            var assets = Selection.GetFiltered<CodeAsset>(SelectionMode.Assets).ToList();

            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var editorPath = path + "Editor/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(editorPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            HUMIO.Delete(oldPath);

            foreach (var asset in assets)
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
                    classAsset.lastCompiledName = classAsset.category + (string.IsNullOrEmpty(classAsset.category) ? string.Empty : ".") + classAsset.title.LegalMemberName();
                }
                else if (asset is StructAsset)
                {
                    var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(StructAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is EnumAsset)
                {
                    var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is DelegateAsset)
                {
                    var generator = DelegateAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
                }
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

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(editorPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

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
                    classAsset.lastCompiledName = classAsset.category + (string.IsNullOrEmpty(classAsset.category) ? string.Empty : ".") + classAsset.title.LegalMemberName();
                }
                else if (asset is StructAsset)
                {
                    var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(StructAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is EnumAsset)
                {
                    var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is DelegateAsset)
                {
                    var generator = DelegateAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
                }
            }
            else if (targetasset is ScriptGraphAsset scriptGraphAsset)
            {
                string fullPath = csharpPath + GetGraphName(scriptGraphAsset).LegalMemberName() + ".cs";
                HUMIO.Delete(fullPath);
                HUMIO.Ensure(fullPath).Path();
                var code = ScriptGraphAssetGenerator.GetSingleDecorator(scriptGraphAsset).GenerateClean(0);
                HUMIO.Save(code).Custom(fullPath).Text(false);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetGraphName(ScriptGraphAsset graphAsset)
        {
            if(!string.IsNullOrEmpty(graphAsset.graph.title))
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