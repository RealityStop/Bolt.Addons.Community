using UnityEditor;
using UnityEngine;
using Bolt.Addons.Community.Code;
using Bolt.Addons.Community.Code.Editor;
using Bolt.Addons.Integrations.Continuum.Humility;
using Bolt.Addons.Integrations.Continuum.CSharp;

namespace Bolt.Addons.Community.Utility.Editor
{
    public static class AssetCompiler
    {
        public static void Compile()
        {
            var path = Application.dataPath + "/Bolt.Addons.Generated/";
            var csharpPath = Application.dataPath + "/Bolt.Addons.Generated/CSharp Objects/";
            var enumPath = Application.dataPath + "/Bolt.Addons.Generated/Enums/";

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            var csharpObjects = HUMAssets.Find().Assets().OfType<CSharpObject>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();

            for (int i = 0; i < csharpObjects.Count; i++)
            {
                var fullPath = csharpPath + csharpObjects[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(CSharpObjectGenerator.GetSingleDecorator(csharpObjects[i]).GenerateClean(0)).Custom(fullPath).Text(false);
            }

            for (int i = 0; i < enums.Count; i++)
            {
                var fullPath = enumPath + enums[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(enums[i]).GenerateClean(0)).Custom(fullPath).Text(false);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}