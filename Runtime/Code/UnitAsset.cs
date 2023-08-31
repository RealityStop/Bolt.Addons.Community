using UnityEngine;
using System;

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
    public enum UnitType
    {
        Unit,
        EventUnit,
        VariadicNode,
        GeneratedUnit,
        MachineEventUnit,
        GlobalEventUnit,
        GameObjectEventUnit
    }
}
