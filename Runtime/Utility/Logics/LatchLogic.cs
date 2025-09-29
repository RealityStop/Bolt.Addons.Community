namespace Unity.VisualScripting.Community
{
    public class LatchLogic
    {
        private bool _isSet = false;

        public void Update(bool set, bool reset, bool resetDominant)
        {
            if (set)
            {
                if (reset)
                {
                    _isSet = !resetDominant;
                }
                else
                {
                    _isSet = true;
                }
            }
            else if (reset)
            {
                _isSet = false;
            }
        }

        public bool Value => _isSet;
    }
}