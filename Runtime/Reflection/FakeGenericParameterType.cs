using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [fsObject(Converter = typeof(FakeGenericParameterTypeConverter))]
    [TypeIcon(typeof(Type))]
    public class FakeGenericParameterType : Type
    {
        public string _name;
        public TypeParameterConstraints _constraints;
        public Type _baseTypeConstraint;
        public List<Type> _interfaceConstraints = new();

        public readonly bool _isArrayType;
        public int _arrayRank;
        public FakeGenericParameterType _elementType;

        public int _position;

        public FakeGenericParameterType(
            string name,
            int position,
            TypeParameterConstraints constraints = TypeParameterConstraints.None,
            Type baseTypeConstraint = null,
            List<Type> interfaceConstraints = null)
        {
            _name = name;
            _position = position;
            _constraints = constraints;
            _baseTypeConstraint = baseTypeConstraint;
            _interfaceConstraints = interfaceConstraints ?? new List<Type>();
        }

        private FakeGenericParameterType(FakeGenericParameterType elementType, int rank)
        {
            _isArrayType = true;
            _arrayRank = rank;
            _elementType = elementType;
        }
        public void ChangeName(string newName)
        {
            _name = newName;
        }
        public void ChangeBaseTypeConstraint(Type newConstraint)
        {
            _baseTypeConstraint = newConstraint;
        }
        public void ChangeInterfaceTypeConstraints(List<Type> newConstraints)
        {
            _interfaceConstraints = newConstraints;
        }
        public void ChangeTypeParameterConstraints(TypeParameterConstraints newConstraints)
        {
            _constraints = newConstraints;
        }
        public void ChangePosition(int newPosition)
        {
            _position = newPosition;
        }

        public override Type GetElementType() => _isArrayType ? _elementType : throw new InvalidOperationException();

        public override Type MakeArrayType() => new FakeGenericParameterType(this, 1);

        public override Type MakeArrayType(int rank) => new FakeGenericParameterType(this, rank);
        public override int GetArrayRank()
        {
            return _isArrayType ? _arrayRank : 0;
        }
        public override Type MakeGenericType(params Type[] typeArguments)
        {
            throw new InvalidOperationException("FakeGenericParameterType does not support MakeGenericType.");
        }
        public override string Name => _isArrayType
            ? $"{_elementType.Name}[{new string(',', _arrayRank - 1)}]"
            : _name;
        public TypeParameterConstraints Constraints => _constraints;
        public Type BaseTypeConstraint => _baseTypeConstraint;
        public IReadOnlyList<Type> InterfaceConstraints => _interfaceConstraints.AsReadOnly();

        public override bool IsConstructedGenericType => false;
        public override bool IsGenericParameter => false;
        public override Type[] GetGenericParameterConstraints()
        {
            var list = new List<Type>();
            if (_baseTypeConstraint != null)
            {
                list.Add(_baseTypeConstraint);
            }

            foreach (var interfaceConstraint in InterfaceConstraints)
            {
                list.Add(interfaceConstraint);
            }
            return list.ToArray();
        }
        public override Type BaseType => _baseTypeConstraint ?? ((Constraints & TypeParameterConstraints.Struct) != 0 ? typeof(ValueType) : typeof(object));
        public override string ToString()
        {
            string constraintInfo = $"{_name}";
            if (_constraints != TypeParameterConstraints.None || _baseTypeConstraint != null || _interfaceConstraints.Count > 0)
            {
                var parts = new List<string>();
                if (_constraints.HasFlag(TypeParameterConstraints.Class)) parts.Add("class");
                if (_constraints.HasFlag(TypeParameterConstraints.Struct)) parts.Add("struct");
                if (_constraints.HasFlag(TypeParameterConstraints.New)) parts.Add("new()");
                if (_baseTypeConstraint != null) parts.Add(_baseTypeConstraint.Name);
                if (_interfaceConstraints.Count > 0) parts.AddRange(_interfaceConstraints.ConvertAll(i => i.As().CSharpName(false, true, false)));

                constraintInfo += $" : {string.Join(", ", parts)}";
            }
            return constraintInfo;
        }
        public override Assembly Assembly => BaseType.Assembly;
        public override string Namespace => "Unity.VisualScripting.Community.Generics";
        public override Module Module => BaseType.Module;
        public override Type DeclaringType => BaseType.DeclaringType;
        public override Type ReflectedType => BaseType.ReflectedType;
        public override RuntimeTypeHandle TypeHandle => BaseType.TypeHandle;
        public override string AssemblyQualifiedName => BaseType.AssemblyQualifiedName;
        public override string FullName => BaseType.As().CSharpName(false, true, false);
        public override Guid GUID => BaseType.GUID;

        public override Type UnderlyingSystemType => BaseType;
        public override GenericParameterAttributes GenericParameterAttributes
        {
            get
            {
                return Constraints switch
                {
                    TypeParameterConstraints.Class => GenericParameterAttributes.ReferenceTypeConstraint,
                    TypeParameterConstraints.Struct => GenericParameterAttributes.NotNullableValueTypeConstraint,
                    TypeParameterConstraints.New => GenericParameterAttributes.DefaultConstructorConstraint,
                    TypeParameterConstraints.None => GenericParameterAttributes.None,
                    _ => throw new InvalidOperationException(Constraints.ToString() + " is not supported!"),
                };
            }
        }

        public override int GenericParameterPosition => _position;

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }
        public override IEnumerable<CustomAttributeData> CustomAttributes
        {
            get
            {
                return Array.Empty<CustomAttributeData>();
            }
        }
        public override Type[] GetGenericArguments() => new Type[0];
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            if (BaseType != null)
            {
                return BaseType.GetConstructors(bindingAttr);
            }
            return typeof(object).GetConstructors(bindingAttr);
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            if (BaseType != null || InterfaceConstraints.Count > 0)
            {
                return BaseType?.GetEvent(name, bindingAttr) ?? InterfaceConstraints.FirstOrDefault(t => t.GetEvent(name, bindingAttr) != null)?.GetEvent(name, bindingAttr);
            }
            return null;
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            List<EventInfo> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetEvents(bindingAttr));
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetEvents(bindingAttr)));
            }
            return list.ToArray();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            if (BaseType != null || InterfaceConstraints.Count > 0)
            {
                return BaseType?.GetField(name, bindingAttr) ?? InterfaceConstraints.FirstOrDefault(t => t.GetField(name, bindingAttr) != null)?.GetField(name, bindingAttr);
            }
            return null;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<FieldInfo> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetFields(bindingAttr));
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetFields(bindingAttr)));
            }
            return list.ToArray();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (BaseType != null || InterfaceConstraints.Count > 0)
            {
                return BaseType?.GetInterface(name, ignoreCase) ?? InterfaceConstraints.FirstOrDefault(t => t.GetInterface(name, ignoreCase) != null)?.GetInterface(name, ignoreCase);
            }
            return null;
        }

        public override Type[] GetInterfaces()
        {
            List<Type> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetInterfaces());
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetInterfaces()));
            }
            return list.ToArray();
        }

        public override bool IsAssignableFrom(Type c)
        {
            if (c == null)
                return false;

            if (BaseType.IsAssignableFrom(c))
            {
                return true;
            }

            if (InterfaceConstraints.Any(i => i.IsAssignableFrom(c)))
            {
                return true;
            }

            return false;
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            List<MemberInfo> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetMembers(bindingAttr));
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetMembers(bindingAttr)));
            }
            return list.ToArray();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            List<MethodInfo> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetMethods(bindingAttr));
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetMethods(bindingAttr)));
            }
            return list.ToArray();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            if (BaseType != null)
            {
                return BaseType.GetNestedType(name, bindingAttr);
            }

            if (InterfaceConstraints.Count > 0)
            {
                return InterfaceConstraints.FirstOrDefault(t => t.GetNestedType(name, bindingAttr) != null)?.GetNestedType(name, bindingAttr);
            }
            return null;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            List<Type> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetNestedTypes(bindingAttr));
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetNestedTypes(bindingAttr)));
            }
            return list.ToArray();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            List<PropertyInfo> list = new();
            if (BaseType != null)
            {
                list.AddRange(BaseType.GetProperties(bindingAttr));
            }

            if (InterfaceConstraints.Count > 0)
            {
                list.AddRange(InterfaceConstraints.SelectMany(t => t.GetProperties(bindingAttr)));
            }
            return list.ToArray();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            return BaseType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return TypeAttributes.Public | (BaseType == null && InterfaceConstraints.Count > 0 ? TypeAttributes.Interface : TypeAttributes.Class);
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            var list = new List<CustomAttributeData>();

            if (BaseType != null)
            {
                list.AddRange(BaseType.GetCustomAttributesData());
            }

            foreach (var interfaceConstraint in InterfaceConstraints)
            {
                if (interfaceConstraint != null)
                {
                    list.AddRange(interfaceConstraint.GetCustomAttributesData());
                }
            }

            return list;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            // Returning this because when trying to return baseType or interface types attributes its logging a warning not sure why.
            return GetType().GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return GetType().GetCustomAttributes(attributeType, inherit);
        }
        protected override bool HasElementTypeImpl()
        {
            return false;
        }

        protected override bool IsArrayImpl()
        {
            return false;
        }

        protected override bool IsByRefImpl()
        {
            return false;
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }
    }
}