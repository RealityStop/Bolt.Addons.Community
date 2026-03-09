namespace Unity.VisualScripting.Community
{
    // using System;
    // using System.Collections;
    // using System.Collections.Generic;
    // using Unity.VisualScripting;
    // using UnityEngine;

    // namespace Unity.VisualScripting.Community
    // {

    //     [UnitCategory("Community/InheritedMembers/Base/Events")]
    //     [UnitSurtitle("Base")]
    //     public class BaseEventUnit : InheritedMemberUnit
    //     {
    //         [Obsolete(Serialization.ConstructorWarning)]
    //         public BaseEventUnit() { }

    //         public BaseEventUnit(Member member, Type eventType)
    //         {
    //             this.member = member;
    //             memberType = MemberType.Event;
    //             this.eventType = eventType;
    //         }

    //         [DoNotSerialize]
    //         [PortLabelHidden]
    //         public ControlInput addListener;

    //         [DoNotSerialize]
    //         [PortLabelHidden]
    //         public ControlInput removeListener;

    //         [DoNotSerialize]
    //         [PortLabelHidden]
    //         public ControlOutput exit;

    //         [DoNotSerialize]
    //         public ValueInput eventHandler;

    //         [Serialize]
    //         public Type eventType;

    //         protected override void Definition()
    //         {
    //             eventHandler = ValueInput(eventType, nameof(eventHandler));
    //             eventHandler.SetDefaultValue(eventType.PseudoDefault());

    //             addListener = ControlInput(nameof(addListener), (flow) =>
    //             {
    //                 return exit;
    //             });

    //             removeListener = ControlInput(nameof(removeListener), (flow) =>
    //             {
    //                 return exit;
    //             });

    //             exit = ControlOutput(nameof(exit));

    //             Requirement(eventHandler, addListener);
    //             Requirement(eventHandler, removeListener);
    //             Succession(addListener, exit);
    //             Succession(removeListener, exit);
    //         }
    //     }
    // }
}