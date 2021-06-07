using UnityEditor;
using UnityEngine;
using Bolt.Addons.Community.Code;
using Bolt.Addons.Community.Code.Editor;
using Bolt.Addons.Libraries.Humility;
using Bolt.Addons.Libraries.CSharp;

namespace Bolt.Addons.Community.Utility.Editor
{
    public static class AssetCompiler
    {
        public static void Compile()
        {
            var path = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            HUMIO.Delete(delegatesPath);
            HUMIO.Delete(csharpPath);
            HUMIO.Delete(enumPath);

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            var csharpObjects = HUMAssets.Find().Assets().OfType<CSharpObject>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var delegates = HUMAssets.Find().Assets().OfType<DelegateAsset>();

            for (int i = 0; i < csharpObjects.Count; i++)
            {
                var fullPath = csharpPath + csharpObjects[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(CSharpObjectGenerator.GetSingleDecorator(csharpObjects[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                csharpObjects[i].lastCompiledName = csharpObjects[i].category + (string.IsNullOrEmpty(csharpObjects[i].category) ? string.Empty : ".") + csharpObjects[i].title.LegalMemberName();
            }

            for (int i = 0; i < enums.Count; i++)
            {
                var fullPath = enumPath + enums[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(enums[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                enums[i].lastCompiledName = enums[i].category + (string.IsNullOrEmpty(enums[i].category) ? string.Empty : ".") + enums[i].title.LegalMemberName();
            }

            for (int i = 0; i < delegates.Count; i++)
            {
                var generator = DelegateAssetGenerator.GetSingleDecorator(delegates[i]);
                var code = generator.GenerateClean(0);
                var fullPath = delegatesPath + delegates[i].title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";
                HUMIO.Save(code).Custom(fullPath).Text(false);
                delegates[i].lastCompiledName = delegates[i].category + (string.IsNullOrEmpty(delegates[i].category) ? string.Empty : ".") + delegates[i].title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName(); 
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}