using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Unit")]
    public class UnitAsset : MemberTypeAsset<UnitFieldDeclaration, UnitMethodDeclaration, UnitConstructorDeclaration>, ISerializationCallbackReceiver
    {
        [Inspectable]
        public string PreviousType = "Select Type";
        [Inspectable]
        public string UnitPreviousType = "Select Unit";

        [Inspectable]
        [SerializeField]
        public Type unitType = typeof(Unit);
        [Inspectable]
        public string fileName, menuName, Namespace;
        [Inspectable]
        [InspectorWide]
        [SerializeField]
        public Type unitArgs = typeof(EmptyEventArgs);
        public bool usingsFoldout;

        public Action<Type> onValueChanged;

        public SystemType TypeIcon = new SystemType() { type = typeof(Null) };

        public void OnAfterDeserialize()
        {
            unitArgs = Type.GetType(PreviousType);
            unitType = Type.GetType(UnitPreviousType);
        }

        public void OnBeforeSerialize()
        {
            PreviousType = unitArgs.AssemblyQualifiedName;
            UnitPreviousType = unitType.AssemblyQualifiedName;
        }
    }
}
