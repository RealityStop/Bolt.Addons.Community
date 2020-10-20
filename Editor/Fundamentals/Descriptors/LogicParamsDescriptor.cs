using Ludiq;
using Bolt;
using UnityEngine;
using UnityEditor;
using System;

namespace Bolt.Addons.Community.Fundamentals
{
    [Descriptor(typeof(LogicParams))]
    public class LogicParamsDescriptor : UnitDescriptor<LogicParams>
    {
        public LogicParamsDescriptor(LogicParams unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            switch (unit.BranchingType)
            {
                case LogicParamNode.BranchType.And: return typeof(Bolt.And).Icon();
                case LogicParamNode.BranchType.Or: return typeof(Bolt.Or).Icon();
                default: return base.DefinedIcon();
            }
        }
    }
}