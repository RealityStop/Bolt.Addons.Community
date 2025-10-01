using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [UnitTitle("Function")]
    [SpecialUnit]
    [RenamedFrom("Bolt.Addons.Community.Code.FunctionUnit")]
    public sealed class FunctionNode : Unit
    {
        public MethodDeclaration methodDeclaration;
        public FieldDeclaration fieldDeclaration;
        public ConstructorDeclaration constructorDeclaration;

        public FunctionType functionType;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput invoke;

        [DoNotSerialize]
        public List<ValueOutput> parameterPorts = new List<ValueOutput>();

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput returnValue;

        public FunctionNode() : base() { }

        public FunctionNode(FunctionType functionType)
        {
            this.functionType = functionType;
        }

        protected override void Definition()
        {
            isControlRoot = true;

            invoke = ControlOutput("invoke");

            switch (functionType)
            {
                case FunctionType.Method:
                    MethodDefinition();
                    break;

                case FunctionType.Getter:
                    GetterDefinition();
                    break;

                case FunctionType.Setter:
                    SetterDefinition();
                    break;

                case FunctionType.Constructor:
                    ConstructorDefinition();
                    break;
            }
        }

        private void ConstructorDefinition()
        {
            if (constructorDeclaration == null) return;
            for (int i = 0; i < constructorDeclaration.parameters.Count; i++)
            {
                if (constructorDeclaration.parameters[i].type != null && !string.IsNullOrEmpty(constructorDeclaration.parameters[i].name)) parameterPorts.Add(ValueOutput(constructorDeclaration.parameters[i].type, constructorDeclaration.parameters[i].name));
            }
        }

        private void SetterDefinition()
        {
            parameterPorts.Add(ValueOutput(fieldDeclaration.type, "value"));
        }

        private void GetterDefinition()
        {
            // if (fieldDeclaration?.type != null)
            // {
            //     returnValue = ValueInput(fieldDeclaration.type, "value");
            // }
        }

        private void MethodDefinition()
        {
            if (methodDeclaration != null)
            {
                // if (methodDeclaration.returnType != null && (methodDeclaration.returnType != typeof(void) || methodDeclaration.returnType != typeof(Libraries.CSharp.Void)))
                // {
                //     returnValue = ValueInput(methodDeclaration.returnType, "result");
                // }

                for (int i = 0; i < methodDeclaration.parameters.Count; i++)
                {
                    if (methodDeclaration.parameters[i].type != null && !string.IsNullOrEmpty(methodDeclaration.parameters[i].name)) parameterPorts.Add(ValueOutput(methodDeclaration.parameters[i].type, methodDeclaration.parameters[i].name));
                }
            }
        }

        public object New(Type type, params object[] parameters)
        {
            if (type.Inherits<ScriptableObject>()) return ScriptableObject.CreateInstance(type);
            return type.New(parameters);
        }

        public object Get()
        {
            if (fieldDeclaration == null) throw new NullReferenceException($"{nameof(fieldDeclaration)} cannot be null.");
            if (!fieldDeclaration.get) throw new InvalidOperationException($"Cannot get value from field {fieldDeclaration.name}");
            var flow = Flow.New(fieldDeclaration.getter.GetReference() as GraphReference);
            var result = flow.GetValue(returnValue);
            return result;
        }

        public void Set(object value)
        {
            if (fieldDeclaration == null) throw new NullReferenceException($"{nameof(fieldDeclaration)} cannot be null.");
            if ((value != null && !fieldDeclaration.type.IsAssignableFrom(value.GetType())) || !fieldDeclaration.set)
            {
                throw new ArgumentException($"Value of type {value.GetType()} cannot be assigned to field of type {fieldDeclaration.type}");
            }

            var flow = Flow.New(fieldDeclaration.setter.GetReference() as GraphReference);
            flow.SetValue(parameterPorts[0], value);
            flow.Invoke(invoke);
        }

        public object Invoke(params object[] parameters)
        {
            if (methodDeclaration == null) throw new NullReferenceException($"{nameof(methodDeclaration)} cannot be null.");
            if (parameters.Length != methodDeclaration.parameters.Count)
                throw new ArgumentException($"Expected {methodDeclaration.parameters.Count} parameters but got {parameters.Length}");

            // Validate parameter types
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && !methodDeclaration.parameters[i].type.IsAssignableFrom(parameters[i].GetType()))
                {
                    throw new ArgumentException($"Parameter {i} of type {parameters[i].GetType()} cannot be assigned to parameter of type {methodDeclaration.parameters[i].type}");
                }
            }

            var flow = Flow.New(methodDeclaration.GetReference() as GraphReference);

            for (int i = 0; i < parameterPorts.Count; i++)
            {
                flow.SetValue(parameterPorts[i], parameters[i]);
            }

            flow.Invoke(invoke);

            return returnValue != null ? flow.GetValue(returnValue) : null;
        }
    }
}
