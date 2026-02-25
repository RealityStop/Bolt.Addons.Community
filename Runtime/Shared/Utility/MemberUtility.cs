using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class MemberUtility
    {
        /// <summary>
        /// Converts the memberInfo to its manipulator returns null if not a supported member
        /// </summary>
        /// <returns></returns>
        public static Member ToManipulatorSafe(this MemberInfo memberInfo, Type targetType, bool nonPublic = false)
        {
            if (memberInfo == null) return null;

            if (memberInfo is EventInfo) return null;

            if (memberInfo is FieldInfo fieldInfo)
            {
                if (!nonPublic && !fieldInfo.IsPublic) return null;
                return fieldInfo.ToManipulator(targetType);
            }

            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetIndexParameters().Length > 0) return null;

                var getter = propertyInfo.GetGetMethod(nonPublic);
                var setter = propertyInfo.GetSetMethod(nonPublic);
                if (getter == null && setter == null) return null;

                return propertyInfo.ToManipulator(targetType);
            }

            if (memberInfo is MethodInfo methodInfo)
            {
                if (methodInfo.IsSpecialName) return null;
                if (!nonPublic && !methodInfo.IsPublic) return null;
                return methodInfo.ToManipulator(targetType);
            }

            if (memberInfo is ConstructorInfo ctorInfo)
            {
                if (!nonPublic && !ctorInfo.IsPublic) return null;
                return ctorInfo.ToManipulator(targetType);
            }

            return null;
        }

        /// <summary>
        /// Gets the type's members including ExtendedMembers.
        /// </summary>
        public static IEnumerable<Member> GetSafeExtendedMembers(this Type type, bool nonPublic = false)
        {
            if (type is FakeGenericParameterType fakeGeneric)
            {
                type = fakeGeneric.BaseType ??
                                 fakeGeneric.InterfaceConstraints.FirstOrDefault() ??
                                 typeof(object);
            }
            foreach (var memberInfo in type.GetExtendedMembers(Member.SupportedBindingFlags))
            {
                var member = memberInfo.ToManipulatorSafe(type, nonPublic);

                if (member == null)
                    continue;

                try
                {
                    member.Reflect();
                }
                catch (ArgumentNullException)
                {
                    continue;
                }
                catch (MissingMemberException)
                {
                    continue;
                }

                switch (member.info)
                {
                    case MethodInfo methodInfo:
                        if (type.IsArray &&
                            (methodInfo.Name == "Get" ||
                             methodInfo.Name == "Set" ||
                             methodInfo.Name == "Address"))
                            continue;

                        if (methodInfo.ReturnType.IsByRef) continue;
                        if (methodInfo.ContainsGenericParameters) continue;
                        break;

                    case ConstructorInfo ctorInfo:
                        if (ctorInfo.ContainsGenericParameters) continue;
                        break;
                }

                yield return member;
            }
        }

        /// <summary>
        /// Gets the type's members excluding ExtendedMembers.
        /// </summary>
        public static IEnumerable<Member> GetSafeMembers(this Type type, bool nonPublic = false)
        {
            if (type is FakeGenericParameterType fakeGeneric)
            {
                type = fakeGeneric.BaseType ??
                                 fakeGeneric.InterfaceConstraints.FirstOrDefault() ??
                                 typeof(object);
            }
            foreach (var memberInfo in type.GetMembers(Member.SupportedBindingFlags))
            {
                var member = memberInfo.ToManipulatorSafe(type, nonPublic);

                if (member == null)
                    continue;

                try
                {
                    member.Reflect();
                }
                catch (ArgumentNullException)
                {
                    continue;
                }
                catch (MissingMemberException)
                {
                    continue;
                }

                switch (member.info)
                {
                    case MethodInfo methodInfo:
                        if (type.IsArray &&
                            (methodInfo.Name == "Get" ||
                             methodInfo.Name == "Set" ||
                             methodInfo.Name == "Address"))
                            continue;

                        if (methodInfo.ReturnType.IsByRef) continue;
                        if (methodInfo.ContainsGenericParameters) continue;
                        break;

                    case ConstructorInfo ctorInfo:
                        if (ctorInfo.ContainsGenericParameters) continue;
                        break;
                }

                yield return member;
            }
        }
    }
}