using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;
using System.Linq;

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

            var classes = HUMAssets.Find().Assets().OfType<ClassAsset>();
            var Units = HUMAssets.Find().Assets().OfType<UnitAsset>();
            var structs = HUMAssets.Find().Assets().OfType<StructAsset>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var delegates = HUMAssets.Find().Assets().OfType<DelegateAsset>();

            for (int i = 0; i < classes.Count; i++)
            {
                var fullPath = csharpPath + classes[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                classes[i].lastCompiledName = classes[i].category + (string.IsNullOrEmpty(classes[i].category) ? string.Empty : ".") + classes[i].title.LegalMemberName();
            }

            for (int i = 0; i < Units.Count; i++)
            {
                var fullPath = csharpPath + Units[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(UnitAssetGenerator.GetSingleDecorator(Units[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                Units[i].lastCompiledName = Units[i].category + (string.IsNullOrEmpty(Units[i].category) ? string.Empty : ".") + Units[i].title.LegalMemberName();
            }

            for (int i = 0; i < structs.Count; i++)
            {
                var fullPath = csharpPath + structs[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(StructAssetGenerator.GetSingleDecorator(structs[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                structs[i].lastCompiledName = structs[i].category + (string.IsNullOrEmpty(structs[i].category) ? string.Empty : ".") + structs[i].title.LegalMemberName();
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

        public static void CompileSelected()
        {
            var selectedClasses = Selection.GetFiltered<ClassAsset>(SelectionMode.Assets).ToList();
            var selectedUnits = Selection.GetFiltered<UnitAsset>(SelectionMode.Assets).ToList();
            var selectedStructs = Selection.GetFiltered<StructAsset>(SelectionMode.Assets).ToList();
            var selectedEnums = Selection.GetFiltered<EnumAsset>(SelectionMode.Assets).ToList();
            var selectedDelegates = Selection.GetFiltered<DelegateAsset>(SelectionMode.Assets).ToList();

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

            foreach (var asset in selectedClasses)
            {
                var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
            }
            foreach (var asset in selectedUnits)
            {
                var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                HUMIO.Save(UnitAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();
            }
            foreach (var asset in selectedStructs)
            {
                var fullPath = csharpPath + asset.title.LegalMemberName() + ".cs";
                HUMIO.Save(StructAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();

            }
            foreach (var asset in selectedEnums)
            {
                var fullPath = enumPath + asset.title.LegalMemberName() + ".cs";
                HUMIO.Save(EnumAssetGenerator.GetSingleDecorator(asset).GenerateClean(0)).Custom(fullPath).Text(false);
                asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.LegalMemberName();

            }
            foreach (var asset in selectedDelegates)
            {
                var generator = DelegateAssetGenerator.GetSingleDecorator(asset);
                var code = generator.GenerateClean(0);
                var fullPath = delegatesPath + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName() + ".cs";
                HUMIO.Save(code).Custom(fullPath).Text(false);
                asset.lastCompiledName = asset.category + (string.IsNullOrEmpty(asset.category) ? string.Empty : ".") + asset.title.EnsureNonConstructName().Replace("`", string.Empty).Replace("&", string.Empty).LegalMemberName();

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}