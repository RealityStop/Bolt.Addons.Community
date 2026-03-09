using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;

namespace Unity.VisualScripting.Community.CSharp
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
            var generator = ScriptGraphAssetGenerator.GetSingleDecorator(graphAsset);
            var code = generator.GenerateClean(new CodeWriter(), generator.GetGenerationData());
            return code;
        }

        protected override string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var graphAsset = (ScriptGraphAsset)asset;
            return Path.Combine(paths.ObjectsRelativePath, GetGraphName(graphAsset).LegalMemberName() + ".cs");
        }

        public override void PostProcess(UnityEngine.Object asset, PathConfig paths, Type type)
        {
            var graphAsset = (ScriptGraphAsset)asset;
            var scriptImporter = AssetImporter.GetAtPath(GetRelativeFilePath(asset, paths)) as MonoImporter;
            var values = CodeGeneratorValueUtility.GetAllValues(graphAsset);
#if VISUAL_SCRIPTING_1_7
            var variables = graphAsset.graph.variables.Where(v =>
            {
                var type = Type.GetType(v.typeHandle.Identification);
                return type != null && typeof(UnityEngine.Object).IsAssignableFrom(type);
            });
#else
            var variables = graphAsset.graph.variables.Where(v =>
                typeof(UnityEngine.Object).IsAssignableFrom(v.value?.GetType()));
#endif
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
