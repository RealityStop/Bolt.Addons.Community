using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public class FakeFieldInfo : FieldInfo
    {
        private readonly FieldDeclaration source;
        private readonly Type declaringType;

        public FakeFieldInfo(Type declaringType, FieldDeclaration source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.declaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
        }

        public override MemberTypes MemberType => MemberTypes.Field;

        public override Type FieldType => source.type;

        public override FieldAttributes Attributes => FieldAttributes.Public;

        public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException("FakeFieldInfo does not have a runtime handle.");

        public override Type DeclaringType => declaringType;

        public override string Name => source.name;

        public override Type ReflectedType => declaringType;

        public override object GetValue(object obj) => source.value;

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            source.value = value;
        }

        public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();

        public override bool IsDefined(Type attributeType, bool inherit) => false;
        public override IEnumerable<CustomAttributeData> CustomAttributes => Enumerable.Empty<CustomAttributeData>();

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new List<CustomAttributeData>();
        }

    }

    public class FakePropertyInfo : PropertyInfo
    {
        private readonly FieldDeclaration source;
        private readonly Type declaringType;

        public FakePropertyInfo(Type declaringType, FieldDeclaration source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.declaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
        }

        public override MemberTypes MemberType => MemberTypes.Property;

        public override Type PropertyType => source.type;

        public override bool CanRead => source.get;

        public override bool CanWrite => source.set;

        public override PropertyAttributes Attributes => PropertyAttributes.None;

        public override Type DeclaringType => declaringType;

        public override string Name => source.FieldName;

        public override Type ReflectedType => declaringType;

        public override MethodInfo[] GetAccessors(bool nonPublic) => Array.Empty<MethodInfo>();

        public override MethodInfo GetGetMethod(bool nonPublic) => null;

        public override MethodInfo GetSetMethod(bool nonPublic) => null;

        public override ParameterInfo[] GetIndexParameters() => Array.Empty<ParameterInfo>();

        public override object GetValue(object obj, object[] index)
        {
            if (index != null && index.Length > 0)
                throw new NotSupportedException("FakePropertyInfo does not support indexed properties.");
            return source.value;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return GetValue(obj, index);
        }

        public override void SetValue(object obj, object value, object[] index)
        {
            if (index != null && index.Length > 0)
                throw new NotSupportedException("FakePropertyInfo does not support indexed properties.");
            source.value = value;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            SetValue(obj, value, index);
        }

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