using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.BinaryEncryption
{

    [TypeIcon(typeof(IList<BinaryVariable>))]
    [UnitSurtitle("Binary Encryption")]
    [UnitTitle("Create Binary Variable List")]
    [UnitShortTitle("Create Variable List")]
    [UnitCategory("Data")]
    public class CreateBinaryVariables : MultiInputUnit<BinaryVariable>
    {
        protected override int minInputCount
        {
            get
            {
                return 0;
            }
        }

        [SerializeAs("inputCount")]
        private int _inputCount;
        [Inspectable, InspectorLabel("Variables"), UnitHeaderInspectable("Variables")]
        public override int inputCount
        {
            get
            {
                return _inputCount;
            }
            set
            {
                _inputCount = Mathf.Clamp(value, minInputCount, 100);
            }
        }

        [PortLabelHidden, DoNotSerialize]
        public ValueOutput variables;

        protected override void Definition()
        {
            variables = ValueOutput<IList>("variables", new Func<Recursion, IList>(CreateVariables));
            base.Definition();
            foreach(ValueInput input in base.multiInputs)
            {
                base.Relation(input, variables);
            }
        }
        

        public IList CreateVariables(Recursion recursion)
        {
            IList variableList = new List<BinaryVariable>();
            
            foreach(ValueInput input in base.multiInputs)
            {
                variableList.Add(input.GetValue<BinaryVariable>());
            }
            return variableList;
        }
    }

    
}

