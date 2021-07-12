using UnityEditor;

namespace Bolt.Addons.Community.Code.Editor
{
    [CustomEditor(typeof(StructAsset))]
    public sealed class StructAssetEditor : MemberTypeAssetEditor<StructAsset, StructAssetGenerator, StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
        protected override void OnExtendedOptionsGUI()
        {
            
        }
    }
}
