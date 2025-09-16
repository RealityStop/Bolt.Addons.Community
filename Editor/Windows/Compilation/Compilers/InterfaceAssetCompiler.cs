using System.IO;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor.Compilation;

namespace Unity.VisualScripting.Community
{
    public class InterfaceAssetCompiler : BaseCompiler
    {
        protected override string GetFilePath(UnityEngine.Object asset, PathConfig paths)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            return Path.Combine(paths.InterfacesPath, interfaceAsset.title.LegalMemberName() + ".cs");
        }

        protected override string GenerateCode(UnityEngine.Object asset)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            return InterfaceAssetGenerator.GetSingleDecorator(interfaceAsset).GenerateClean(0);
        }

        protected override void PostProcess(UnityEngine.Object asset, PathConfig paths)
        {
            var interfaceAsset = (InterfaceAsset)asset;
            var name = interfaceAsset.title.LegalMemberName();

            if (!interfaceAsset.lastCompiledNames.Contains(name))
                interfaceAsset.lastCompiledNames.Add(name);
        }
    }
}