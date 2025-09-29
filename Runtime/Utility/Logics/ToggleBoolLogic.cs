namespace Unity.VisualScripting.Community
{
    public class ToggleBoolLogic
    {
        private bool value;

        private bool isInitial = true;

        public bool ToggleBool(bool initialValue)
        {
            if (isInitial)
            {
                value = !initialValue;
                isInitial = false;
                return value;
            }
            value = !value;
            return value;
        }
    }
}