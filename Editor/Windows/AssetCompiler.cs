using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Unity.VisualScripting.Community
{
    public static class AssetCompiler
    {
        public static void Compile()
        {
            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            HUMIO.Delete(oldPath);
            HUMIO.Delete(delegatesPath);
            HUMIO.Delete(csharpPath);
            HUMIO.Delete(enumPath);

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            var classes = HUMAssets.Find().Assets().OfType<ClassAsset>().ToList();
            var structs = HUMAssets.Find().Assets().OfType<StructAsset>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var delegates = HUMAssets.Find().Assets().OfType<DelegateAsset>();

            for (int i = 0; i < classes.Count; i++)
            {
                var fullPath = csharpPath + classes[i].title.LegalMemberName() + ".cs";

                var code = ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0);


                HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                classes[i].lastCompiledName = classes[i].category + (string.IsNullOrEmpty(classes[i].category) ? string.Empty : ".") + classes[i].title.LegalMemberName();
            }

            for (int i = 0; i < structs.Count; i++)
            {
                var fullPath = csharpPath + structs[i].title.LegalMemberName() + ".cs";

                var code = ClassAssetGenerator.GetSingleDecorator(structs[i]).GenerateClean(0);

                HUMIO.Save(StructAssetGenerator.GetSingleDecorator(structs[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                structs[i].lastCompiledName = structs[i].category + (string.IsNullOrEmpty(structs[i].category) ? string.Empty : ".") + structs[i].title.LegalMemberName();
            }

            for (int i = 0; i < enums.Count; i++)
            {
                var fullPath = enumPath + enums[i].title.LegalMemberName() + ".cs";
                var code = ClassAssetGenerator.GetSingleDecorator(enums[i]).GenerateClean(0);

                HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(enums[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                enums[i].lastCompiledName = enums[i].category + (string.IsNullOrEmpty(enums[i].category) ? string.Empty : ".") + enums[i].title.LegalMemberName();
            }

            for (int i = 0; i < delegates.Count; i++)
            {
                var fullPath = delegatesPath + delegates[i].title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";
                var code = ClassAssetGenerator.GetSingleDecorator(delegates[i]).GenerateClean(0);

                HUMIO.Save(code).Custom(fullPath).Text(false);
                delegates[i].lastCompiledName = delegates[i].category + (string.IsNullOrEmpty(delegates[i].category) ? string.Empty : ".") + delegates[i].title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CompileSelected()
        {
            var assets = Selection.GetFiltered<CodeAsset>(SelectionMode.Assets).ToList();

            var path = Application.dataPath + "/Unity.VisualScripting.Community.Generated/";
            var oldPath = Application.dataPath + "/Bolt.Addons.Generated/";
            var scriptsPath = path + "Scripts/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var enumPath = scriptsPath + "Enums/";

            HUMIO.Ensure(oldPath).Path();
            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();

            HUMIO.Delete(oldPath);

            foreach (var asset in assets)
            {
                if (asset is ClassAsset)
                {
                    var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is StructAsset)
                {
                    var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(StructAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is EnumAsset)
                {
                    var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
                }
                else if (asset is DelegateAsset)
                {
                    var generator = DelegateAssetGenerator.GetSingleDecorator(asset);
                    var code = generator.GenerateClean(0);
                    var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";

                    HUMIO.Delete(fullPath);
                    HUMIO.Ensure(fullPath).Path();
                    HUMIO.Save(code).Custom(fullPath).Text(false);
                    asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}