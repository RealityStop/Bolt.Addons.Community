using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(OnVariableChanged))]
    public class OnVariableChangedOption : UnitOption<OnVariableChanged>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public OnVariableChangedOption() : base() { }

        public OnVariableChangedOption(OnVariableChanged unit) : base(unit)
        {

        }

        public OnVariableChangedOption(VariableKind kind, string defaultName = null) : base() {
            this.kind = kind;
            this.name = defaultName;
            this.unit = (OnVariableChanged)Activator.CreateInstance(typeof(OnVariableChanged));
            FillFromUnit();
        }

        public override void Deserialize(UnitOptionRow row)
        {
            base.Deserialize(row);

            kind = (VariableKind)Enum.Parse(typeof(VariableKind), row.tag1);
            name = row.tag2;
        }


        public override UnitOptionRow Serialize()
        {
            var row = base.Serialize();

            row.tag1 = kind.ToString();
            row.tag2 = name;

            return row;
        }

        public string name { get; private set; }

        public VariableKind kind { get; private set; }

        public bool hasName => !string.IsNullOrEmpty(name);



        protected string NamedLabel(bool human)
        {
            return $"On {name} Variable Changed";
        }

        protected string UnnamedLabel(bool human)
        {
            return $"On {kind} Variable Changed";
        }


        protected override string FavoriteKey()
        {
            return $"{unit.GetType().FullName}${name}";
        }

        private string DimmedKind()
        {
            return LudiqGUIUtility.DimString($" ({kind})");
        }

        protected override string Label(bool human)
        {
            if (hasName)
            {
                return NamedLabel(human);
            }
            else
            {
                return UnnamedLabel(human);
            }
        }

        public override string SearchResultLabel(string query)
        {
            if (hasName)
            {
                return base.SearchResultLabel(query) + DimmedKind();
            }
            else
            {
                return base.SearchResultLabel(query);
            }
        }

        protected override EditorTexture Icon()
        {
            return BoltCore.Icons.VariableKind(kind);
        }

        public override void PreconfigureUnit(OnVariableChanged unit)
        {
            unit.kind = kind;

            if (hasName)
            {
                unit.name.SetDefaultValue(name);
            }

            unit.Define(); // Force redefine, because we changed the kind
        }
    }
}