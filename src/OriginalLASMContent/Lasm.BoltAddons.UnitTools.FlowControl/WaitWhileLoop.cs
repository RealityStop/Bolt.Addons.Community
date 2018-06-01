using System;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Lasm.BoltAddons.UnitTools.FlowControl
{
    [UnitTitle("Wait While Loop")]
    public sealed class WaitWhileLoopUnit : WaitLoopUnit, IUnityLifecycle
    {

        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlInput breakIn;
        [DoNotSerialize]
        public ControlOutput breakOut;
        [DoNotSerialize]
        public ControlOutput entered;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ControlOutput loop;

        [DoNotSerialize]
        public ValueInput condition;

        [SerializeAs("async")]
        private bool _async;

        [Inspectable][InspectorToggleLeft]
        [UnitHeaderInspectable("Async"), InspectorLabel("Async")]
        public bool async
        {
            get { return _async; }
            set { _async = value; }
        }

        private bool destroyed = false;
        private bool isRunning = false;

        private CoroutineRunner coroutineInstance;
        private AotList coroutineInstances = new AotList();
        private Coroutine loopRoutine;
        private AotList loopRoutines = new AotList();

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(StartSequence));
            breakIn = ControlInput("break", new Action<Flow>(BreakSequence));
            condition = ValueInput<bool>("condition");

            entered = ControlOutput("entered");
            loop = ControlOutput("loop");
            exit = ControlOutput("exit");
            breakOut = ControlOutput("broken");

            Relation(enter, entered);
            Relation(enter, loop);
            Relation(enter, exit);
            Relation(breakIn, breakOut);
        }

        public void StartSequence(Flow flow)
        {
            if (coroutineInstances == null)
            {
                coroutineInstances = new AotList();
            }

            if (loopRoutines == null)
            {
                loopRoutines = new AotList();
            }

            if (!isRunning || async)
            {
                isRunning = true;

                coroutineInstance = CoroutineRunner.instance;
                loopRoutine = coroutineInstance.ConvertTo<CoroutineRunner>().StartCoroutine(Loop());
                coroutineInstances.Add(coroutineInstance);
                loopRoutines.Add(loopRoutine);

                Flow _flow = Flow.New();
                _flow.Invoke(entered);
                _flow.Dispose();
            }
        }

        public void BreakSequence(Flow flow)
        {
            if (coroutineInstances != null)
            {
                destroyed = true;

                Flow _flow = Flow.New();
                _flow.Invoke(breakOut);
                _flow.Dispose();
            }

        }

        public IEnumerator Loop()
        {
            while (condition.GetValue<bool>())
            {
                if (!destroyed)
                {
                    Flow _loop = Flow.New();
                    _loop.Invoke(loop);
                    _loop.Dispose();
                }

                yield return null;
            }

            isRunning = false;
            Flow _exit = Flow.New();

            _exit.Invoke(exit);
            _exit.Dispose();
        }

        public void OnEnable()
        {
        }

        public void Start()
        {
        }

        public void OnDisable()
        {
            destroyed = true;

            for (int i = 0; i < coroutineInstances.Count; i++)
            {
                CoroutineRunner corunner = (CoroutineRunner)coroutineInstances[i];
                corunner.StopCoroutine((Coroutine)loopRoutines[i]);
            }

            isRunning = false;
        }

        public void OnDestroy()
        {
            destroyed = true;

            for (int i = 0; i < coroutineInstances.Count; i++)
            {
                CoroutineRunner corunner = (CoroutineRunner)coroutineInstances[i];
                corunner.StopCoroutine((Coroutine)loopRoutines[i]);
            }

            isRunning = false;
        }

        
    }
}

