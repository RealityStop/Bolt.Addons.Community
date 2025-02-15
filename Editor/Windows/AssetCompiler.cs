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
        private static readonly Dictionary<System.Type, IAssetCompiler> compilers = new Dictionary<System.Type, IAssetCompiler>();
        private static readonly PathConfig paths;

        static AssetCompiler()
        {
            paths = new PathConfig();
            RegisterDefaultCompilers();
        }

        private static void RegisterDefaultCompilers()
        {
            RegisterCompiler<ClassAsset>(new ClassAssetCompiler());
            RegisterCompiler<StructAsset>(new StructAssetCompiler());
            RegisterCompiler<EnumAsset>(new EnumAssetCompiler());
            RegisterCompiler<DelegateAsset>(new DelegateAssetCompiler());
            RegisterCompiler<InterfaceAsset>(new InterfaceAssetCompiler());
            RegisterCompiler<ScriptGraphAsset>(new ScriptGraphAssetCompiler());
            RegisterCompiler<ScriptMachine>(new ScriptMachineCompiler());
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
            CompileGameObjects(AssetCompilierUtility.FindObjectsOfTypeIncludingInactive<ScriptMachine>());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Addons/Compile Selected")]
        public static void CompileSelected()
        {
            paths.EnsureDirectories();

            CompileAssets(Selection.GetFiltered<CodeAsset>(SelectionMode.Assets));
            CompileAssets(Selection.GetFiltered<ScriptGraphAsset>(SelectionMode.Assets));
            CompileGameObjects(Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered)
                .SelectMany(go => go.GetComponents<ScriptMachine>()));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CompileAsset(UnityEngine.Object asset)
        {
            paths.EnsureDirectories();

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

        private static void CompileGameObjects(IEnumerable<ScriptMachine> machines)
        {
            if (compilers.TryGetValue(typeof(ScriptMachine), out var compiler))
            {
                foreach (var machine in machines)
                {
                    compiler.Compile(machine, paths);
                }
            }
        }
    }
}