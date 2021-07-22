using Bolt.Addons.Libraries.Humility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    [UnitTitle("Function")]
    [SpecialUnit]
    public sealed class FunctionUnit : Unit
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

        public FunctionUnit() : base() { }

        public FunctionUnit(FunctionType functionType)
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
            
        }

        private void SetterDefinition()
        {
            
        }

        private void GetterDefinition()
        {
            
        }

        private void MethodDefinition()
        {
            for (int i = 0; i < methodDeclaration?.parameters.Count; i++)
            {
                if (methodDeclaration.parameters[i].type != null && !string.IsNullOrEmpty(methodDeclaration.parameters[i].name)) parameterPorts.Add(ValueOutput(methodDeclaration.parameters[i].type, methodDeclaration.parameters[i].name));
            }
        }

        public object New(Type type, params object[] parameters)
        {
            if (type.Inherits<ScriptableObject>()) return ScriptableObject.CreateInstance(type);
            return type.New(parameters);
        }

        public object Get()
        {
            // To Do for Live C#
            return null;
        }

        public void Set(object value)
        {
            // To Do for Live C#
        }

        public void Invoke(params object[] parameters)
        {
            if (methodDeclaration == null) throw new NullReferenceException($"{nameof(methodDeclaration)} cannot be null.");

            if (parameters.Length != methodDeclaration.parameters.Count) throw new ArgumentOutOfRangeException("parameters", "Parameters are not the correct count or types to invoke this method.");

            var flow = Flow.New(methodDeclaration.GetReference() as GraphReference);

            for (int i = 0; i < parameterPorts.Count; i++)
            {
                flow.SetValue(parameterPorts[i], parameters[i]);
            }

            flow.Invoke(invoke);
        }
    }
}
