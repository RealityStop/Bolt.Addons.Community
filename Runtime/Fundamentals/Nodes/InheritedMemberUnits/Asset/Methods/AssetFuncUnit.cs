using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

public class AssetFuncUnit : CodeAssetUnit
{
    public AssetFuncUnit() { }
    public AssetFuncUnit(MethodDeclaration method)
    {
        this.method = method;
        funcType = GetFuncType();
    }

    public MethodDeclaration method;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput func;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput enter;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput exit;

    [Serialize]
    public Type funcType;
    private Type GetFuncType()
    {
        var result = method.returnType;
        List<Type> parameters = method.parameters.Select(param => param.type).ToList();
        var paramCount = parameters.Count;

        return paramCount switch
        {
            0 => typeof(Func<>).MakeGenericType(result),
            1 => typeof(Func<,>).MakeGenericType(parameters[0], result),
            2 => typeof(Func<,,>).MakeGenericType(parameters[0], parameters[1], result),
            3 => typeof(Func<,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], result),
            4 => typeof(Func<,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], result),
            5 => typeof(System.Func<,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], result),
            6 => typeof(System.Func<,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], result),
            7 => typeof(Func<,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], result),
            8 => typeof(Func<,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], result),
            9 => typeof(Func<,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], result),
            10 => typeof(Func<,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], result),
            11 => typeof(Func<,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], result),
            12 => typeof(Func<,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], result),
            13 => typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], result),
            14 => typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13], result),
            15 => typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13], parameters[14], result),
            16 => typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13], parameters[14], parameters[15], result),
            _ => throw new ArgumentException("Too many parameters. Func only supports up to 16 parameters."),
        };
    }

    protected override void Definition()
    {
        if (method != null)
        {
            // Insures that the type is correct if the Method Return Type is changed
            method.OnSerialized += UpdateFuncType;
        }
        if (funcType == null) UpdateFuncType();
        func = ValueOutput(funcType, nameof(func), (flow) => throw new Exception("This is not supported"));
    }

    private void UpdateFuncType()
    {
        funcType = GetFuncType();
    }
}