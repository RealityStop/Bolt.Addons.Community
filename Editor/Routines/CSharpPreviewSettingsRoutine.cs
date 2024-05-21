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
            }

            CSharpPreview.NumericColor = settings.NumericColor;
            CSharpPreview.EnumColor = settings.EnumColor;
            CSharpPreview.ConstructColor = settings.ConstructColor;
            CSharpPreview.VariableColor = settings.VariableColor;
            CSharpPreview.StringColor = settings.StringColor;
            CSharpPreview.InterfaceColor = settings.InterfaceColor;
            CSharpPreview.TypeColor = settings.TypeColor;
        }
    }
}