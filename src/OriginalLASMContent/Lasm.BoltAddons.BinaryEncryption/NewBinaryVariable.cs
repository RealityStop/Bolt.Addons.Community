using System;
using Ludiq;
using Bolt;


namespace Lasm.BoltAddons.BinaryEncryption
{
    [UnitSurtitle("Binary Encryption")]
    [UnitShortTitle("Create Variable")]
    public class NewBinaryVariable : BinaryEncryptionBaseUnit
    {
        [Inspectable]
        [UnitHeaderInspectable("Type"), InspectorLabel("Type")]
        public Type type = typeof(int);

        [DoNotSerialize]
        public ValueInput name;
        [AllowsNull]
        [DoNotSerialize]
        public ValueInput value;
        [DoNotSerialize]
        public ValueInput valueNull;
        [DoNotSerialize]
        public ValueOutput variable;

        protected override void Definition()
        {
            name = ValueInput<string>("name", string.Empty);
            
            
            if (type.IsValueType)
            {
                value = ValueInput(type, "value");
                value.SetDefaultValue(Activator.CreateInstance(type));
            }
            else
            {
                valueNull = ValueInput(type, "valueNull");
                valueNull.SetDefaultValue(null);
            }

            Func<Recursion, BinaryVariable> binaryVariable = getBinaryVariable => GetBinaryVariable();
            variable = ValueOutput<BinaryVariable>("variable", binaryVariable);
        }

        private BinaryVariable GetBinaryVariable()
        {
            BinaryVariable _variable = new BinaryVariable();
            _variable.name = name.GetValue<string>();
            _variable.variable = value.GetValue(type);

            return _variable;
        }
    }
}

