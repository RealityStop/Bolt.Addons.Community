using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class InheritedMemberUnit : CodeAssetUnit
    {
        public Member member;
        public MemberType memberType;

        public override IEnumerable<object> GetAotStubs(HashSet<object> visited)
        {
            if (member != null && member.isReflected)
            {
                yield return member.info;
            }
        }
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