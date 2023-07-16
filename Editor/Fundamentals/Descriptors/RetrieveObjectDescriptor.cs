using Bolt.Addons.Community.Fundamentals;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Descriptor(typeof(RetrieveObjectNode))]
public class RetrieveObjectNodeDescriptor : UnitDescriptor<RetrieveObjectNode>
{
    public RetrieveObjectNodeDescriptor(RetrieveObjectNode target) : base(target)
    {
    }
    protected override EditorTexture DefinedIcon()
    {
        string iconFullPath = "Packages/dev.bolt.addons/Runtime/Fundamentals/Nodes/Object Pooling/Icons/database-file-icon.png";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
        return EditorTexture.Single(icon);
    }
    protected override string DefinedSummary()
    {
        return "Get Object From Pool";
    }
}
