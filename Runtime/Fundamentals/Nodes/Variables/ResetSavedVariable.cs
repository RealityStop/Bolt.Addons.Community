using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
[RenamedFrom("ResetSavedVariable")]    
    [UnitCategory("Community/Variables")]
    [UnitTitle("ResetSavedVariable")]//Unit title
    [TypeIcon(typeof(FlowGraph))]//Unit icon
    public class ResetSavedVariable : Unit
    {
        [SerializeAs(nameof(_argumentCount))]
        private int _argumentCount;
    
        [UnitHeaderInspectable("Keys")]
        [DoNotSerialize]
        public int KeyCount
        {
            get
            {
                return Mathf.Max(1, _argumentCount);
            }
            set
            {
                _argumentCount = Mathf.Clamp(value, 1, 10);
            }
        }
    
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Reset;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput OnReset;
        [DoNotSerialize]
        public List<ValueInput> Keys;
    
        protected override void Definition()
        {
            Keys = new List<ValueInput>();
    
            Reset = ControlInput(nameof(Reset), _Enter_);
            OnReset = ControlOutput(nameof(OnReset));
    
            for (int i = 0; i < KeyCount; i++)
            {
                var input = ValueInput("Key " + i, "");
                Requirement(input, Reset);
                Keys.Add(input);
            }
    
            Succession(Reset, OnReset);
        }
    
        public ControlOutput _Enter_(Flow flow)
        {
            foreach (var Key in Keys)
            {
                string key = flow.GetValue<string>(Key);
    
                var initalvariable = SavedVariables.initial.GetDeclaration(key).CloneViaFakeSerialization();
                SavedVariables.saved[key] = initalvariable.value;
    
                if (SavedVariables.current != SavedVariables.initial) SavedVariables.current[key] = initalvariable.value;
            }
            return OnReset;
        }
    }
}