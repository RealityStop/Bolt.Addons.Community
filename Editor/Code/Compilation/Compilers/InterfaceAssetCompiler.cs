using System;
using System.IO;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    public class InterfaceAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            return Path.Combine(paths.InterfacesPath, interfaceAsset.title.LegalMemberName() + ".cs");
        }

        protected override string GetRelativeFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            return Path.Combine(paths.InterfacesRelativePath, interfaceAsset.title.LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            var generator = InterfaceAssetGenerator.GetSingleDecorator(interfaceAsset);
            return generator.GenerateClean(new CodeWriter(), generator.GetGenerationData());
        }

        public override void PostProcess(UnityEngine.Object asset, PathConfig paths, Type type)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            var name = interfaceAsset.title.LegalMemberName();

            if (!interfaceAsset.lastCompiledNames.Contains(name))
                interfaceAsset.lastCompiledNames.Add(name);
        }
    }
}