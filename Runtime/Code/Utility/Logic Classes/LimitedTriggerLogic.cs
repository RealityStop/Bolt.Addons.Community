namespace Unity.VisualScripting.Community
{
    public class LimitedTriggerLogic
    {
        private int triggers = 0;
        private int maxTriggers;

        private bool isInitialized = false;

        public void Initialize(int maxTriggers)
        {
            if (isInitialized && this.maxTriggers == maxTriggers) return;
            isInitialized = true;
            this.maxTriggers = maxTriggers;
        }

        public bool Trigger()
        {
            return triggers++ <= maxTriggers;
        }

        public void Reset()
        {
            triggers = 0;
        }
    }
}