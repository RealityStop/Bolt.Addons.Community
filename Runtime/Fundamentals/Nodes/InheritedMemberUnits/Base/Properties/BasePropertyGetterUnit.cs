using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("CSharp/InheritedMembers/Base/Properties/Get")]
    [UnitSurtitle("Base")]
    public class BasePropertyGetterUnit : InheritedMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public BasePropertyGetterUnit() { }
    
        public BasePropertyGetterUnit(Member member)
        {
            this.member = member;
            memberType = MemberType.Property;
        }
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput getter;
    
        protected override void Definition()
        {
            var propertyType = member.propertyInfo.PropertyType;
    
            getter = ValueOutput(propertyType, nameof(getter), (flow) =>
            {
                Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                return propertyType.PseudoDefault();
            });
        }
    }
}