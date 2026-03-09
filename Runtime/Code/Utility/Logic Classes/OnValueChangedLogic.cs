using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class OnValueChangedLogic
    {
        public OnValueChangedLogic() { }
        public OnValueChangedLogic(bool provideInitial = false)
        {
            this.provideInitial = provideInitial;
        }

        private object previousValue;
        private bool firstExecution = true;

        public bool provideInitial;

        public bool HasValueChanged(object newValue, out object result, object initalValue = null)
        {
            if (firstExecution)
            {
                firstExecution = false;
                previousValue = provideInitial ? initalValue : newValue;
            }

            if (!OperatorUtility.Equal(previousValue, newValue))
            {
                previousValue = newValue;
                result = newValue;
                return true;
            }
            result = null;
            return false;
        }
    }
}
