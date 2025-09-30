using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community
{
    public static class AssetCompiler
    {
        private static readonly Dictionary<System.Type, IAssetCompiler> compilers = new Dictionary<System.Type, IAssetCompiler>();
        private static readonly PathConfig paths;

        static AssetCompiler()
        {
            paths = new PathConfig();
            RegisterDefaultCompilers();
        }

        public static ReadOnlyDictionary<Type, IAssetCompiler> GetCompiliers()
        {
            return new ReadOnlyDictionary<Type, IAssetCompiler>(compilers);
        }

        private static void RegisterDefaultCompilers()
        {
            RegisterCompiler<ClassAsset>(new ClassAssetCompiler());
            RegisterCompiler<StructAsset>(new StructAssetCompiler());
            RegisterCompiler<EnumAsset>(new EnumAssetCompiler());
            RegisterCompiler<DelegateAsset>(new DelegateAssetCompiler());
            RegisterCompiler<InterfaceAsset>(new InterfaceAssetCompiler());
            RegisterCompiler<ScriptGraphAsset>(new ScriptGraphAssetCompiler());
            RegisterCompiler<SMachine>(new ScriptMachineCompiler());
        }

        public static void RegisterCompiler<T>(IAssetCompiler compiler) where T : UnityEngine.Object
        {
            compilers[typeof(T)] = compiler;
        }

        public const string GeneratedPath = "Unity.VisualScripting.Community.Generated";
        
        [MenuItem("Addons/Compile All")]
        public static void Compile()
        {
            if (!EditorUtility.DisplayDialog("Compile All Assets",
                $"Compile all assets? This will clear all generated scripts in {GeneratedPath}.", "Yes", "No"))
                return;

            paths.EnsureDirectories();
            paths.ClearGeneratedFiles();

            CompileAssets(HUMAssets.Find().Assets().OfType<CodeAsset>());
            CompileAssets(HUMAssets.Find().Assets().OfType<ScriptGraphAsset>());
            CompileScriptMachines(AssetCompilierUtility.FindObjectsOfTypeIncludingInactive<SMachine>());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Addons/Compile Selected")]
        public static void CompileSelected()
        {
            paths.EnsureDirectories();

            foreach (var item in Selection.GetFiltered<CodeAsset>(SelectionMode.Assets))
            {
                CompileAsset(item);
            }
            CompileAssets(Selection.GetFiltered<ScriptGraphAsset>(SelectionMode.Assets));
            CompileScriptMachines(Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered)
                .SelectMany(go => go.GetComponents<SMachine>()));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CompileAsset(UnityEngine.Object asset)
        {
            paths.EnsureDirectories();
            if (asset is GameObject @object)
            {
                SMachine scriptMachine = null;
                if (CodeGenerator.GetSingleDecorator(@object) is GameObjectGenerator gen && gen.current != null)
                    scriptMachine = gen.current;
                if (scriptMachine == null)
                    scriptMachine = @object.GetComponent<SMachine>();
                if (scriptMachine != null)
                    CompileScriptMachines(new List<SMachine>() { scriptMachine });
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return;
            }
            var type = asset.GetType();
            if (compilers.TryGetValue(type, out var compiler))
            {
                compiler.Compile(asset, paths);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CompileAssets<T>(IEnumerable<T> assets) where T : UnityEngine.Object
        {
            if (compilers.TryGetValue(typeof(T), out var compiler))
            {
                foreach (var asset in assets)
                {
                    compiler.Compile(asset, paths);
                }
            }
        }

        private static void CompileScriptMachines(IEnumerable<SMachine> machines)
        {
            if (compilers.TryGetValue(typeof(SMachine), out var compiler))
            {
                foreach (var machine in machines)
                {
                    compiler.Compile(machine, paths);
                }
            }
        }
    }
}