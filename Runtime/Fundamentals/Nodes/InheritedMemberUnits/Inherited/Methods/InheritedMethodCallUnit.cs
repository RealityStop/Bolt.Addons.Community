using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [UnitSurtitle("Inherited")]
    public class InheritedMethodCall : InheritedMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public InheritedMethodCall() { }
    
        public InheritedMethodCall(Member member, MethodType methodType)
        {
            this.member = member;
            memberType = MemberType.Method;
            this.methodType = methodType;
        }
    
        [UnitHeaderInspectable("Show 'this'")]
        [Inspectable]
        [InspectorLabel("Show 'this'")]
        public bool showThisKeyword = true;
    
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
    
        protected override void Definition()
        {
            InputParameters = new Dictionary<int, ValueInput>();
            OutputParameters = new Dictionary<int, ValueOutput>();
    
            if (methodType == MethodType.Invoke || member.methodInfo.GetParameters().Any(param => param.HasOutModifier()))
            {
                enter = ControlInput(nameof(enter), (flow) =>
                {
                    Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                    return exit;
                });
                exit = ControlOutput(nameof(exit));
                Succession(enter, exit);
            }
    
            if (methodType == MethodType.ReturnValue && member.isGettable)
                result = ValueOutput(member.methodInfo.ReturnType, nameof(result), (flow) =>
                {
                    Debug.Log("This node is for the code generators only it does not work inside normal graphs");
                    return member.methodInfo.ReturnType.PseudoDefault();
                });
    
            var parameterInfos = member.GetParameterInfos().ToArray();
    
            var parameterCount = parameterInfos.Length;
    
            for (int parameterIndex = 0; parameterIndex < parameterCount; parameterIndex++)
            {
                var parameterInfo = parameterInfos[parameterIndex];
    
                var parameterType = parameterInfo.UnderlyingParameterType();
    
                if (!parameterInfo.HasOutModifier())
                {
                    var inputParameterKey = "%" + parameterInfo.Name;
    
                    if (parameterNames != null && parameterNames[parameterIndex] != parameterInfo.Name)
                    {
                        inputParameterKey = "%" + parameterNames[parameterIndex];
                    }
    
                    var inputParameter = ValueInput(parameterType, inputParameterKey);
    
                    InputParameters.Add(parameterIndex, inputParameter);
    
                    inputParameter.SetDefaultValue(parameterInfo.PseudoDefaultValue());
    
                    if (parameterInfo.AllowsNull() || parameterInfo.IsOptional)
                    {
                        inputParameter.AllowsNull();
                    }
    
                    if (methodType == MethodType.Invoke)
                        Requirement(inputParameter, enter);
    
                    if (member.isGettable && methodType == MethodType.ReturnValue)
                    {
                        Requirement(inputParameter, result);
                    }
                }
    
                if (parameterInfo.ParameterType.IsByRef || parameterInfo.IsOut)
                {
                    var outputParameterKey = "&" + parameterInfo.Name;
    
                    if (parameterNames != null && parameterNames[parameterIndex] != parameterInfo.Name)
                    {
                        outputParameterKey = "&" + parameterNames[parameterIndex];
                    }
    
                    var outputParameter = ValueOutput(parameterType, outputParameterKey);
    
                    OutputParameters.Add(parameterIndex, outputParameter);
    
                    if (methodType == MethodType.Invoke)
                        Assignment(enter, outputParameter);
                }
            }
    
            parameterNames ??= parameterInfos.Select(pInfo => pInfo.Name).ToList();
        }
    }
    
}