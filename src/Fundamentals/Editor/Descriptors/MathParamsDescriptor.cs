using Ludiq;
using Bolt;
using UnityEngine;
using UnityEditor;
using System;

namespace Bolt.Addons.Community.Fundamentals
{
    [Descriptor(typeof(MathParamNode))]
    public class MathParamsDescriptor : UnitDescriptor<MathParamNode>
    {
        public MathParamsDescriptor(MathParamNode unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            switch (unit.OperationType)
            {
                case MathParamNode.MathType.Add: return typeof(Bolt.GenericAdd).Icon();
                case MathParamNode.MathType.Subtract: return typeof(Bolt.GenericSubtract).Icon();
                case MathParamNode.MathType.Multiply: return typeof(Bolt.GenericMultiply).Icon();
                case MathParamNode.MathType.Divide: return typeof(Bolt.GenericDivide).Icon();
                default: return base.DefinedIcon();
            }
        }
    }
}