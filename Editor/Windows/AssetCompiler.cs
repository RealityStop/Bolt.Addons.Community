using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

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
            var structs = HUMAssets.Find().Assets().OfType<StructAsset>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var delegates = HUMAssets.Find().Assets().OfType<DelegateAsset>();

            for (int i = 0; i < classes.Count; i++)
            {
                var fullPath = csharpPath + classes[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(ClassAssetGenerator.GetSingleDecorator(classes[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                classes[i].lastCompiledName = classes[i].category + (string.IsNullOrEmpty(classes[i].category) ? string.Empty : ".") + classes[i].title.LegalMemberName();
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
    }
}