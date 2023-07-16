using Bolt.Addons.Community.Fundamentals;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Descriptor(typeof(ReturnAllObjectsToPoolNode))]
public class ReturnAllObjectsToPoolNodeDescriptor : UnitDescriptor<ReturnAllObjectsToPoolNode>
{
    public ReturnAllObjectsToPoolNodeDescriptor(ReturnAllObjectsToPoolNode target) : base(target)
    {
    }
    protected override EditorTexture DefinedIcon()
    {
        string iconFullPath = "Packages/dev.bolt.addons/Runtime/Fundamentals/Nodes/Object Pooling/Icons/data-update-icon.png";
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
        return EditorTexture.Single(icon);
    }
    protected override string DefinedSummary()
    {
        return "Return All Object To The Pool";
    }
}
