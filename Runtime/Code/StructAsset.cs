using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Struct")]
    [RenamedFrom("Bolt.Addons.Community.Code.StructAsset")]
    public class StructAsset : MemberTypeAsset<StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
    }
}
