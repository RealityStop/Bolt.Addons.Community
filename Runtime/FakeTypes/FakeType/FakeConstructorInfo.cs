using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public class FakeConstructorInfo : ConstructorInfo
    {
        private readonly ConstructorDeclaration source;
        private readonly Type declaringType;

        public FakeConstructorInfo(Type declaringType, ConstructorDeclaration source)
        {
            this.declaringType = declaringType;
            this.source = source;
        }

        public override string Name => declaringType.Name;
        public override Type DeclaringType => declaringType;
        public override Type ReflectedType => declaringType;
        public override MethodAttributes Attributes => MethodAttributes.Public;
        public override RuntimeMethodHandle MethodHandle => default;

        public override ParameterInfo[] GetParameters()
        {
            return source.parameters
                .Select((p, i) => new FakeParameterInfo(source, p.name, p.type, this, i))
                .ToArray();
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            => throw new InvalidOperationException("Cannot invoke FakeConstructorInfo.");

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            => throw new InvalidOperationException("Cannot invoke FakeConstructorInfo.");

        public override MethodImplAttributes GetMethodImplementationFlags() => MethodImplAttributes.Managed;
        public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();
        public override bool IsDefined(Type attributeType, bool inherit) => false;
        public override IEnumerable<CustomAttributeData> CustomAttributes => Enumerable.Empty<CustomAttributeData>();

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new List<CustomAttributeData>();
        }

    }
}
