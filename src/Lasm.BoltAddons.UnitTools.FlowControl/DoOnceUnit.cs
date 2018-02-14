using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.FlowControl
{
    [TypeIcon(typeof(SwitchOnEnum))]
    [UnitTitle("Do Once")]
    [UnitCategory("Control")]
    public class DoOnceUnit : Unit
    {
        [DoNotSerialize]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlInput reset;

        [DoNotSerialize]
        public ControlOutput doOnce;

        [DoNotSerialize]
        public ControlOutput and;

        [DoNotSerialize]
        public ControlOutput then;

        [DoNotSerialize]
        public bool doOnceComplete = false;
        [DoNotSerialize]
        public bool thenOccured = false;

        [Inspectable]
        [InspectorToggleLeft]
        [UnitHeaderInspectable("Then")]
        public bool thenHeader;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));
            reset = ControlInput("reset", new Action<Flow>(Reset));
            doOnce = ControlOutput("doOnce");

            if (thenHeader)
            {
                then = ControlOutput("then");
                Relation(enter, then);
            } else
            {
                and = ControlOutput("and");
                Relation(enter, and);
            }

            Relation(enter, doOnce);
            
        }

        private void Enter(Flow flow)
        {
            Flow _flowDoOnce = Flow.New();
            Flow _flowAndThen = Flow.New();

            if (!doOnceComplete)
            {
                DoOnce(_flowDoOnce);
            }

            AndThen(_flowAndThen);
            
        }

        private void Reset(Flow flow)
        {
            doOnceComplete = false;
            thenOccured = false;
        }

        private void DoOnce(Flow flow)
        {
            doOnceComplete = true;
            flow.Invoke(doOnce);
        }

        private void AndThen(Flow flow)
        {
            CoroutineRunner.instance.StartCoroutine(WaitForNextFrame(flow));
        }

        private IEnumerator WaitForNextFrame(Flow flow)
        {
            if (thenHeader)
            {
                if (!thenOccured)
                {
                    int count = 0;

                    while (count < 1)
                    {
                        count++;
                        yield return null;
                    }

                    flow.Invoke(then);
                }
                else
                {
                    flow.Invoke(then);
                }
            } else
            {
                flow.Invoke(and);
            }

        }

    }
}
