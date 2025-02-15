using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEditor.Compilation;

namespace Unity.VisualScripting.Community
{
    public class ScriptGraphAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var graphAsset = (ScriptGraphAsset)asset;
            return Path.Combine(paths.ObjectsPath, GetGraphName(graphAsset).LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var graphAsset = (ScriptGraphAsset)asset;
            var code = ScriptGraphAssetGenerator.GetSingleDecorator(graphAsset).GenerateClean(0);
            return code;
        }

        protected override void PostProcess(UnityEngine.Object asset, string relativePath)
        {
            var graphAsset = (ScriptGraphAsset)asset;
            var scriptImporter = AssetImporter.GetAtPath(relativePath) as MonoImporter;
            var values = CodeGeneratorValueUtility.GetAllValues(graphAsset);
            var variables = graphAsset.graph.variables.Where(v =>
                            typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(v.typeHandle.Identification)));

            string[] names = variables.Select(v => v.name)
                            .Concat(values.Select(v => v.Key.LegalMemberName()))
                            .ToArray();

            UnityEngine.Object[] objects = variables.Select(v => (UnityEngine.Object)v.value)
                            .Concat(values.Select(v => v.Value))
                            .ToArray();

            scriptImporter.SetDefaultReferences(names, objects);
        }

        private string GetGraphName(ScriptGraphAsset graphAsset)
        {
            if (!string.IsNullOrEmpty(graphAsset.graph.title))
                return graphAsset.graph.title;
            else if (!string.IsNullOrEmpty(graphAsset.name))
                return graphAsset.name;
            return "UnnamedGraph";
        }

    }
}
