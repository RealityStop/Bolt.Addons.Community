using System;
using UnityEngine;
using TMPro;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community\\EaseOfAccess")]
    [UnitTitle("EaseTextMeshPro")]
    [UnitShortTitle("TextMeshPro (SetText)")]
    [TypeIcon(typeof(TextMeshPro))]
    [UnitOrder(0)]
    public class TextMeshProEase : Unit
    {
        [PortLabelHidden]
        [PortLabel("Target")]
        [NullMeansSelf]
        [DoNotSerialize]
        public ValueInput TMPUGUIComponent;

        [PortLabelHidden]
        [PortLabel("Target")]
        [NullMeansSelf]
        [DoNotSerialize]
        public ValueInput TMPComponent;

        [PortLabelHidden]
        [PortLabel("In")]
        [DoNotSerialize]
        public ControlInput In;

        [PortLabelHidden]
        [PortLabel("Out")]
        [DoNotSerialize]
        public ControlOutput Out;

        [PortLabelHidden]
        [PortLabel("Text")]
        [DoNotSerialize]
        public ValueInput Text;

        [PortLabelHidden]
        [PortLabel("Result")]
        [DoNotSerialize]
        public ValueOutput Result;

        [UnitHeaderInspectable("Type")]
        [Inspectable]
        public TMPType type;

        protected override void Definition()
        {
            In = ControlInput(nameof(In), SetText);
            Out = ControlOutput(nameof(Out));

            switch (type) 
            {
                case TMPType.Normal: TMPComponent = ValueInput<TextMeshPro>(nameof(TMPComponent), default).NullMeansSelf();
                        break;
                case TMPType.UGUI: TMPUGUIComponent = ValueInput<TextMeshProUGUI>(nameof(TMPUGUIComponent), default).NullMeansSelf();
                    break;
            }

            Text = ValueInput<string>(nameof(Text), default);

            Result = ValueOutput<string>(nameof(Result));

            Succession(In, Out);
        }

        private ControlOutput SetText(Flow flow)
        {
            switch (type)
            {
                case TMPType.Normal:
                    {
                        string _Text = flow.GetValue<string>(Text);
                        TextMeshPro _TextMeshPro = flow.GetValue<TextMeshPro>(TMPComponent);
                        _TextMeshPro.text = _Text;
                        flow.SetValue(Result, _Text);
                    }
                    break;
                case TMPType.UGUI:
                    {
                        string _Text = flow.GetValue<string>(Text);
                        TextMeshProUGUI _TextMeshPro = flow.GetValue<TextMeshProUGUI>(TMPUGUIComponent);
                        _TextMeshPro.text = _Text;
                        flow.SetValue(Result, _Text);
                    }   
                    break;
            }
            return Out;
        }
    }
}


public enum TMPType
{
    Normal,
    UGUI
}
