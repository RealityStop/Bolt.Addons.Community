using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class CSharpPreviewSettingsRoutine : DeserializedRoutine
    {
        public override void Run()
        {
            var path = "Assets/Unity.VisualScripting.Community.Generated/";
            HUMIO.Ensure(path).Path();
            CSharpPreviewSettings settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CSharpPreviewSettings>();
                settings.name = "CSharpPreviewSettings";
                AssetDatabase.CreateAsset(settings, path + "CSharpPreviewSettings.asset");
                settings.Initalize();
                settings.VariableColor = settings.VariableColor.WithAlpha(1f);
                settings.StringColor = settings.StringColor.WithAlpha(1f);
                settings.NumericColor = settings.NumericColor.WithAlpha(1f);
                settings.ConstructColor = settings.ConstructColor.WithAlpha(1f);
                settings.TypeColor = settings.TypeColor.WithAlpha(1f);
                settings.EnumColor = settings.EnumColor.WithAlpha(1f);
                settings.InterfaceColor = settings.InterfaceColor.WithAlpha(1f);
            }
            else if (!settings.isInitalized)
            {
                settings.Initalize();
                settings.VariableColor = settings.VariableColor.WithAlpha(1f);
                settings.StringColor = settings.StringColor.WithAlpha(1f);
                settings.NumericColor = settings.NumericColor.WithAlpha(1f);
                settings.ConstructColor = settings.ConstructColor.WithAlpha(1f);
                settings.TypeColor = settings.TypeColor.WithAlpha(1f);
                settings.EnumColor = settings.EnumColor.WithAlpha(1f);
                settings.InterfaceColor = settings.InterfaceColor.WithAlpha(1f);
            }
            CSharpPreviewSettings.ShouldShowRecommendations = settings.showRecommendations;
            CSharpPreviewSettings.ShouldGenerateTooltips = settings.showTooltips;
            CSharpPreviewSettings.ShouldShowSubgraphComment = settings.showSubgraphComment;
        }
    }
}