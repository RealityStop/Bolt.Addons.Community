using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitSurtitle("Base")]
    public class BasePropertySetterUnit : InheritedMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public BasePropertySetterUnit() { }
    
        public BasePropertySetterUnit(Member member)
        {
            this.member = member;
            memberType = MemberType.Property;
        }
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput value;
    
        protected override void Definition()
        {
            var propertyType = member.propertyInfo.PropertyType;
    
            enter = ControlInput(nameof(enter), (flow) =>
            {
                Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                return exit;
            });
            exit = ControlOutput(nameof(exit));
            Succession(enter, exit);
    
            value = ValueInput(propertyType, nameof(value));
            value.SetDefaultValue(propertyType.PseudoDefault());
        }
    }
}