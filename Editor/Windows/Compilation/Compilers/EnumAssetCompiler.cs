using System.IO;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor.Compilation;

namespace Unity.VisualScripting.Community
{
    public class EnumAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var enumAsset = (EnumAsset)asset;
            return Path.Combine(paths.EnumsPath, enumAsset.title.LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var enumAsset = (EnumAsset)asset;
            return EnumAssetGenerator.GetSingleDecorator(enumAsset).GenerateClean(0);
        }

        protected override void PostProcess(UnityEngine.Object asset, string relativePath)
        {
            var enumAsset = (EnumAsset)asset;
            var name = enumAsset.category + (string.IsNullOrEmpty(enumAsset.category) ? string.Empty : ".") + enumAsset.title.LegalMemberName();
            if (!enumAsset.lastCompiledNames.Contains(name))
                enumAsset.lastCompiledNames.Add(name);
        }
    }
}
