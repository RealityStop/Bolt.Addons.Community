using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [FuzzyOption(typeof(AssetFieldUnit))]
    public class AssetFieldUnitOption : UnitOption<AssetFieldUnit>
    {
        public AssetFieldUnitOption() : base() { }
    
        public AssetFieldUnitOption(AssetFieldUnit unit) : base(unit)
        {
            sourceScriptGuids = sourceScriptGuids = VisualScripting.LinqUtility.ToHashSet(UnitBase.GetScriptGuids(unit.field.type));
        }
    
        public bool humanNaming => BoltCore.Configuration.humanNaming;
    
        private ActionDirection direction => unit.actionDirection;
    
        public Type targetType { get; private set; }
    
        protected override string Label(bool human)
        {
            return unit.field.FieldName + $"({direction})";
        }
    
        public override bool favoritable => false;
    
        protected override int Order()
        {
            return 0;
        }
    
        public override string SearchResultLabel(string query)
        {
            return unit.field.FieldName + $" ({direction})";
        }
    
        protected override string Haystack(bool human)
        {
            return $"{unit.field.FieldName}{(human ? ": " : ".")}{Label(human)}";
        }
    
        protected override UnitCategory Category()
        {
            var humanName = humanNaming ? "Variables" : "Fields";
            return new UnitCategory($"CSharp/Asset/{humanName}");
        }
    }
    
}