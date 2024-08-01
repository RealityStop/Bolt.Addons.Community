using Bolt.Addons.Community.Fundamentals;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [Descriptor(typeof(AssetMethodCallUnit))]
    public class AssetMethodCallUnitDescriptor : UnitDescriptor<AssetMethodCallUnit>
    {
        public AssetMethodCallUnitDescriptor(AssetMethodCallUnit target) : base(target)
        {
        }
    
        protected override string DefinedSurtitle()
        {
            return target.method.classAsset.title;
        }
    
        protected override EditorTexture DefinedIcon()
        {
            return target.method.returnType.Icon();
        }
    
        protected override string DefinedTitle()
        {
            return target.method.classAsset.title + "." +target.method.methodName;
        }
    
        protected override string DefinedShortTitle()
        {
            return target.method.methodName;
        }
    }
}