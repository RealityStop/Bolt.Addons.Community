using System;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;

namespace Lasm.BoltAddons.UnitTools.FlowControl
{
    [TypeIcon(typeof(While))]
    [UnitSurtitle("Control")]
    [UnitTitle("Cycle Control")]
    [UnitShortTitle("Cycle")]
    [UnitCategory("Control")]
    public class CycleControlUnit : Unit
    {
        private int count = -1;

        [SerializeAs("controlCount")]
        private int _controlCount;

        [Inspectable]
        [UnitHeaderInspectable("Steps"), InspectorLabel("Steps")]
        public int controlCount
        {
            get { return _controlCount; }
            set { _controlCount = Mathf.Clamp(value, 1, 10); }
        }

        [Inspectable]
        [InspectorToggleLeft]
        [UnitHeaderInspectable("Use Tags"), InspectorLabel("Use Tags ")]
        public bool useTags;


        [DoNotSerialize]
        public ValueInput ascend;

        [DoNotSerialize]
        public ValueOutput indexOut;
        [DoNotSerialize]
        public ValueOutput tagOut;
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlInput reset;

        [DoNotSerialize]
        public ReadOnlyCollection<ControlOutput> multiOutputs;
        [DoNotSerialize]
        public ReadOnlyCollection<ValueInput> multiValueInputs;
        [DoNotSerialize]
        public bool lastAscended = false;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Trigger));
            reset = ControlInput("reset", new Action<Flow>(Reset));
            ascend = ValueInput<bool>("ascend", false);

            List<ValueInput> _multiValueInputs = new List<ValueInput>();
            this.multiValueInputs = _multiValueInputs.AsReadOnly();

            List<ControlOutput> _multiOutputs = new List<ControlOutput>();
            this.multiOutputs = _multiOutputs.AsReadOnly();

            for (int i = 0; i < _controlCount; i++)
            {
                if (useTags)
                {
                    ValueInput tagIndex = ValueInput<string>("tag_" + i.ToString(), String.Empty);
                }
            }

            if (useTags)
            {
                Func<Recursion, string> getTag = getCurrentTag => CurrentTag();
                tagOut = ValueOutput("tag", getTag);

                for (int i = 0; i < controlCount; i++)
                {
                    ControlOutput output = ControlOutput("control_" + i.ToString());
                    base.Relation(enter, output);
                    _multiOutputs.Add(output);
                }
            }
            else
            {
                Func<Recursion, int> getIndex = getCurrentIndex => CurrentIndex();
                indexOut = ValueOutput("index", getIndex);

                for (int i = 0; i < controlCount; i++)
                {
                    ControlOutput output = ControlOutput("control_" + i.ToString());
                    base.Relation(enter, output);
                    _multiOutputs.Add(output);
                }
            }



        }

        public string CurrentTag()
        {
            return valueInputs["tag_" + count].GetValue<string>();
        }

        public int CurrentIndex()
        {
            return count;
        }

        public void Trigger(Flow flow)
        {
            Flow _flow = Flow.New();



            if (!ascend.GetValue<bool>())
            {
                if (count < controlOutputs.Count - 1)
                {
                    count++;
                }
                else
                {
                    count = 0;
                }
            }
            else
            {

                if (count > 0)
                {
                    count--;

                }
                else
                {
                    count = controlOutputs.Count - 1;
                }
            }

            _flow.Invoke(controlOutputs["control_" + count]);
            _flow.Dispose();
        }

        public void Reset(Flow flow)
        {
            count = -1;
        }

    }


}





