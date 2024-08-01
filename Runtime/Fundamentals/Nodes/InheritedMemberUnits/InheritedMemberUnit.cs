using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    public abstract class InheritedMemberUnit : Unit
    {
        public Member member;
        public MemberType memberType;
    }
    
    public enum MethodType
    {
        Invoke,
        ReturnValue
    }
    
    public enum MemberType
    {
        Property,
        Field,
        Method,
    }
}