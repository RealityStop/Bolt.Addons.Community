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
    internal sealed class BasePropertySetterUnitOption : UnitOption<BasePropertySetterUnit>
    {
        public BasePropertySetterUnitOption(BasePropertySetterUnit unit) : base(unit)
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

        private ActionDirection direction = ActionDirection.Set;

        public Type targetType { get; private set; }

        protected override string Label(bool human)
        {
            return "base." + unit.member.info.CSharpName(direction);
        }
        
        public override bool favoritable => false;

        protected override int Order()
        {
            return 0;
        }

        public override string SearchResultLabel(string query)
        {
            return "base." + unit.member.info.CSharpName(direction);
        }

        protected override string Haystack(bool human)
        {
            return $"base.{targetType.CSharpName(direction)}{(human ? ": " : ".")}{Label(human)}";
        }

        protected override void FillFromUnit()
        {
            targetType = unit.member.targetType;
            member = unit.member;
            pseudoDeclarer = member.ToPseudoDeclarer();

            base.FillFromUnit();
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