using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Bolt.Addons.Community.Code
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Class")]
    public class ClassAsset : MemberTypeAsset<ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        [Inspectable]
        public bool scriptableObject;
        [Inspectable]
        public string fileName, menuName;
    }
}
