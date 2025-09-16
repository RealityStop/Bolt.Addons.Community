using System.IO;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;
using UnityEditor.Compilation;

namespace Unity.VisualScripting.Community
{
    public class StructAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var structAsset = (StructAsset)asset;
            return Path.Combine(paths.ObjectsPath, structAsset.title.LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var structAsset = (StructAsset)asset;
            return StructAssetGenerator.GetSingleDecorator(structAsset).GenerateClean(0);
        }

        private string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var structAsset = (StructAsset)asset;
            return Path.Combine(paths.ObjectsRelativePath, structAsset.title.LegalMemberName() + ".cs");
        }

        protected override void PostProcess(UnityEngine.Object asset, PathConfig paths)
        {
            var structAsset = (StructAsset)asset;
            var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(GetRelativeFilePath(asset, paths));
            scriptImporter.SetIcon(structAsset.icon);
            if (!structAsset.lastCompiledNames.Contains(structAsset.GetFullTypeName()))
                structAsset.lastCompiledNames.Add(structAsset.GetFullTypeName());
        }
    }
}
