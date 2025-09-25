namespace Unity.VisualScripting.Community
{
    public class ChangeDetectLogic
    {
        private object previousValue;
        public bool Changed(object value)
        {
            if (!OperatorUtility.Equal(previousValue, value))
            {
                previousValue = value;
                return true;
            }
            return false;
        }

        public object PreviousValue => previousValue;
    }
}