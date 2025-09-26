using System;
using System.IO;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;

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

        protected override string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var structAsset = (StructAsset)asset;
            return Path.Combine(paths.ObjectsRelativePath, structAsset.title.LegalMemberName() + ".cs");
        }

        public override void PostProcess(UnityEngine.Object asset, PathConfig paths, Type type)
        {
            var structAsset = (StructAsset)asset;
            var scriptImporter = (MonoImporter)MonoImporter.GetAtPath(GetRelativeFilePath(asset, paths));
#if UNITY_2023_1_OR_NEWER
            scriptImporter.SetIcon(structAsset.icon);
#endif
            if (!structAsset.lastCompiledNames.Contains(structAsset.GetFullTypeName()))
                structAsset.lastCompiledNames.Add(structAsset.GetFullTypeName());
        }
    }
}
