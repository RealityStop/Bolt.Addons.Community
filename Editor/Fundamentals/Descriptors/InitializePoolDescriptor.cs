using Bolt.Addons.Community.Fundamentals;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Descriptor(typeof(InitializePoolNode))]
public class InitializePoolNodeDescriptor : UnitDescriptor<InitializePoolNode>
{
    public InitializePoolNodeDescriptor(InitializePoolNode target) : base(target)
    {
    }
    protected override EditorTexture DefinedIcon()
    {
        string iconFullPath = "Assets/Object Pooling/Icons/database-data-quality-icon.png";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
        return EditorTexture.Single(icon);
    }
    protected override string DefinedSummary()
    {
        return "Create Initial Pool";
    }
}