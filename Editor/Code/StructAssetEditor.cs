using UnityEditor;

namespace Unity.VisualScripting.Community.CSharp
{
    [CustomEditor(typeof(StructAsset))]
    public sealed class StructAssetEditor : MemberTypeAssetEditor<StructAsset, StructAssetGenerator, StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
        protected override void OnExtendedOptionsGUI()
        {
            
        }
    }
}
