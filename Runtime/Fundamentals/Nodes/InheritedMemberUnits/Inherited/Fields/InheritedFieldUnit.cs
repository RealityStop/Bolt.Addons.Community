using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitSurtitle("Inherited")]
    public class InheritedFieldUnit : InheritedMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public InheritedFieldUnit() { }
    
        public InheritedFieldUnit(Member member, ActionDirection actionDirection)
        {
            this.member = member;
            memberType = MemberType.Field;
            this.actionDirection = actionDirection;
        }
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;
    
        public ActionDirection actionDirection;
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput get;
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput value;
    
        protected override void Definition()
        {
            var fieldType = member.isField ? member.fieldInfo.FieldType : member.propertyInfo.PropertyType;
            switch (actionDirection)
            {
                case ActionDirection.Set:
                    {
                        enter = ControlInput(nameof(enter), (flow) =>
                        {
                            Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                            return exit;
                        });
                        exit = ControlOutput(nameof(exit));
                        Succession(enter, exit);
                        value = ValueInput(fieldType, nameof(value));
                        value.SetDefaultValue(fieldType.PseudoDefault());
                        break;
                    }
                case ActionDirection.Get:
                    get = ValueOutput(fieldType, nameof(get), (flow) =>
                    {
                        Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                        return fieldType.PseudoDefault();
                    });
                    break;
            }
    
        }
    }
}