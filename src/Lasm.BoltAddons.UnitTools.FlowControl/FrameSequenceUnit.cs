using System;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;

namespace Lasm.BoltAddons.UnitTools.FlowControl
{
    [TypeIcon(typeof(Sequence))]
    [UnitTitle("Frame Sequence")]
    [UnitCategory("Control")]
    public class FrameSequenceUnit : Unit
    {
        private int count;

        [SerializeAs("outputCount")]
        private int _outputCount;

        [Inspectable]
        [UnitHeaderInspectable("Steps"), InspectorLabel("Steps")]
        public int outputCount
        {
            get { return _outputCount; }
            set { _outputCount = Mathf.Clamp(value, 1, 10); }
        }

        [DoNotSerialize]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlInput reset;

        [DoNotSerialize]
        public ReadOnlyCollection<ControlOutput> multiOutputs;

        private bool resetBool = false;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(StartSequence));
            reset = ControlInput("reset", new Action<Flow>(Reset));

            List<ControlOutput> _multiOutputs = new List<ControlOutput>();
            this.multiOutputs = _multiOutputs.AsReadOnly();

            for (int i = 0; i < outputCount; i++)
            {
                ControlOutput output = ControlOutput(i.ToString());
                base.Relation(enter, output);
                _multiOutputs.Add(output);
            }
        }

        public void StartSequence(Flow flow)
        {
            CoroutineRunner.instance.StartCoroutine(WaitForNextFrame(flow));
        }

        public IEnumerator WaitForNextFrame(Flow flow)
        {
            restart:
            count = 0;

            foreach (ControlOutput output in multiOutputs)
            {
                Flow _flow = Flow.New();
                _flow.Invoke(multiOutputs[count]);
                Debug.Log(Time.time);
                int _count = 0;

                while (_count < 1)
                {
                    _count++;
                    yield return null;
                }

                if (resetBool)
                {
                    resetBool = false;
                    goto restart;
                }

                count++;
            }
            
        }

        public void Reset(Flow flow)
        {
            resetBool = true;
        }

    }
}

