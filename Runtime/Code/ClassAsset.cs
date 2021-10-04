using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Class")]
    [RenamedFrom("Bolt.Addons.Community.Code.ClassAsset")]
    public class ClassAsset : MemberTypeAsset<ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        [Inspectable]
        public bool scriptableObject;
        [Inspectable]
        public string fileName, menuName;
    }
}
