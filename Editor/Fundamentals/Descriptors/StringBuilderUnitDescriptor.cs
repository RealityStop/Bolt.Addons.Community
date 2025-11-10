using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System;
using Unity.VisualScripting.Community;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(StringBuilderUnit))]
    public class StringBuilderUnitDescriptor : UnitDescriptor<StringBuilderUnit>
    {
        public StringBuilderUnitDescriptor(StringBuilderUnit target) : base(target)
        {
        }

        protected override string DefinedTitle()
        {
            return "String Builder";
        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            description.showLabel = false;
        }
    }
}