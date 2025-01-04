using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class AssetActionUnit : CodeAssetUnit
    {
        public AssetActionUnit() { }
        public AssetActionUnit(MethodDeclaration method)
        {
            this.method = method;
            actionType = GetActionType();
        }

        public MethodDeclaration method;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput action;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        [Serialize]
        public Type actionType;
        private Type GetActionType()
        {
            List<Type> parameters = method.parameters.Select(param => param.type).ToList();
            var paramCount = parameters.Count;

            return paramCount switch
            {
                0 => typeof(Action),
                1 => typeof(Action<>).MakeGenericType(parameters[0]),
                2 => typeof(Action<,>).MakeGenericType(parameters[0], parameters[1]),
                3 => typeof(Action<,,>).MakeGenericType(parameters[0], parameters[1], parameters[2]),
                4 => typeof(Action<,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3]),
                5 => typeof(Action<,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4]),
                6 => typeof(Action<,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]),
                7 => typeof(Action<,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]),
                8 => typeof(Action<,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]),
                9 => typeof(Action<,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8]),
                10 => typeof(Action<,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9]),
                11 => typeof(Action<,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10]),
                12 => typeof(Action<,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11]),
                13 => typeof(Action<,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12]),
                14 => typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13]),
                15 => typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13], parameters[14]),
                16 => typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13], parameters[14], parameters[15]),
                _ => throw new ArgumentException("Too many parameters. Action only supports up to 16 parameters."),
            };
        }


        protected override void Definition()
        {
            if (method != null)
            {
                // Insures that the type is correct if the Method Return Type is changed
                method.OnSerialized += UpdateActionType;
            }
            if (actionType == null) UpdateActionType();
            action = ValueOutput(actionType, nameof(action), (flow) => throw new Exception("This is not supported"));
        }

        private void UpdateActionType()
        {
            actionType = GetActionType();
        }
    }
}