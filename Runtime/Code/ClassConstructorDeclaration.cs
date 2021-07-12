using Bolt.Addons.Community.Utility;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    public sealed class ClassConstructorDeclaration : ConstructorDeclaration
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }

    public sealed class FunctionUnit : Unit
    {
        public MethodDeclaration declaration;

        public FunctionType functionType;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput invoke;

        [DoNotSerialize]
        public List<ValueOutput> parameterPorts;

        public FunctionUnit() : base() { }

        public FunctionUnit(FunctionType functionType)
        {
            this.functionType = functionType;
        }

        protected override void Definition()
        {
            isControlRoot = true;

            invoke = ControlOutput("invoke");

            for (int i = 0; i < declaration?.parameters.Count; i++)
            {
                parameterPorts.Add(ValueOutput(declaration.parameters[i].type, declaration.parameters[i].name));
            }
        }

        public void Invoke(params object[] parameters)
        {
            if (declaration == null) throw new NullReferenceException($"{nameof(declaration)} cannot be null.");

            if (parameters.Length != declaration.parameters.Count) throw new ArgumentOutOfRangeException("parameters", "Parameters are not the correct count or types to invoke this method.");

            var flow = Flow.New(declaration.GetReference() as GraphReference);

            for (int i = 0; i < parameterPorts.Count; i++)
            {
                flow.SetValue(parameterPorts[i], parameters[i]);
            }

            flow.Invoke(invoke);
        }
    }

    public enum FunctionType
    {
        Constructor,
        Method,
        Getter,
        Setter
    }
}
