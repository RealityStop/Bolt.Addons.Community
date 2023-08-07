using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

[Descriptor(typeof(Arrow))]
public class ArrowDescriptor : UnitDescriptor<Arrow>
{
    public ArrowDescriptor(Arrow target) : base(target)
    {
    }
    protected override EditorTexture DefinedIcon()
    {
        string iconFullPath = "Assets/ArrowIcon.png";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
        return EditorTexture.Single(icon);
    }
    protected override string DefinedSummary()
    {
        return base.DefinedSummary();
    }
}