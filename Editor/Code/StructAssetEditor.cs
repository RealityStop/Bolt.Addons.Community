using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(StructAsset))]
    public sealed class StructAssetEditor : MemberTypeAssetEditor<StructAsset, StructAssetGenerator, StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
        protected override void OnExtendedOptionsGUI()
        {
            
        }
    }
}
