using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ToggleFlowLogic
    {
        public bool isOn { get; private set; }
        private bool initialized;
        private bool _startOn;
        // Used by the code generator to allow initialization via object initializer syntax.
        [GenerateProperty]
        public bool startOn
        {
            get => _startOn;
            set
            {
                if (!initialized)
                {
                    _startOn = value;
                    isOn = value;
                    initialized = true;
                }
            }
        }
        private Action TurnedOn;
        private Action TurnedOff;

        public void Initialize(Action turnedOn = null, Action turnedOff = null)
        {
            TurnedOn = turnedOn;
            TurnedOff = turnedOff;
        }

        public void Toggle()
        {
            isOn = !isOn;
            if (isOn) TurnedOn?.Invoke();
            else TurnedOff?.Invoke();
        }

        public void TurnOn()
        {
            if (isOn) return;
            isOn = true;
            TurnedOn?.Invoke();
        }

        public void TurnOff()
        {
            if (!isOn) return;
            isOn = false;
            TurnedOff?.Invoke();
        }
    }
}