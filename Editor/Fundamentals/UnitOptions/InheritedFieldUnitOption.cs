using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [FuzzyOption(typeof(InheritedFieldUnit))]
    public class InheritedFieldUnitOption : UnitOption<InheritedFieldUnit>
    {
        public InheritedFieldUnitOption() : base() { }
    
        public InheritedFieldUnitOption(InheritedFieldUnit unit) : base(unit)
        {
            sourceScriptGuids = sourceScriptGuids = VisualScripting.LinqUtility.ToHashSet(UnitBase.GetScriptGuids(unit.member.targetType));
        }
    
        private Member _member;
    
        private Member _pseudoDeclarer;
    
        public Member member
        {
            get => _member ?? unit.member;
            set => _member = value;
        }
    
        public Member pseudoDeclarer
        {
            get => _pseudoDeclarer ?? member.ToPseudoDeclarer();
            set => _pseudoDeclarer = value;
        }
    
        public bool humanNaming => BoltCore.Configuration.humanNaming;
    
        private ActionDirection direction => unit.actionDirection;
    
        public Type targetType { get; private set; }
    
        protected override GUIStyle Style()
        {
            return base.Style();
        }
    
        protected override string Label(bool human)
        {
            return "this." + unit.member.info.CSharpName(direction);
        }
    
        public override bool favoritable => false;
    
        protected override int Order()
        {
            return 0;
        }
    
        public override string SearchResultLabel(string query)
        {
            return "this." + unit.member.info.CSharpName(direction);
        }
    
        protected override string Haystack(bool human)
        {
            return $"this{(human ? ": " : ".")}{Label(human)}";
        }
    
        protected override void FillFromUnit()
        {
            targetType = unit.member.targetType;
            member = unit.member;
            pseudoDeclarer = member.ToPseudoDeclarer();
    
            base.FillFromUnit();
        }
    
        protected override UnitCategory Category()
        {
            var humanName = humanNaming ? "Variables" : "Fields";
            return direction switch
            {
                ActionDirection.Get => new UnitCategory($"CSharp/InheritedMembers/{humanName}/Get"),
                ActionDirection.Set => new UnitCategory($"CSharp/InheritedMembers/{humanName}/Set"),
                _ => base.Category(),
            };
        }
    
        public override void Deserialize(UnitOptionRow row)
        {
            base.Deserialize(row);
    
            targetType = Codebase.DeserializeType(row.tag1);
    
            if (!string.IsNullOrEmpty(row.tag2))
            {
                member = Codebase.DeserializeMember(row.tag2);
            }
    
            if (!string.IsNullOrEmpty(row.tag3))
            {
                pseudoDeclarer = Codebase.DeserializeMember(row.tag3);
            }
        }
    
        public override UnitOptionRow Serialize()
        {
            var row = base.Serialize();
    
            row.tag1 = Codebase.SerializeType(targetType);
            row.tag2 = Codebase.SerializeMember(member);
            row.tag3 = Codebase.SerializeMember(pseudoDeclarer);
    
            return row;
        }
    
        public override void OnPopulate()
        {
            // Members are late-reflected to speed up loading and search
            // We only reflect them when we're just about to populate their node
            // By doing it in OnPopulate instead of on-demand later, we ensure
            // any error will be gracefully catched and shown as a warning by
            // the fuzzy window
    
            member.EnsureReflected();
            pseudoDeclarer.EnsureReflected();
    
            base.OnPopulate();
        }
    }
    
}