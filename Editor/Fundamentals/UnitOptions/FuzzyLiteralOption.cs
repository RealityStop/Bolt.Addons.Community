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
            return 1;
        }
    
        public override string SearchResultLabel(string query)
        {
            return query;
        }
    
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