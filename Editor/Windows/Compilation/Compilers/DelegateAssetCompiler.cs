using System;
using System.IO;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    public class DelegateAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var delegateAsset = (DelegateAsset)asset;
            return Path.Combine(paths.DelegatesPath,
                delegateAsset.title.EnsureNonConstructName()
                    .Replace("`", string.Empty)
                    .Replace("&", string.Empty)
                    .LegalMemberName() + ".cs");
        }

        protected override string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var delegateAsset = (DelegateAsset)asset;
            return Path.Combine(paths.DelegatesRelativePath,
                delegateAsset.title.EnsureNonConstructName()
                    .Replace("`", string.Empty)
                    .Replace("&", string.Empty)
                    .LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var delegateAsset = (DelegateAsset)asset;
            return DelegateAssetGenerator.GetSingleDecorator(delegateAsset).GenerateClean(0);
        }

        public override void PostProcess(UnityEngine.Object asset, PathConfig paths, Type type)
        {
            var delegateAsset = (DelegateAsset)asset;
            var name = delegateAsset.category +
                            (string.IsNullOrEmpty(delegateAsset.category) ? string.Empty : ".") +
                            delegateAsset.title.EnsureNonConstructName()
                                .Replace("`", string.Empty)
                                .Replace("&", string.Empty)
                                .LegalMemberName();

            if (!delegateAsset.lastCompiledNames.Contains(name))
                delegateAsset.lastCompiledNames.Add(name);
        }
    }
}
