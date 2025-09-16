using System;
using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class WaitForFlowLogic
    {
        private readonly bool[] inputsActivated;

        public bool ResetOnExit { get; private set; }

        public int InputCount => inputsActivated.Length;

        public WaitForFlowLogic(int inputCount, bool resetOnExit = false)
        {
            if (inputCount < 2)
                throw new ArgumentException("WaitForFlowLogic requires at least 2 inputs.");

            inputsActivated = new bool[inputCount];
            ResetOnExit = resetOnExit;
        }

        public bool Enter(int index)
        {
            inputsActivated[index] = true;

            if (CheckActivated())
            {
                if (ResetOnExit)
                    Reset();

                return true;
            }

            return false;
        }

        public void Reset()
        {
            for (int i = 0; i < inputsActivated.Length; i++)
                inputsActivated[i] = false;
        }

        private bool CheckActivated()
        {
            for (int i = 0; i < inputsActivated.Length; i++)
            {
                if (!inputsActivated[i])
                    return false;
            }
            return true;
        }
    }
}
