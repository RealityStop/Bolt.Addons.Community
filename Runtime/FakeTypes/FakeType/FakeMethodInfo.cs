using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    public class FakeMethodInfo : MethodInfo
    {
        private readonly MethodDeclaration source;
        private readonly Type declaringType;
        public FakeMethodInfo(Type declaringType, MethodDeclaration source)
        {
            this.source = source;
            this.declaringType = declaringType;
        }

        public override MemberTypes MemberType => MemberTypes.Method;
        public override MethodAttributes Attributes => MethodAttributes.Public;

        public override Type ReturnType => source.returnType;

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => null;

        public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

        public override Type DeclaringType => declaringType;

        public override string Name => source.name;

        public override Type ReflectedType => declaringType;

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return Array.Empty<object>();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return Array.Empty<object>();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            return source.parameters
                .Select((p, i) => new FakeParameterInfo(source, p.name, p.type, this, i))
                .ToArray();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        public override string ToString() => $"{ReturnType.Name} {Name}({string.Join(", ", GetParameters().Select(p => p.ParameterType.Name))})";

        public override IEnumerable<CustomAttributeData> CustomAttributes => Enumerable.Empty<CustomAttributeData>();

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new List<CustomAttributeData>();
        }

    }

    public class FakeParameterInfo : ParameterInfo
    {
#pragma warning disable
        private bool isMethodParameter;
        private readonly MethodDeclaration methodParent;
        private readonly ConstructorDeclaration constructorParent;
#pragma warning restore
        public FakeParameterInfo(MethodDeclaration method, string name, Type type, MemberInfo member, int position)
        {
            methodParent = method;
            isMethodParameter = true;
            NameImpl = name;
            ClassImpl = type;
            MemberImpl = member;
            PositionImpl = position;
        }

        public FakeParameterInfo(ConstructorDeclaration constructor, string name, Type type, MemberInfo member, int position)
        {
            constructorParent = constructor;
            isMethodParameter = false;
            NameImpl = name;
            ClassImpl = type;
            MemberImpl = member;
            PositionImpl = position;
        }

        public override IEnumerable<CustomAttributeData> CustomAttributes => Enumerable.Empty<CustomAttributeData>();

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new List<CustomAttributeData>();
        }

        public override ParameterAttributes Attributes => methodParent.parameters[Position].ToAttributes();

        public override object[] GetCustomAttributes(bool inherit)
        {
            return Array.Empty<object>();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return Array.Empty<object>();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }
    }
}