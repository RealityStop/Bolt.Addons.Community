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
            var scriptsPath = path + "Scripts/";
            var csharpPath = scriptsPath + "Objects/";
            var delegatesPath = scriptsPath + "Delegates/";
            var actionsPath = delegatesPath + "Actions/";
            var funcsPath = delegatesPath + "Funcs/";
            var enumPath = scriptsPath + "Enums/";

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();
            HUMIO.Ensure(actionsPath).Path();
            HUMIO.Ensure(funcsPath).Path();

            HUMIO.Delete(delegatesPath);
            HUMIO.Delete(csharpPath);
            HUMIO.Delete(enumPath);
            HUMIO.Delete(actionsPath);
            HUMIO.Delete(funcsPath);

            HUMIO.Ensure(path).Path();
            HUMIO.Ensure(scriptsPath).Path();
            HUMIO.Ensure(delegatesPath).Path();
            HUMIO.Ensure(csharpPath).Path();
            HUMIO.Ensure(enumPath).Path();
            HUMIO.Ensure(actionsPath).Path();
            HUMIO.Ensure(funcsPath).Path();

            var csharpObjects = HUMAssets.Find().Assets().OfType<CSharpObject>();
            var enums = HUMAssets.Find().Assets().OfType<EnumAsset>();
            var actions = HUMAssets.Find().Assets().OfType<ActionAsset>();

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

            for (int i = 0; i < actions.Count; i++)
            {
                var fullPath = actionsPath + actions[i].title.LegalMemberName() + ".cs";
                HUMIO.Save(ActionAssetGenerator.GetSingleDecorator(actions[i]).GenerateClean(0)).Custom(fullPath).Text(false);
                actions[i].lastCompiledName = actions[i].category + (string.IsNullOrEmpty(actions[i].category) ? string.Empty : ".") + actions[i].title.LegalMemberName();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}