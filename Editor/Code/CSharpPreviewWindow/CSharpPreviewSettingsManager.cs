using System;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class CSharpPreviewSettingsManager
    {
        private CSharpPreviewSettings settings;
        bool isInitalized;
        public void InitializeSettings()
        {
            if (isInitalized) return;

            isInitalized = true;

            const string path = "Assets/Unity.VisualScripting.Community.Generated/";
            HUMIO.Ensure(path).Path();
            CSharpPreviewSettings settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CSharpPreviewSettings>();
                settings.name = "CSharpPreviewSettings";
                AssetDatabase.CreateAsset(settings, path + "CSharpPreviewSettings.asset");
                settings.Initalize();
            }
            else if (!settings.isInitalized)
            {
                settings.Initalize();
            }

            CSharpPreviewSettings.ShouldShowRecommendations = settings.showRecommendations;
            CSharpPreviewSettings.ShouldGenerateTooltips = settings.showTooltips;
            CSharpPreviewSettings.ShouldShowSubgraphComment = settings.showSubgraphComment;
            CSharpPreviewSettings.RecursionDepth = settings.recursionDepth;

            this.settings = settings;
        }

        public void SaveSettings()
        {
            settings.SaveAndDirty();
        }

        public void UpdateSettings(Action<CSharpPreviewSettings> action)
        {
            action?.Invoke(settings);
        }
    }
}