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

        protected override void PostProcess(UnityEngine.Object asset, string relativePath)
        {
            var structAsset = (StructAsset)asset;
            var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(relativePath);
            scriptImporter.SetIcon(structAsset.icon);
        }
    }
}
