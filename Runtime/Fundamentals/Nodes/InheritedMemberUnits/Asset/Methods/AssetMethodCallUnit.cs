using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class AssetMethodCallUnit : AssetMemberUnit
    {
        public string name;
        public MethodDeclaration method;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [DoNotSerialize]
        public Dictionary<int, ValueInput> InputParameters { get; private set; }

        [DoNotSerialize]
        public Dictionary<int, ValueOutput> OutputParameters { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput result;

        [Serialize]
        public MethodType methodType;

        public string parameterCode;

        [Obsolete(Serialization.ConstructorWarning)]
        public AssetMethodCallUnit()
        {
        }

        public AssetMethodCallUnit(string _name, MethodDeclaration _method, MethodType methodType)
        {
            name = _name;
            method = _method;
            this.methodType = methodType;
        }
        private bool isRegistered;
        protected override void Definition()
        {
            if (method != null && !isRegistered)
            {
                method.OnSerialized += Define;
                isRegistered = true;
            }
            InputParameters = new Dictionary<int, ValueInput>();
            OutputParameters = new Dictionary<int, ValueOutput>();

            if (methodType == MethodType.Invoke || method.parameters.Any(param => param.modifier == Libraries.CSharp.ParameterModifier.Out) || method.parameters.Any(param => param.modifier == Libraries.CSharp.ParameterModifier.Ref))
            {
                enter = ControlInput(nameof(enter), (flow) =>
                {
                    Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                    return exit;
                });
                exit = ControlOutput(nameof(exit));
                Succession(enter, exit);
            }
            if (methodType == MethodType.ReturnValue && method.returnType != typeof(void) && method.returnType != typeof(Libraries.CSharp.Void))
            {
                result = ValueOutput(method.returnType, nameof(result), (flow) =>
                {
                    Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                    return method.returnType.PseudoDefault();
                });
            }

            for (int parameterIndex = 0; parameterIndex < method.parameters.Count; parameterIndex++)
            {
                var parameterInfo = method.parameters[parameterIndex];

                var parameterType = parameterInfo.type;

                if (parameterInfo.modifier != ParameterModifier.Out)
                {
                    var inputParameterKey = "%" + parameterInfo.name;

                    var isFakeGeneric = parameterInfo.type is FakeGenericParameterType;
                    var inputParameter = ValueInput(isFakeGeneric ? (parameterInfo.type as FakeGenericParameterType).BaseType : parameterType, inputParameterKey).NullMeansSelf();

                    InputParameters.Add(parameterIndex, inputParameter);

                    inputParameter.SetDefaultValue(isFakeGeneric ? (parameterInfo.type as FakeGenericParameterType).BaseType.PseudoDefault() : parameterType.PseudoDefault());

                    if (parameterInfo.hasDefault)
                    {
                        inputParameter.AllowsNull();
                    }

                    if (methodType == MethodType.Invoke)
                        Requirement(inputParameter, enter);

                    if (method.returnType != typeof(void) && method.returnType != typeof(Libraries.CSharp.Void) && methodType == MethodType.ReturnValue)
                    {
                        Requirement(inputParameter, result);
                    }
                }

                if (parameterInfo.type.IsByRef || parameterInfo.modifier == ParameterModifier.Out)
                {
                    var outputParameterKey = "&" + parameterInfo.name;

                    var outputParameter = ValueOutput(parameterType, outputParameterKey);

                    OutputParameters.Add(parameterIndex, outputParameter);

                    if (methodType == MethodType.Invoke)
                        Assignment(enter, outputParameter);
                }
            }
        }
    }
}