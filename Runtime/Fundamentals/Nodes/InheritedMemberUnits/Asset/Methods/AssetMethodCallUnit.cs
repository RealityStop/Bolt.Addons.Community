using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("CSharp/Asset/Methods")]
    public class AssetMethodCallUnit : Unit
    {
        public string name;
        public MethodDeclaration method;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize]
        public Dictionary<int, ValueInput> InputParameters { get; private set; }

        [DoNotSerialize]
        public Dictionary<int, ValueOutput> OutputParameters { get; private set; }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput result;

        public MethodType methodType;

        public string parameterCode;

        [Serialize]
        List<string> parameterNames;

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

        protected override void Definition()
        {
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
                result = ValueOutput(method.returnType, nameof(result), (flow) =>
                {
                    Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                    return method.returnType.PseudoDefault();
                });

            var parameterInfos = method.parameters.ToArray();

            var parameterCount = parameterInfos.Length;

            for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
            {
                var parameterInfo = parameterInfos[parameterIndex];

                var parameterType = parameterInfo.type;

                if (parameterInfo.modifier != ParameterModifier.Out)
                {
                    var inputParameterKey = "%" + parameterInfo.name;

                    if (parameterNames != null && parameterNames[parameterIndex] != parameterInfo.name)
                    {
                        inputParameterKey = "%" + parameterNames[parameterIndex];
                    }

                    var inputParameter = ValueInput(parameterType, inputParameterKey);

                    InputParameters.Add(parameterIndex, inputParameter);

                    inputParameter.SetDefaultValue(parameterInfo.type.PseudoDefault());

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

                    if (parameterNames != null && parameterNames[parameterIndex] != parameterInfo.name)
                    {
                        outputParameterKey = "&" + parameterNames[parameterIndex];
                    }

                    var outputParameter = ValueOutput(parameterType, outputParameterKey);

                    OutputParameters.Add(parameterIndex, outputParameter);

                    if (methodType == MethodType.Invoke)
                        Assignment(enter, outputParameter);
                }
            }

            parameterNames ??= parameterInfos.Select(pInfo => pInfo.name).ToList();
        }
    }
}