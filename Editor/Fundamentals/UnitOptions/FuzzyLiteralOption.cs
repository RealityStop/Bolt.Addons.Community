using System;
using UnityObject = UnityEngine.Object;

namespace Unity.VisualScripting.Community 
{
    [FuzzyOption(typeof(FuzzyLiteral))]
    public sealed class FuzzyLiteralOption : UnitOption<FuzzyLiteral>
    {
        public FuzzyLiteralOption() : base() { }
    
        public FuzzyLiteralOption(FuzzyLiteral unit) : base(unit)
        {
        }
    
        public Type literalType { get; private set; }
    
        public override bool favoritable => false;
    
        public string currentQuery = "Fuzzy Literal";
    
        protected override int Order()
        {
            return 2;
        }
    
        public override string SearchResultLabel(string query)
        {
            return query + $"<color=#{ColorPalette.unityForegroundDim.ToHexString()}> (dynamic value)</color>";
        }

        // Used to make the query not match exactly so it will still prefer other matches over this.
        private const string Poison = "zzx";

        public override string formerHaystack => Poison + currentQuery + Poison;
    
        protected override void FillFromUnit()
        {
            literalType = unit.type;
            base.FillFromUnit();
        }
    
        public override string headerLabel => currentQuery;
    
        protected override string Label(bool human)
        {
            return currentQuery;
        }

        protected override string Haystack(bool human)
        {
            return Poison + currentQuery + Poison;
        }
    
        protected override EditorTexture Icon()
        {
            if (unit.value is UnityObject uo && !uo.IsUnityNull())
            {
                return uo.Icon();
            }
    
            return unit.type.Icon();
        }
    
        public override void Deserialize(UnitOptionRow row)
        {
            base.Deserialize(row);
    
            literalType = Codebase.DeserializeType(row.tag1);
        }
    
        public override UnitOptionRow Serialize()
        {
            var row = base.Serialize();
    
            row.tag1 = Codebase.SerializeType(literalType);
    
            return row;
        }
    
        public void Update(string label)
        {
            currentQuery = label;
            FillFromUnit();
        }
    } 
}